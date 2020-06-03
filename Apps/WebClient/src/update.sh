#!/bin/bash
cd ClientApp
node node_modules/webpack/bin/webpack.js
cd ..
dotnet run