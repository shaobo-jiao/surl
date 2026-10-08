import axios from "axios";
import apiClient from "../apiClient";
import { GeneralFailureResult, type ProblemDetails, type Result } from "../shared";
import { toShortUrl, type ShortUrl, type ShortUrlResponse } from "./shortUrl.shared";

export interface UpdateShortUrlRequest {
  // null clears the expiry so the short URL never expires
  expiresAt?: Date | null,
}

export type UpdateShortUrlFields = "expiresAt";

/**
 * Updates a short URL owned by the authenticated user.
 * Currently only the expiry date can be changed.
 */
export async function updateShortUrlApi(id: string, request: UpdateShortUrlRequest)
  : Promise<Result<ShortUrl, ProblemDetails<UpdateShortUrlFields>>> {
  try {
    const { data } = await apiClient.patch(`short-urls/${id}`, request);
    return { success: true, data: toShortUrl(data as ShortUrlResponse) }
  } catch (error) {
    if (!axios.isAxiosError(error) || !error.response)
      return GeneralFailureResult;
    return { success: false, error: error.response.data };
  }
}
