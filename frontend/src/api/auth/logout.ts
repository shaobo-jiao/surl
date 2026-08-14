import { API_BASE_URL, GeneralFailureResult, type ProblemDetails, type Result } from "../shared";


export async function logout(): Promise<Result<null, ProblemDetails>> {
  try {
    const httpResponse = await fetch(new URL("/auth/logout", API_BASE_URL), {
      method: "POST",
      credentials: "include"
    });

    return httpResponse.ok
      ? { success: true, data: null }
      : { success: false, error: await httpResponse.json() };
  }
  catch {
    return GeneralFailureResult;
  }
}