// Used only by Jest (babel-jest) to turn JSX + ES modules into CommonJS for
// Node. Vite builds the app with its own pipeline and ignores this file.
module.exports = {
  presets: [
    ['@babel/preset-env', { targets: { node: 'current' } }],
    ['@babel/preset-react', { runtime: 'automatic' }],
  ],
};
