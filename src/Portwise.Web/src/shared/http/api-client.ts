import axios from "axios"

import { apiPath, normalizeWireNumerics, type ApiDeletePath, type ApiGetPath, type ApiPathParameters, type ApiPostPath, type ApiPutPath, type ApiResponse } from "@/shared/http/api-contract"

export const apiClient = axios.create({
  baseURL: "/api/v1",
  timeout: 20_000,
  headers: {
    Accept: "application/json",
    "Content-Type": "application/json",
  },
})

apiClient.interceptors.request.use((config) => {
  const locale = localStorage.getItem("portwise-locale") ?? "zh-CN"
  config.headers.set("Accept-Language", locale)
  return config
})

apiClient.interceptors.response.use((response) => {
  response.data = normalizeWireNumerics(response.data)
  return response
})

export async function apiGet<P extends ApiGetPath>(
  path: P,
  pathParameters: ApiPathParameters<P> = {} as ApiPathParameters<P>,
  config?: Parameters<typeof apiClient.get<ApiResponse<P, "get">>>[1],
) {
  const response = await apiClient.get<ApiResponse<P, "get">>(apiPath(path, pathParameters), config)
  return response.data
}

export async function apiPost<P extends ApiPostPath, Body = unknown>(
  path: P,
  body?: Body,
  pathParameters: ApiPathParameters<P> = {} as ApiPathParameters<P>,
  config?: Parameters<typeof apiClient.post<ApiResponse<P, "post">>>[2],
) {
  const response = await apiClient.post<ApiResponse<P, "post">>(apiPath(path, pathParameters), body, config)
  return response.data
}

export async function apiPut<P extends ApiPutPath, Body = unknown>(
  path: P,
  body?: Body,
  pathParameters: ApiPathParameters<P> = {} as ApiPathParameters<P>,
  config?: Parameters<typeof apiClient.put<ApiResponse<P, "put">>>[2],
) {
  const response = await apiClient.put<ApiResponse<P, "put">>(apiPath(path, pathParameters), body, config)
  return response.data
}

export async function apiDelete<P extends ApiDeletePath>(
  path: P,
  pathParameters: ApiPathParameters<P> = {} as ApiPathParameters<P>,
  config?: Parameters<typeof apiClient.delete<ApiResponse<P, "delete">>>[1],
) {
  const response = await apiClient.delete<ApiResponse<P, "delete">>(apiPath(path, pathParameters), config)
  return response.data
}
