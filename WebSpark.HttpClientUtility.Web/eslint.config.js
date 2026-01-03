import js from '@eslint/js';
import globals from 'globals';

export default [
  js.configs.recommended,
  {
    languageOptions: {
      ecmaVersion: 2022,
      sourceType: 'module',
      globals: {
        ...globals.browser,
        ...globals.es2021,
        ...globals.node,
        bootstrap: 'readonly'
      }
    },
    rules: {
      // Best Practices
      'no-console': ['warn', { allow: ['warn', 'error'] }],
      'no-debugger': 'error',
      'no-alert': 'warn',
      'no-var': 'error',
      'prefer-const': 'error',
      'prefer-arrow-callback': 'warn',
      'no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
      
      // Code Quality
      'eqeqeq': ['error', 'always'],
      'curly': ['error', 'all'],
      'brace-style': ['error', '1tbs'],
      'indent': ['error', 2, { SwitchCase: 1 }],
      'quotes': ['error', 'single', { avoidEscape: true }],
      'semi': ['error', 'always'],
      'comma-dangle': ['error', 'never'],
      'no-trailing-spaces': 'error',
      'eol-last': ['error', 'always'],
      
      // ES6+
      'arrow-spacing': 'error',
      'no-duplicate-imports': 'error',
      'prefer-template': 'warn',
      'template-curly-spacing': ['error', 'never']
    }
  },
  {
    ignores: [
      'wwwroot/**',
      'node_modules/**',
      'dist/**',
      'build/**',
      '*.min.js'
    ]
  }
];
