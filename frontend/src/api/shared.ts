
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// defines API error responses. the type parameter F should be 
// a union of string literals for validation fields.
export interface ProblemDetails<F extends string = never> {
  status: number,
  title: string,
  detail: string,
  code?: string,
  errors?: Partial<Record<F, string[]>>
}

interface SuccessResult<T> {
  success: true,
  data: T
}

interface FailureResult<E> {
  success: false,
  error: E
}

export type Result<T, E> = SuccessResult<T> | FailureResult<E>;


export const GeneralFailureResult = {
  success: false,
  error: {
    status: 400,
    title: "Something went wrong.",
    detail: "There's something wrong with the application. Please try again later.",
    code: "general_error"
  }
} as const