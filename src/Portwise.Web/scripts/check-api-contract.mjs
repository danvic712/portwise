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
