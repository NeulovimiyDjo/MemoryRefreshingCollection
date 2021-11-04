var path = require('path')
const VueLoaderPlugin = require('vue-loader/lib/plugin')

module.exports = {
  entry: [
    '@babel/polyfill',
    './Client/main.js'
  ],
  output: {
    path: path.resolve(__dirname, './wwwroot/js'),
    filename: 'build.js'
  },
  module: {
    rules: [
      {
        test: /\.vue$/,
        loader: 'vue-loader'
      },
      {
        test: /\.js$/,
        loader: 'babel-loader'
      },
      {
        test: /\.css$/,
        use: [
          'vue-style-loader',
          'css-loader'
        ]
      },
      {
        test: /\.(png|jpg|gif)$/,
        use: [
          {
            loader: 'file-loader',
            options: {
              name: '[path][name].[ext]',
              context: 'Client/resources',
              outputPath: '../', // path for generated files (wwwroot/js/* => wwwroot/*)
            },
          },
        ],
      },
    ]
  },
  plugins: [
    new VueLoaderPlugin()
  ],
  devServer: {
    contentBase: path.join(__dirname, 'wwwroot'),
    publicPath: '/js/', // path to virtual build.js
    historyApiFallback: {
      index: 'index.html'
    },
    watchContentBase: true,
    compress: true
  }
}