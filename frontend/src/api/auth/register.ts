import { API_BASE_URL, GeneralFailureResult, type ProblemDetails, type Result } from "../shared";

export interface RegisterRequest {
  username: string,
  password: string,
  email?: string
}

export interface RegisterResponse {
  id: number,
  username: string,
  email: string,
  createdAt: string
}

export type RegisterFields = "username" | "email" | "password";

export async function registerApi(request: RegisterRequest)
  : Promise<Result<RegisterResponse, ProblemDetails<RegisterFields>>> {
  try {
    // call register api
    const httpResponse = await fetch(new URL("/auth/register", API_BASE_URL), {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });

    if (httpResponse.ok) {
      // success with data
      return { success: true, data: await httpResponse.json() };
    }
    else {
      // failure with error
      return { success: false, error: await httpResponse.json() };
    }
  } catch (e) {
    // unexpected error happened;
    return GeneralFailureResult;
  }
}