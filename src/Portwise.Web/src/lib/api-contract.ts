import type { components, paths } from "@/lib/api-contract.generated"

type IsWireNumeric<T> = [number] extends [T]
  ? [string] extends [T]
    ? true
    : false
  : false

type Normalize<T> = IsWireNumeric<Exclude<T, null | undefined>> extends true
  ? number
    | (null extends T ? null : never)
    | (undefined extends T ? undefined : never)
  : T extends readonly (infer U)[]
    ? Normalize<U>[]
    : T extends Record<string, never>
      ? Record<string, unknown>
      : T extends object
        ? { [K in keyof T]: Normalize<T[K]> }
        : T

export type ApiSchemas = components["schemas"]

/**
 * Frontend transport models are projected from the generated OpenAPI contract.
 * Numeric JSON values are normalized to the runtime number shape used by the UI.
 */
export type ApiSchema<K extends keyof ApiSchemas> = Normalize<ApiSchemas[K]>

export type ApiPath = keyof paths

type HttpMethod = "get" | "post" | "put" | "delete" | "patch"
type PathsWithMethod<Method extends HttpMethod> = {
  [Path in ApiPath]: paths[Path][Method] extends never ? never : Path
}[ApiPath]

export type ApiGetPath = PathsWithMethod<"get">
export type ApiPostPath = PathsWithMethod<"post">
export type ApiPathParameters = Record<string, string | number>

/**
 * Expands a generated OpenAPI path and returns the relative path expected by
 * the Axios client. The template is checked against generated paths, so a
 * removed or renamed backend route fails the frontend typecheck.
 */
export function apiPath<P extends ApiPath>(
  template: P,
  parameters: ApiPathParameters = {},
) {
  const expanded = Object.entries(parameters).reduce(
    (path, [name, value]) => path.replace(`{${name}}`, encodeURIComponent(String(value))),
    template as string,
  )

  return expanded.replace(/^\/api\/v1/, "")
}

const numericPropertyPattern = /(?:amount|count|shares|quantity|price|ratio|yield|pershare|threshold|weight|fee|earningsperShare|returnOnEquity)$/i

/**
 * Converts numeric strings emitted by JSON number serializers to numbers while
 * leaving identifiers, codes, dates and arbitrary text untouched.
 */
export function normalizeWireNumerics<T>(value: T): T {
  if (Array.isArray(value)) {
    return value.map((item) => normalizeWireNumerics(item)) as T
  }

  if (value === null || typeof value !== "object") {
    return value
  }

  const normalized = Object.fromEntries(
    Object.entries(value).map(([key, item]) => {
      if (numericPropertyPattern.test(key) && typeof item === "string" && item.trim() !== "") {
        const numericValue = Number(item)
        if (Number.isFinite(numericValue)) return [key, numericValue]
      }

      return [key, normalizeWireNumerics(item)]
    }),
  )

  return normalized as T
}

export type { components, paths } from "@/lib/api-contract.generated"
