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

export type { components, paths } from "@/lib/api-contract.generated"
