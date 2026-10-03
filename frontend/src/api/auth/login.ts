import { API_BASE_URL, GeneralFailureResult, type ProblemDetails, type Result } from "../shared";

export interface LoginRequest {
  username: string,
  password: string
}

// access token in memory, refresh token in cookie
export interface LoginResponse {
  accessToken: string
}

export type LoginFields = "username" | "password";

export async function loginApi(request: LoginRequest)
  : Promise<Result<LoginResponse, ProblemDetails<LoginFields>>> {
    try {
    // call login api
    const httpResponse = await fetch(new URL("/auth/login", API_BASE_URL), {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      credentials: "include", 
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

