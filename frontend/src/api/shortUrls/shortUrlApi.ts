// methods for calling the ShortUrl API

export interface CreateShortUrlRequest {
  originalUrl: string;
}

export interface ShortUrlResponse {
  id: string, // long Id from backend
  originalUrl: string,
  shortUrlCode: string,
  expiresAt: string,
  clickCount: number
}

export interface ShortUrl {
  id: string,
  originalUrl: string,
  code: string,
  absoluteUrl: string,
  expiresAt: Date,
  clickCount: number
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export async function createShortUrl(request: CreateShortUrlRequest): Promise<ShortUrl> {
  const response = await fetch(new URL("short-urls/", API_BASE_URL), {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(request)
  });

  // backend should return OK only, otherwise there's error;
  if (!response.ok)
    throw new Error(`Failed to create short URL: ${response.statusText}`);

  const data = await response.json() as ShortUrlResponse;
  const shortUrl: ShortUrl = {
    id: data.id,
    originalUrl: data.originalUrl,
    code: data.shortUrlCode,
    absoluteUrl: new URL(data.shortUrlCode, import.meta.env.VITE_APP_URL).toString(),
    expiresAt: new Date(data.expiresAt),
    clickCount: data.clickCount,
  }
  return shortUrl;
}

export async function getOriginalUrl(shortUrlCode: string): Promise<string> {
  const response = await fetch(new URL(`redirection/${shortUrlCode}`, API_BASE_URL));
  if (!response.ok)
    throw new Error("The link is either invalid or expired.");

  return await response.json();;
}
