module.exports = {
  presets: [
    '@vue/app',
    '@babel/preset-env',
    [
      '@vue/babel-preset-jsx',
      {
        injectH: false
      }
    ]
  ],
  plugins: [
    // ② 按需加载 umy-ui（可并存）
    ["component", {
      "libraryName": "umy-ui",
      "styleLibraryName": "theme-chalk"
    }],
    ['@babel/plugin-proposal-optional-chaining']]
}
