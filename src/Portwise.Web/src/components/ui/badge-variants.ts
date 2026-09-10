import { cva } from "class-variance-authority"

export const badgeVariants = cva(
  "inline-flex h-6 w-fit shrink-0 items-center justify-center gap-1 rounded-full border px-2.5 text-xs font-medium whitespace-nowrap",
  {
    variants: {
      variant: {
        default: "border-transparent bg-primary text-primary-foreground",
        secondary: "border-transparent bg-secondary text-secondary-foreground",
        destructive: "border-destructive/20 bg-destructive/10 text-destructive",
        outline: "border-border text-foreground",
        attention: "border-attention/30 bg-attention-soft text-attention-strong",
        accent: "border-accent/30 bg-accent-soft text-accent-strong",
      },
    },
    defaultVariants: { variant: "default" },
  },
)
