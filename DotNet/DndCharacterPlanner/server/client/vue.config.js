const fs = require('fs');

module.exports = {
    outputDir: './dist',
    configureWebpack: {
        devtool: 'source-map',
    },
    publicPath:process.env.NODE_ENV === 'production'
        ? '/' : '/',
    devServer: {
        host: '0.0.0.0',
        https: {
            key: fs.readFileSync('./cert/serverKey.pem'),
            cert: fs.readFileSync('./cert/server.pem')
        }
    }
};
