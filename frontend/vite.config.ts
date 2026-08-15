import path from "path"
import tailwindcss from "@tailwindcss/vite"
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from "node:fs";

// https://vite.dev/config/
export default defineConfig(({command}) => ({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    host: "localhost",
    port: 5001,
    https: command === "build" ? undefined : {
      key: fs.readFileSync('../certs/localhost.key'),
      cert: fs.readFileSync('../certs/localhost.crt'),
    }
  },
}))
