import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],

  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './tests/setup.js',

    // Only run the Vitest tests we create for Member 2.
    // The existing triage.test.js remains a Node test.
    include: [
      'tests/**/*.vitest.test.js',
      'tests/**/*.vitest.test.jsx'
    ]
  }
});