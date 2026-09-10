import { defineConfig } from "vite"
import react from "@vitejs/plugin-react"
import tailwindcss from "@tailwindcss/vite"
import path from "node:path"

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(import.meta.dirname, "./src"),
    },
  },
  build: {
    outDir: "../Portwise/wwwroot",
    emptyOutDir: true,
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes("node_modules/react/") || id.includes("node_modules/react-dom/")) return "react-vendor"
          if (id.includes("node_modules/@base-ui/") || id.includes("node_modules/class-variance-authority/")) return "ui-vendor"
          if (id.includes("node_modules/lucide-react/")) return "icon-vendor"
          return undefined
        },
      },
    },
  },
  server: {
    host: true,
    port: 4173,
    fs: {
      allow: [path.resolve(import.meta.dirname), path.resolve(import.meta.dirname, "../../locales")],
    },
    proxy: {
      "/api": "http://localhost:5276",
      "/healthz": "http://localhost:5276",
      "/readyz": "http://localhost:5276",
    },
  },
})
