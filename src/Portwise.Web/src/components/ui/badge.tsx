import { mergeProps } from "@base-ui/react/merge-props"
import { useRender } from "@base-ui/react/use-render"
import type { VariantProps } from "class-variance-authority"

import { cn } from "@/lib/utils"
import { badgeVariants } from "@/components/ui/badge-variants"

function Badge({
  className,
  variant = "default",
  render,
  ...props
}: React.ComponentProps<"span"> &
  VariantProps<typeof badgeVariants> & { render?: React.ReactElement }) {
  return useRender({
    defaultTagName: "span",
    props: mergeProps<"span">(
      { className: cn(badgeVariants({ variant }), className) },
      props,
    ),
    render,
    state: { slot: "badge", variant },
  })
}

export { Badge }
