import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

// The API runs on a different origin (http://localhost:5095) on purpose, so
// the browser enforces CORS exactly as it would in production (Task 8.9).
// Override with VITE_API_BASE_URL. The value is injected as a global
// constant rather than read via import.meta.env inside the app, so the same
// source also runs unchanged under Jest.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  return {
    plugins: [react()],
    server: { port: 5173, strictPort: true },
    define: {
      __API_BASE_URL__: JSON.stringify(env.VITE_API_BASE_URL || 'http://localhost:5100'),
    },
  };
});
