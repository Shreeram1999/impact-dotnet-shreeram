/* global __API_BASE_URL__ */

// Injected by vite.config.js at build time. Under Jest the constant doesn't
// exist, so fall back to the API's local address.
export const API_BASE_URL =
  typeof __API_BASE_URL__ !== 'undefined' ? __API_BASE_URL__ : 'http://localhost:5095';
