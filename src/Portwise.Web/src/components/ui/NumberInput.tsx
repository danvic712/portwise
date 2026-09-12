import type { ComponentProps } from "react"

import { Input } from "@/components/ui/Input"
import { cn } from "@/shared/utils/utils"

type NumberInputProps = Omit<ComponentProps<typeof Input>, "type"> & {
  inputMode?: "decimal" | "numeric"
}

/**
 * A numeric input that keeps the shared shadcn input treatment.
 */
function NumberInput({ className, inputMode = "numeric", ...props }: NumberInputProps) {
  return <Input {...props} type="number" inputMode={inputMode} className={cn("[appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none", className)} />
}

export { NumberInput }
