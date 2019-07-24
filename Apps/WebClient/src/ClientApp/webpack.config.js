const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const bundleOutputDir = '../wwwroot/dist';
const VueLoaderPlugin = require('vue-loader/lib/plugin')
const CompressionPlugin = require('compression-webpack-plugin');

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);
    return [{
        mode: isDevBuild ? 'development' : 'production',
        stats: { modules: false },
        context: __dirname,
        resolve: {
            extensions: ['.js', '.ts', 'vue', 'json'],
            alias: {
                '@': path.resolve('app'),
                vue$: 'vue/dist/vue.runtime.esm.js'
            }
        },
        entry: { 'main': './app/boot.ts' },
        module: {
            noParse: /^(vue|vue-router|vuex|vuex-router-sync)$/,
            rules: [
                { test: /\.vue$/, include: /app/, use: 'vue-loader' },
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
                    use: isDevBuild ? [
                        "style-loader", // creates style nodes from JS strings
                        "css-loader", // translates CSS into CommonJS
                        "sass-loader" // compiles Sass to CSS, using Node Sass by default
                    ] : [
                            {
                                loader: MiniCssExtractPlugin.loader,
                            },
                            'css-loader',
                            "sass-loader" // compiles Sass to CSS, using Node Sass by default
                        ],
                },
                {
                    test: /\.css$/,
                    use: isDevBuild ? ['style-loader', 'css-loader'] : [
                        {
                            loader: MiniCssExtractPlugin.loader,
                        },
                        'css-loader',
                    ],
                },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' },
                {
                    test: /\.(woff|woff2|eot|ttf|otf)$/,
                    use: [
                        'file-loader'
                    ]
                }
            ]
        },
        output: {
            path: path.resolve(__dirname, bundleOutputDir),
            filename: '[name].js',
            publicPath: '/dist/'
        },
        plugins: [
            new VueLoaderPlugin(),
            new CheckerPlugin(),
            new webpack.DefinePlugin({
                'process.env': {
                    NODE_ENV: JSON.stringify(isDevBuild ? 'development' : 'production')
                }
            }),
            new webpack.DllReferencePlugin({
                context: __dirname,
                manifest: require('../wwwroot/dist/vendor-manifest.json')
            }),
            new CompressionPlugin({ 
                algorithm: "gzip",
                test: /\.js$|\.css$|\.html$/
            })
        ]
        .concat(isDevBuild ? [
            // Plugins that apply in development builds only
            new webpack.SourceMapDevToolPlugin({
                filename: '[file].map', // Remove this line if you prefer inline source maps
                moduleFilenameTemplate: path.relative(bundleOutputDir, '[resourcePath]') // Point sourcemap entries to the original file locations on disk
            })
        ] : [
                // Plugins that apply in production builds only
                //new webpack.optimize.UglifyJsPlugin(),
                new MiniCssExtractPlugin({ filename: 'site.css' })
            ])
    }];
};
