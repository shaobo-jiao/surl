// API Client handles API calls to authenticated endpoints.
// handles access token auto refresh using axios interceptors

// for public endpoints, use fetch or default axios instance
// for authenticated endpoints, use the apiClient defined in this file

// apiClient logic
// defines: 
// - access token, 
// - isRefreshing: whether it's refreshing access token now 
// - failedQueue: stores 2nd and following refresh requests to avoid duplicate refresh requests
// - expiryBuffer: how long to trigger refresh before access token expires

// interceptors:
// - request interceptors: check if access token is expiring and trigger refresh before sending the request out
// - response interceptors: catch 401 and trigger refresh; handle duplicate refresh requests

import axios from "axios";
import { API_BASE_URL } from "./shared";
import { refreshApi } from "./auth/refresh";

// the API client for the app to make API calls to authenticated endpoints
const apiClient = axios.create({
  baseURL: API_BASE_URL
});

let accessToken: string | null = null;
export function setAccessToken(token: string | null) {
  accessToken = token;
}

// for AuthProvider to update state when auth status changes, e.g. due to refresh failure
let onAuthSuccess: (() => void) | null = null;
let onAuthFailure: (() => void) | null = null;
export function registerAuthHandlers(success: () => void, failure: () => void) {
  onAuthSuccess = success;
  onAuthFailure = failure;
}

// handles duplicate refresh requests
let isRefreshing = false;
let failedQueue: {
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}[] = [];

// to notify duplicate refresh requests when 1st refresh request completes,
function processQueue(token: string | null, error: unknown) {
  failedQueue.forEach(({ resolve, reject }) => {
    token ? resolve(token) : reject(error)
  });
  failedQueue = [];
}

// request interceptors: handles injecting access token and refresh in advance
apiClient.interceptors.request.use(async (config) => {
    try {
      // NOTE: we can check if access token is expiring and do preemptive refresh
      // this requires the backend to return the access token expiry date together with the token
      // currently this is not implemented as the response interceptor always catches

      // inject access token
      if (accessToken) 
        config.headers.Authorization = `Bearer ${accessToken}`;
    } catch (error) {
      console.warn("apiClient access token failure", error);
    }
    return config;
  }
)

// response interceptor: handles token refresh 
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    // if normal non 401 errors, just return 
    // if for any reason, we encountered 401 again after refresh, just reject 
    if (!error.response || error.response.status !== 401 || originalRequest._retry) {
      return Promise.reject(error);
    }
    
    // handle 401 error due to expired access token
    if (isRefreshing) {
      // already refreshing, enqueue future refresh requests and return promises
      // promises will be fulfilled or rejected when the 1st refresh request completes
      // upon fulfillment, the original request is retried
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      })
        .then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return apiClient.request(originalRequest);
        });
    }

    // process refresh request
    isRefreshing = true;
    originalRequest._retry = true;

    try {
      // upon successful refresh, update access token, fulfill failedQueue and retry the request
      const { accessToken: newAccessToken } = await refreshApi();
      setAccessToken(newAccessToken);
      onAuthSuccess?.();

      processQueue(newAccessToken, null);
      originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
      return apiClient.request(originalRequest);
    } catch (refreshError) {
      // refreshApi rejects with an AxiosError (the ProblemDetails is in
      // error.response.data), so callers get the same shape as any other
      // failed request, and queued requests are rejected the same way
      setAccessToken(null);
      onAuthFailure?.();

      processQueue(null, refreshError);
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  }
);

export default apiClient;
