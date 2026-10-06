import { APP_URL } from "../shared"

export interface ShortUrl {
  id: string,
  originalUrl: string,
  code: string,
  absoluteUrl: string,
  expiresAt: Date | null,
  clickCount: number
}

export interface ShortUrlResponse {
  id: string, // long Id from backend
  originalUrl: string,
  shortUrlCode: string,
  expiresAt: string | null,
  clickCount: number
}


// converts the backend response into the ShortUrl used across the frontend;
export function toShortUrl(data: ShortUrlResponse): ShortUrl {
  return {
    id: data.id,
    originalUrl: data.originalUrl,
    code: data.shortUrlCode,
    absoluteUrl: new URL(data.shortUrlCode, APP_URL).toString(),
    expiresAt: data.expiresAt ? new Date(data.expiresAt) : null,
    clickCount: data.clickCount,
  }
}
