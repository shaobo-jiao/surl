import z from "zod";

/**
 * Normal password scheme. For logging with existing passwords
 */
export const passwordSchema = z
  .string()
  .min(6)
  .max(50)
  .regex(/^\S+$/, "Password must not contain whitespaces.");

/**
 * Strong password scheme: for settings new passwords
 */
export const strongPasswordSchema = z
  .string()
  .min(12)
  .max(50)
  .regex(/[A-Z]/, "Password must contain an uppercase letter.")
  .regex(/[a-z]/, "Password must contain a lowercase letter.")
  .regex(/[0-9]/, "Password must contain a number.")
  .regex(/[^a-zA-Z0-9]/, "Password must contain a special character.")
  .regex(/^\S+$/, "Password must not contain whitespaces.");
