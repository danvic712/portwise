import type { ReactNode } from "react"
import { Component } from "react"

type RouteErrorBoundaryProps = {
  children: ReactNode
  fallback: ReactNode
  resetKey: string
}

type RouteErrorBoundaryState = {
  error: Error | null
}

/** Keeps a page render failure local to the current route instead of blanking the application shell. */
export class RouteErrorBoundary extends Component<RouteErrorBoundaryProps, RouteErrorBoundaryState> {
  state: RouteErrorBoundaryState = { error: null }

  static getDerivedStateFromError(error: Error): RouteErrorBoundaryState {
    return { error }
  }

  componentDidUpdate(previousProps: RouteErrorBoundaryProps) {
    if (previousProps.resetKey !== this.props.resetKey && this.state.error) {
      this.setState({ error: null })
    }
  }

  render() {
    return this.state.error ? this.props.fallback : this.props.children
  }
}
