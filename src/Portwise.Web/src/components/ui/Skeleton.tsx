import type { HTMLAttributes } from "react"

import { cn } from "@/shared/utils/utils"

export function Skeleton({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div aria-hidden="true" className={cn("skeleton", className)} {...props} />
}
