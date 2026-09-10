import { cva } from "class-variance-authority"

export const buttonVariants = cva(
  "inline-flex shrink-0 items-center justify-center gap-2 rounded-md border border-transparent text-center text-sm font-medium transition-colors outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/35 active:translate-y-px disabled:pointer-events-none disabled:opacity-50 [&_svg]:size-4 [&_svg]:shrink-0",
  {
    variants: {
      variant: {
        default: "bg-primary text-primary-foreground hover:bg-primary/88",
        outline: "border-border bg-background hover:bg-muted",
        secondary: "bg-secondary text-secondary-foreground hover:bg-secondary/75",
        ghost: "hover:bg-muted hover:text-foreground",
        destructive: "bg-destructive/10 text-destructive hover:bg-destructive/18",
        link: "text-primary underline-offset-4 hover:underline",
      },
      size: {
        default: "h-9 px-3",
        sm: "h-8 px-2.5 text-[0.8rem]",
        lg: "h-11 px-4",
        icon: "size-9",
      },
    },
    defaultVariants: { variant: "default", size: "default" },
  },
)
