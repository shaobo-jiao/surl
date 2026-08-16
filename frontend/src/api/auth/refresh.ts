

import { API_BASE_URL, GeneralFailureResult, type ProblemDetails, type Result } from "../shared";

interface RefreshResponse {
  accessToken: string,
}

export async function refresh(): Promise<Result<RefreshResponse, ProblemDetails>> {
  // request: empty body, refresh token sent in cookie;
  // response: new access token in body, new refresh token received in cookie
  try {
    const httpResponse = await fetch(new URL("/auth/refresh", API_BASE_URL), {
      method: "POST",
      credentials: "include"
    });

    if (httpResponse.ok) {
      return { success: true, data: await httpResponse.json() };
    }
    else {
      return { success: false, error: await httpResponse.json() };
    }
  }
  catch {
    return GeneralFailureResult;
  }
}