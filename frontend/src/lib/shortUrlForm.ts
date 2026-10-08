import z from "zod";

/**
 * Validates that a value is a non-empty, well-formed http(s) URL
 */
export const originalUrlSchema = z
  .string()
  .trim()
  .nonempty("Please enter a URL to shorten.")
  .refine((value) => {
    try {
      // the URL must be valid and use http or https
      const url = new URL(value);
      return url.protocol === "http:" || url.protocol === "https:";
    } catch {
      return false;
    }
  }, "The URL is invalid");
