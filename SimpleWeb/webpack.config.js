var path = require("path");
var webpack = require("webpack");

module.exports = {
    context: path.resolve("./Client"),
    entry: "./index",
    output: {
        path: path.resolve("./wwwroot"),
        filename: "app.js"
    },
    resolve: {
        extensions: ["", ".webpack.js", ".js", ".ts", ".tsx"]
    },
    devtool: "source-map",
    module: {
        loaders: [
            { test: /\.tsx?$/, loader: "ts-loader" },
            { test: /\.html$/, loader: "file-loader?name=[name].[ext]" }
        ]
    },
    plugins: [
        // new webpack.DefinePlugin({
        //     "process.env": {
        //         NODE_ENV: '"production"'
        //     }
        // })
    ]
};