import { API_BASE_URL } from "../shared";

export async function getRedirectionUrlApi(shortUrlCode: string): Promise<string> {
  const response = await fetch(new URL(`redirection/${shortUrlCode}`, API_BASE_URL));
  if (!response.ok)
    throw new Error("The link is either invalid or expired.");

  return await response.json();;
}