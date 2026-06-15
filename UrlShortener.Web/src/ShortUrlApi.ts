// methods for calling the ShortUrl API

export interface CreateShortUrlRequest {
    originalUrl: string;
}

export interface ShortUrlResponse {
    id: string, // long Id from backend
    originalUrl: string,
    shortUrl: string,
    expiresAt: Date
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export async function createShortUrl(request: CreateShortUrlRequest): Promise<ShortUrlResponse> {
    // return new Promise((resolve) => {
    //     setTimeout(() => {
    //         resolve({
    //             id: "123",
    //             shortUrl: "http://short.url/abc123",
    //             originalUrl: "https://www.google.com",
    //             expiresAt: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000) // 3 days
    //         })
    //     }, 3000);
    // });
    const response = await fetch(API_BASE_URL, {
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
    return { ...data, expiresAt: new Date(data.expiresAt) };
}

