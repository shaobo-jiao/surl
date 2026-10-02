import { API_BASE_URL, APP_URL, GeneralFailureResult, type ProblemDetails, type Result } from "../shared";

// shortUrl API related methods and types 

export interface ShortUrl {
  id: string,
  originalUrl: string,
  code: string,
  absoluteUrl: string,
  expiresAt: Date,
  clickCount: number
}

export interface CreateShortUrlRequest {
  originalUrl: string,
  expiresAt?: Date,
}

export interface ShortUrlResponse {
  id: string, // long Id from backend
  originalUrl: string,
  shortUrlCode: string,
  expiresAt: string,
  clickCount: number
}

export type CreateShortUrlFields = "originalUrl" | "expiresAt";

// converts the backend response into the ShortUrl used across the frontend;
function toShortUrl(data: ShortUrlResponse): ShortUrl {
  return {
    id: data.id,
    originalUrl: data.originalUrl,
    code: data.shortUrlCode,
    absoluteUrl: new URL(data.shortUrlCode, APP_URL).toString(),
    expiresAt: new Date(data.expiresAt),
    clickCount: data.clickCount,
  }
}

/**
 * Creates a short URL linked to the authenticated user, using the given access token.
 */
export async function createShortUrl(request: CreateShortUrlRequest, accessToken: string)
  : Promise<Result<ShortUrl, ProblemDetails<CreateShortUrlFields>>> {
  try {
    const response = await fetch(new URL("short-urls/", API_BASE_URL), {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${accessToken}`
      },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      // backend returns problem details, including field validation errors
      return { success: false, error: await response.json() };
    }

    const data = await response.json() as ShortUrlResponse;
    return { success: true, data: toShortUrl(data) };
  } catch {
    // unexpected error happened;
    return GeneralFailureResult;
  }
}

/**
 * Creates an anonymous short URL that isn't linked to any user.
 * Used by the "Try It Out" form on the home page.
 */
export async function createAnonymousShortUrl(request: CreateShortUrlRequest)
  : Promise<Result<ShortUrl, ProblemDetails<CreateShortUrlFields>>> {
  try {
    const response = await fetch(new URL("short-urls/", API_BASE_URL), {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      // backend returns problem details, including field validation errors
      return { success: false, error: await response.json() };
    }

    const data = await response.json() as ShortUrlResponse;
    return { success: true, data: toShortUrl(data) };
  } catch {
    // unexpected error happened;
    return GeneralFailureResult;
  }
}

export async function getOriginalUrl(shortUrlCode: string): Promise<string> {
  const response = await fetch(new URL(`redirection/${shortUrlCode}`, API_BASE_URL));
  if (!response.ok)
    throw new Error("The link is either invalid or expired.");

  return await response.json();;
}
