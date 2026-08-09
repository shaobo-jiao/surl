
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// defines API error responses. the type parameter F should be 
// a union of string literals for validation fields.
export interface ProblemDetails<F extends string> {
  status: number,
  title: string,
  detail: string,
  code: string,
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

export type Result<T, E> =  SuccessResult<T> | FailureResult<E>;


export const GeneralFailureResult = {
  success: false,
  error: {
    status: 400,
    title: "General Error",
    detail: "There is an error happened",
    code: "general_error"
  }
} as const