// Jest + React Testing Library. The coverage threshold is the Testing
// Focus mandate: `npm run test:coverage` FAILS if lines, branches,
// functions or statements drop below 80%.
module.exports = {
  testEnvironment: 'jsdom',
  setupFilesAfterEnv: ['<rootDir>/jest.setup.js'],
  moduleNameMapper: {
    '\\.(css)$': 'identity-obj-proxy',
    // axios ships an ESM browser build that Jest's CommonJS runtime can't
    // load; point it at the CommonJS build instead.
    '^axios$': 'axios/dist/node/axios.cjs',
  },
  testMatch: ['<rootDir>/src/__tests__/**/*.test.{js,jsx}'],
  collectCoverageFrom: ['src/**/*.{js,jsx}', '!src/main.jsx', '!src/__tests__/**'],
  coverageReporters: ['text', 'text-summary', 'lcov'],
  coverageThreshold: {
    global: { lines: 80, branches: 80, functions: 80, statements: 80 },
  },
};
