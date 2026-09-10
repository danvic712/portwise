import { mkdtempSync, readFileSync, rmSync } from "node:fs"
import { tmpdir } from "node:os"
import { dirname, join, resolve } from "node:path"
import { spawnSync } from "node:child_process"

const outputDirectory = mkdtempSync(join(tmpdir(), "portwise-openapi-"))
const generatedPath = join(outputDirectory, "api-contract.generated.ts")
const sourcePath = resolve("openapi/portwise_v1.json")
const checkedInPath = resolve("src/lib/api-contract.generated.ts")
const packageRunner = process.platform === "win32" ? "pnpm.cmd" : "pnpm"

try {
  const openapiDocument = JSON.parse(readFileSync(sourcePath, "utf8"))
  const httpMethods = new Set(["get", "post", "put", "patch", "delete", "head", "options", "trace"])
  const operations = Object.values(openapiDocument.paths ?? {})
    .flatMap((pathItem) => Object.entries(pathItem)
      .filter(([method]) => httpMethods.has(method))
      .map(([method, operation]) => ({ method, operation })))
    .filter(({ operation }) => operation && typeof operation === "object")
  const missingOperationSummaries = operations
    .filter(({ operation }) => !String(operation.summary ?? "").trim())
  const missingParameterDescriptions = operations.flatMap(({ method, operation }) =>
    (operation.parameters ?? [])
      .filter((parameter) => parameter && typeof parameter === "object" && !parameter.$ref)
      .filter((parameter) => !String(parameter.description ?? "").trim())
      .map((parameter) => `${method.toUpperCase()} ${parameter.name ?? "<unnamed>"}`),
  )
  const missingRequestBodyDescriptions = operations
    .filter(({ operation }) => operation.requestBody && !operation.requestBody.$ref)
    .filter(({ operation }) => !String(operation.requestBody.description ?? "").trim())
  const missingResponseDescriptions = operations.flatMap(({ method, operation }) =>
    Object.entries(operation.responses ?? {})
      .filter(([, response]) => response && typeof response === "object" && !response.$ref)
      .filter(([, response]) => !String(response.description ?? "").trim())
      .map(([status]) => `${method.toUpperCase()} ${status}`),
  )
  const missingSchemaDescriptions = Object.values(openapiDocument.components?.schemas ?? {})
    .filter((schema) => schema && typeof schema === "object" && !schema.$ref)
    .filter((schema) => !String(schema.description ?? "").trim())

  if (
    missingOperationSummaries.length > 0
    || missingParameterDescriptions.length > 0
    || missingRequestBodyDescriptions.length > 0
    || missingResponseDescriptions.length > 0
    || missingSchemaDescriptions.length > 0
  ) {
    console.error(
      `OpenAPI documentation is incomplete: ${missingOperationSummaries.length} operation summaries, ${missingParameterDescriptions.length} parameter descriptions, ${missingRequestBodyDescriptions.length} request body descriptions, ${missingResponseDescriptions.length} response descriptions and ${missingSchemaDescriptions.length} schema descriptions are missing.`,
    )
    process.exitCode = 1
  }

  const result = spawnSync(
    packageRunner,
    ["exec", "openapi-typescript", sourcePath, "-o", generatedPath],
    { stdio: "inherit" },
  )

  if (result.status !== 0) {
    process.exitCode = result.status ?? 1
  } else if (readFileSync(generatedPath, "utf8") !== readFileSync(checkedInPath, "utf8")) {
    console.error(`Generated API contract is stale: ${dirname(checkedInPath)}`)
    console.error("Run pnpm api:generate and commit the resulting contract.")
    process.exitCode = 1
  }
} finally {
  rmSync(outputDirectory, { recursive: true, force: true })
}
