// Flat ESLint config — https://docs.expo.dev/guides/using-eslint/
const { defineConfig } = require('eslint/config');
const expoConfig = require('eslint-config-expo/flat');

module.exports = defineConfig([
  expoConfig,
  {
    ignores: [
      'dist/*',
      '.expo/*',
      'android/*',
      'ios/*',
      'node_modules/*',
      'scripts/*',
      'expo-env.d.ts',
    ],
  },
]);
