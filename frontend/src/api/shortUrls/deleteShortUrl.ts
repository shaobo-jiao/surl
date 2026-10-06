import axios from "axios";
import apiClient from "../apiClient";
import { GeneralFailureResult, type ProblemDetails, type Result } from "../shared";

/**
 * Deletes a short URL owned by the authenticated user.
 * The backend returns 204 No Content on success.
 */
export async function deleteShortUrlApi(id: string)
  : Promise<Result<null, ProblemDetails>> {
  try {
    await apiClient.delete(`short-urls/${id}`);
    return { success: true, data: null };
  } catch (error) {
    if (!axios.isAxiosError(error) || !error.response)
      return GeneralFailureResult;
    return { success: false, error: error.response.data };
  }
}
