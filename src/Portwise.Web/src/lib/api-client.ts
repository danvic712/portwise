import axios from "axios"

import { apiPath, normalizeWireNumerics, type ApiGetPath, type ApiPathParameters, type ApiPostPath } from "@/lib/api-contract"

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

export async function apiGet<T, P extends ApiGetPath = ApiGetPath>(
  path: P,
  pathParameters: ApiPathParameters = {},
  config?: Parameters<typeof apiClient.get<T>>[1],
) {
  const response = await apiClient.get<T>(apiPath(path, pathParameters), config)
  return response.data
}

export async function apiPost<T, P extends ApiPostPath = ApiPostPath, Body = unknown>(
  path: P,
  body?: Body,
  pathParameters: ApiPathParameters = {},
  config?: Parameters<typeof apiClient.post<T>>[2],
) {
  const response = await apiClient.post<T>(apiPath(path, pathParameters), body, config)
  return response.data
}
