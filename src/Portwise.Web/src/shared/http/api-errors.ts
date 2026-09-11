import axios from "axios"

export type ProblemDetails = {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  errorCode?: string
  error_code?: string
  errors?: Record<string, string[]>
}

export type ApiErrorMessages = {
  timeout: string
  stockModelParametersMissing: string
}

export function getApiErrorMessage(error: unknown, fallback: string, messages?: Partial<ApiErrorMessages>) {
  if (axios.isAxiosError<ProblemDetails>(error)) {
    const data = error.response?.data
    const errorCode = data?.errorCode ?? data?.error_code
    if (data?.detail) {
      return data.detail
    }

    if (data?.errors) {
      const firstError = Object.values(data.errors).flat()[0]
      if (firstError) {
        return firstError
      }
    }

    if (error.code === "ECONNABORTED") {
      return messages?.timeout ?? fallback
    }

    if (errorCode === "stock_model_parameters_not_found") {
      return messages?.stockModelParametersMissing ?? fallback
    }
  }

  return fallback
}
