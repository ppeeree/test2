// const { defineConfig } = require('@vue/cli-service')
// 打包分析code1
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin
//==end
// 优化LODASH内存占用
const LodashWebpackPlugin = require('lodash-webpack-plugin')
require('events').EventEmitter.defaultMaxListeners = 0
const path = require('path')
const http = require('http')
const proxyAgent = new http.Agent({
  keepAlive: true,
  keepAliveMsecs: 30000,
  maxSockets: 30,
  maxFreeSockets: 10
})
module.exports = {
  //路径前缀
  publicPath: '/',
  lintOnSave: false,
  outputDir: 'www',
  productionSourceMap: false,
  runtimeCompiler: true,
  chainWebpack: (config) => {
    // 打包分析code2
    if (process.env.NODE_ENV === 'production') {
      config.plugin('webpack-bundle-analyzer')
        .use(BundleAnalyzerPlugin)
    }
    // ==end
    config.plugin('lodash').use(LodashWebpackPlugin, [{
      caching: true,
      collections: true,
      paths: false,      // 不用 _.get(a,'b.c') 可关
      shorthands: false
    }])
    // 指向自己写的空模块
    config.resolve.alias.set(
      'lodash-es',
      'lodash'
    )
    config.resolve.alias.set(
      'vue$',
      path.resolve(__dirname, 'node_modules/vue/dist/vue.esm-bundler.js')
    )
    config.resolve.alias.set(
      'acharts',
      path.resolve(__dirname, '../acharts/index.js')
    )
    //忽略的打包文件,CDN引入
    config.externals({
      xlsx: 'XLSX',
    })
    //==end
    const entry = config.entry('app')
    entry.add('babel-polyfill').end()
    entry.add('classlist-polyfill').end()
    entry.add('@/mock').end()
  },
  configureWebpack: {
    output: {
      filename: 'js/[name].[fullhash].js',
      chunkFilename: 'js/[name].[chunkhash].js'
    },
    module: {
      rules: [
        {
          test: path.resolve(__dirname, 'node_modules/leader-line/'),
          use: [
            {
              loader: 'skeleton-loader',
              options: {
                procedure: content => `${content}export default LeaderLine`
              }
            }
          ]
        },
        {
          test: /\.mjs$/,
          include: /node_modules/,
          type: "javascript/auto"
        },
      ]
    },
    // 首页加载优化: 配置有效的代码分割
    /* optimization: {
      splitChunks: {
        chunks: 'all',
        minSize: 20000, // 20KB 以上才分割
        cacheGroups: {
          lodashVendor: {
            test: /[\\/]node_modules[\\/]lodash[\\/]/,
            name: 'lodash',
            priority: 50,
            reuseExistingChunk: true
          },
          vue: {
            name: 'chunk-vue',
            test: /[\\/]node_modules[\\/](vue|vue-router|vuex)[\\/]/,
            priority: 30,
          },
          elementUI: {
            name: 'chunk-element-ui',
            test: /[\\/]node_modules[\\/]element-ui[\\/]/,
            priority: 25,
          },
          echarts: {
            name: 'chunk-echarts',
            test: /[\\/]node_modules[\\/]echarts[\\/]/,
            priority: 20,
          },
          vendors: {
            name: 'chunk-vendors',
            test: /[\\/]node_modules[\\/]/,
            priority: 10,
          },
          common: {
            name: 'chunk-common',
            minChunks: 2,
            priority: 5,
            reuseExistingChunk: true
          }
        }
      }
    }, */
    // 首页加载优化：压缩优化
    plugins: process.env.npm_config_report ? [
      /*  new CompressionWebpackPlugin({
         algorithm: 'gzip',
         test: /\.(js|css|html|svg)$/,
         threshold: 10240,
         minRatio: 0.8
       }), */
      // 打包分析code3
      new BundleAnalyzerPlugin({
        analyzerMode: 'server', // 可以是 'server', 'static' 或 'disabled'
        analyzerHost: '127.0.0.1',
        analyzerPort: 8888,
        reportFilename: 'report.html',
        defaultSizes: 'parsed', // 可以是 'stat', 'parsed' 或 'gzip'
        openAnalyzer: true, // 是否自动在浏览器中打开报告
        generateStatsFile: false, // 是否生成stats.json文件
        statsFilename: 'stats.json',
        statsOptions: null,
        logLevel: 'info'
      })
      //==end
    ] : []
    // ==end==
  },
  css: {
    extract: { ignoreOrder: true },
    loaderOptions: {
      css: {
        url: {
          filter: url => {
            return !/^\/(img|assets|svg|cdn|util)\//.test(url)
          }
        }
      }
    }
  },
  //开发模式反向代理配置，生产模式请使用Nginx部署并配置反向代理
  devServer: {
    port: 1888,
    client:{
      overlay: {
      warnings: false,
      errors: false
    }
    
    },
    proxy: {
      /* '/api': {
        //内网
        target: process.env.VUE_APP_API_URL,
        //映射地址
        // target: process.env.VUE_APP_API_URL_REMOTE,
        // target:'http://achxa.nginx.cpolar.cn',
        ws: true,
        changOrigin: true,
        pathRewrite: {
          '^/api': process.env.VUE_APP_PATH_REWRITE
        },
        // logLevel: "debug" topography
      },*/

      '/NetApi': {
        target: process.env.VUE_APP_API_URL,
        changeOrigin: true,
        agent: proxyAgent,
        timeout: 300000,
        proxyTimeout: 300000
      },
      '^/modelfile/': {
        target: process.env.VUE_APP_API_URL,
        changeOrigin: true,
        agent: proxyAgent,
        timeout: 300000,
        proxyTimeout: 300000
      },
      "^/(dem|projection|topography)/": {
        target: 'http://192.168.160.242:602',
        changeOrigin: true
      },
      /*'/imgFile': {
        target: 'http://192.168.124.11:702',//'http://192.168.124.11:702',
        changeOrigin: true,
      }, */
      '^/geoserver/': {
        target: 'http://192.168.124.11:9096',
        changeOrigin: true
      },
      /*  '/tomcat': {
         target: 'http://localhost:8080',
         changeOrigin: true,
         pathRewrite: {
           '^/tomcat': '/'
         }
       },
       '/localWtphmApi': {
         target: 'http://localhost:8201',
         changeOrigin: true,
         pathRewrite: {
           '^/localWtphmApi/wtphm-service': '/'
         }
       },
       '/localDiagApi': {
         target: 'http://localhost:8300',
         changeOrigin: true,
         pathRewrite: {
           '^/localDiagApi/diag-service': '/'
         }
       } */
    }
  }
}
