// var webpack = require('webpack');
const path = require("path");

module.exports = {
  mode: "development",
  entry: './index.js',
  output: {
    filename: 'index.js',
    path: path.join(__dirname, "dist"),
  },
  devServer: {
    contentBase: './public',
    proxy: {
      /* '/api': {
        target: 'https://api.github.com',
        pathRewrite: {
          '^/api': ''
        },
        // 不能使用localhost:8080作为请求GitHub的主机名
        changeOrigin: true, // 以实际代理的主机名去请求
      } */
    }
  },
  module: {
    rules: [
      {
        test: /\.css$/,    //打包规则应用到以css结尾的文件上
        use: ['style-loader', 'css-loader']
      },
      {
        test: /\.(png|jpe?g|gif|webp|svg)$/,
        type: "asset",
        parser: {
          dataUrlCondition: {
            //小于10kb转换格式
            maxSize: 10 * 1024,//10kb
          }
        }
      },
      /*  {
         test: /\.worker\.js$/,
         loader: "worker-loader",
         options: {
           inline: true,
           name: "workerName.[hash].js"
         }
       }, */
      {
        test: /.js$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader',
          options: {
            presets: ['@babel/preset-env'],
            cacheDirectory: true,
            cacheCompression: false,
          },
        }
      },
      /* {
        test: /.html$/,
        use: {
          loader: 'html-loader',
          options: {
            attributes: {
              list: [
                {
                  tag: 'img',
                  attribute: 'src',
                  type: 'src'
                },
                {
                  tag: 'a',
                  attribute: 'href',
                  type: 'src'
                }
              ]
            }
          }
        }
      } */
    ]
  },
  plugins: []
};