module.exports = {
  root: true,
  env: {
    node: true
  },
  'extends': [
    'plugin:vue/essential',
    /* 'eslint:recommended' */
  ],
  rules: {
    'no-console': process.env.NODE_ENV === 'production' ? 'error' : 'off',
    'no-debugger': process.env.NODE_ENV === 'production' ? 'error' : 'off',
    semi: ["error", "never"],   // 强制不得写分号
    'semi-spacing': ["error", { before: false, after: true }]
  },
  parserOptions: {
    parser: 'babel-eslint'
  }
}