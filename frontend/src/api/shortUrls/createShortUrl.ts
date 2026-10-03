import axios from "axios";
import apiClient from "../apiClient";
import { API_BASE_URL, GeneralFailureResult, type ProblemDetails, type Result } from "../shared";
import { toShortUrl, type ShortUrl, type ShortUrlResponse } from "./shortUrl.shared";

export interface CreateShortUrlRequest {
  originalUrl: string,
  expiresAt?: Date,
}

export type CreateShortUrlFields = "originalUrl" | "expiresAt";

/**
 * Creates a short URL linked to the authenticated user
 */
export async function createShortUrlApi(request: CreateShortUrlRequest)
  : Promise<Result<ShortUrl, ProblemDetails<CreateShortUrlFields>>> {
  try {
    const { data } = await apiClient.post("short-urls/", request);
    return { success: true, data: toShortUrl(data as ShortUrlResponse) }
  } catch (error) {
    if (!axios.isAxiosError(error) || !error.response)
      return GeneralFailureResult;
    return { success: false, error: error.response.data };
  }
}


/**
 * Creates an anonymous short URL that isn't linked to any user.
 * Used by the "Try It Out" form on the home page.
 */
export async function createAnonymousShortUrlApi(request: CreateShortUrlRequest)
  : Promise<Result<ShortUrl, ProblemDetails<CreateShortUrlFields>>> {
  try {
    const { data } = await axios.post("short-urls/anonymous", request, { baseURL: API_BASE_URL });
    return { success: true, data: toShortUrl(data) }
  } catch (error) {
    if (!axios.isAxiosError(error) || !error.response)
      return GeneralFailureResult;
    return { success: false, error: error.response.data };
  }
}
