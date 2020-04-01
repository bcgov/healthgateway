const path = require("path");
const webpack = require("webpack");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CheckerPlugin = require("awesome-typescript-loader").CheckerPlugin;
const bundleOutputDir = "../wwwroot/dist";
const VueLoaderPlugin = require("vue-loader/lib/plugin");
const CompressionPlugin = require("compression-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");
const BundleAnalyzerPlugin = require("webpack-bundle-analyzer")
  .BundleAnalyzerPlugin;

module.exports = (env, argv) => {
  const isDevBuild = argv !== undefined ? !argv.p : true;
  const cssLoader = "css-loader";
  return [
    {
      mode: isDevBuild ? "development" : "production",
      stats: { modules: false },
      context: __dirname,
      resolve: {
        extensions: [".js", ".ts", "vue", "json"],
        alias: {
          "@": path.resolve("app"),
          vue$: "vue/dist/vue.runtime.esm.js"
        }
      },
      entry: { main: "./app/boot.ts" },
      module: {
        noParse: /^(vue|vue-router|vuex|vuex-router-sync)$/,
        rules: [
          { test: /\.vue$/, include: /app/, use: "vue-loader" },
          { test: /\.html$/, use: "raw-loader" },
          {
            test: /\.ts$/,
            include: /app/,
            loader: "ts-loader",
            options: {
              appendTsSuffixTo: [/\.vue$/],
              transpileOnly: true
            }
          },
          {
            test: /\.scss$/,
            use: isDevBuild
              ? [
                  "style-loader", // creates style nodes from JS strings
                  cssLoader, // translates CSS into CommonJS
                  "sass-loader" // compiles Sass to CSS, using Node Sass by default
                ]
              : [
                  {
                    loader: MiniCssExtractPlugin.loader
                  },
                  cssLoader,
                  "sass-loader" // compiles Sass to CSS, using Node Sass by default
                ]
          },
          {
            test: /\.css$/,
            use: isDevBuild
              ? ["style-loader", cssLoader]
              : [
                  {
                    loader: MiniCssExtractPlugin.loader
                  },
                  cssLoader
                ]
          },
          { test: /\.(png|jpg|jpeg|gif|svg)$/, use: "url-loader?limit=25000" },
          {
            test: /\.(png|jpg|jpeg|gif|svg)$/,
            use: "image-webpack-loader",
            enforce: "pre"
          },
          {
            test: /\.(woff|woff2|eot|ttf|otf)$/,
            use: ["file-loader"]
          }
        ]
      },
      output: {
        path: path.resolve(__dirname, bundleOutputDir),
        filename: "[name].bundle.js",
        chunkFilename: "[name].chunk.[contenthash].js",
        publicPath: "/dist/"
      },
      plugins: [
        new VueLoaderPlugin(),
        new CheckerPlugin(),
        new webpack.DefinePlugin({
          _NODE_ENV: JSON.stringify(isDevBuild ? "development" : "production")
        }),
        new webpack.DllReferencePlugin({
          context: __dirname,
          manifest: require("../wwwroot/dist/vendor-manifest.json")
        }),
        new webpack.IgnorePlugin(/^\.\/locale$/, /moment$/)
        //new BundleAnalyzerPlugin()
      ].concat(
        isDevBuild
          ? [
              // Plugins that apply in development builds only
              new webpack.SourceMapDevToolPlugin({
                filename: "[file].map", // Remove this line if you prefer inline source maps
                moduleFilenameTemplate: path.relative(
                  bundleOutputDir,
                  "[resourcePath]"
                ) // Point sourcemap entries to the original file locations on disk
              })
            ]
          : [
              // Plugins that apply in production builds only
              new MiniCssExtractPlugin({
                filename: "site.css",
                chunkFilename: "[id].[hash].css"
              }),
              new CompressionPlugin({
                algorithm: "gzip",
                test: /\.js$|\.css$|\.html$/
              })
            ]
      ),
      optimization: {
        usedExports: true,
        minimize: true,
        minimizer: [new TerserPlugin()]
      }
    }
  ];
};
