import { readdirSync } from "node:fs"
import { join, relative } from "node:path"
import { spawnSync } from "node:child_process"

function findTests(directory) {
  return readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const path = join(directory, entry.name)
    if (entry.isDirectory()) return findTests(path)
    return entry.isFile() && entry.name.endsWith(".test.ts") ? [relative(process.cwd(), path)] : []
  })
}

const testFiles = findTests(join(process.cwd(), "tests")).sort()
if (testFiles.length === 0) {
  console.error("No TypeScript test files were found under tests/")
  process.exit(1)
}

const result = spawnSync(process.execPath, ["--experimental-strip-types", "--test", ...testFiles], { stdio: "inherit" })
process.exit(result.status ?? 1)
