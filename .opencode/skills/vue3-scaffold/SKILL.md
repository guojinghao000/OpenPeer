---
name: vue3-scaffold
description: Scaffold Vue 3 + Vite + TypeScript + Element Plus SPA frontend project
---

# Vue 3 Frontend Scaffold

## Prerequisites
- Node.js 18+ installed
- npm available

## Step-by-step

### 1. Create Vite project

```bash
npm create vite@latest OpenPeer.Web -- --template vue-ts
```

Move into `src/OpenPeer.Web/` if scaffolded at root level.

### 2. Install dependencies

```bash
npm install
npm install vue-router@4 pinia@2 axios@1 element-plus @element-plus/icons-vue sass
npm install -D @types/node vitest @vue/test-utils happy-dom
```

### 3. Create directory structure

```
src/
├── api/           # Axios client + API modules
├── assets/styles/ # SCSS variables + global styles
├── components/    # Reusable components
│   ├── common/
│   ├── paper/
│   └── comment/
├── composables/   # Vue composable functions
├── layouts/       # Layout components
├── router/        # Vue Router config
├── stores/        # Pinia stores
├── types/         # TypeScript interfaces
├── utils/         # Utility functions
└── views/         # Route-level page components
```

### 4. Configure vite.config.ts

Enable Element Plus auto-import if desired, or use full import:
```ts
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: { '@': resolve(__dirname, 'src') }
  },
  server: {
    port: 5173,
    proxy: {
      '/api': { target: 'http://localhost:5000', changeOrigin: true }
    }
  }
})
```

### 5. Setup vue-tsc type checking

In `tsconfig.json`, ensure `"noUnusedLocals": false` and `"noUnusedParameters": false` (too strict for dev).
Add `vue-tsc --noEmit` as typecheck script in package.json.

## Convention checklist
- All components use `<script setup lang="ts">` (no Options API)
- Component filenames in PascalCase
- Pinia stores use composition API style (not options API style)
- Axios instance with baseURL `/api`, interceptor for JWT Bearer token
- Element Plus components globally registered or auto-imported
