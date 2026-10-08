import axios from "axios";
import apiClient from "../apiClient";
import { GeneralFailureResult, type ProblemDetails, type Result } from "../shared";

export interface ChangePasswordRequest {
  oldPassword: string,
  newPassword: string
}

export type ChangePasswordFields = "oldPassword" | "newPassword";

/**
 * Changes the password of the currently authenticated user.
 * Requires a valid access token; apiClient injects it and refreshes on 401.
 *
 * On success the backend revokes all refresh tokens, so the current session
 * ends once the access token expires.
 */
export async function changePasswordApi(request: ChangePasswordRequest)
  : Promise<Result<null, ProblemDetails<ChangePasswordFields>>> {
  try {
    await apiClient.post("/auth/change-password", request);
    return { success: true, data: null };
  } catch (error) {
    if (!axios.isAxiosError(error) || !error.response)
      return GeneralFailureResult;
    return { success: false, error: error.response.data };
  }
}
