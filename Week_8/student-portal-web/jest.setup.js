import '@testing-library/jest-dom';
import { TextEncoder, TextDecoder } from 'node:util';

// React Router needs these; jsdom doesn't provide them.
globalThis.TextEncoder ??= TextEncoder;
globalThis.TextDecoder ??= TextDecoder;
