
import axios from "axios";
import apiClient from "../apiClient";
import { GeneralFailureResult, type ProblemDetails, type Result } from "../shared";

export interface MeResponse {
  id: number,
  username: string,
  email: string | null,
  createdAt: string
}

/**
 * Gets the currently authenticated user.
 * Requires a valid access token; apiClient injects it and refreshes on 401.
 */
export async function meApi(): Promise<Result<MeResponse, ProblemDetails>> {
  try {
    const { data } = await apiClient.get("/auth/me");
    return { success: true, data: data as MeResponse };
  } catch (error) {
    if (!axios.isAxiosError(error) || !error.response)
      return GeneralFailureResult;
    return { success: false, error: error.response.data };
  }
}
