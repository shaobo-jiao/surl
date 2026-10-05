import axios from "axios";
import { API_BASE_URL } from "../shared";

export interface RefreshResponse {
  accessToken: string,
}

/**
 * Rotates the refresh token and returns a new access token.
 *
 * request: empty body, refresh token sent in the HttpOnly cookie;
 * response: new access token in body, new refresh token set in the cookie.
 *
 * bare axios instance is used to make API calls here, as it throws errors in the 
 * same shape as errors in the apiClient; When there's error, the error will 
 * just be thrown/ There's no wrapping around ProblemDetails
 */
export async function refreshApi(): Promise<RefreshResponse> {
  const { data } = await axios.post<RefreshResponse>("/auth/refresh", null, {
    baseURL: API_BASE_URL,
    withCredentials: true,
  });
  return data;
}
