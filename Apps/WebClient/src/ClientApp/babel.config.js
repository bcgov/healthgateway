module.exports = function (api) {
    api.cache(true);
    return {
        presets: [
            ["babel-preset-expo", { jsxImportSource: "nativewind" }],
            "nativewind/babel",
        ],
        // Jest runs CommonJS without --experimental-vm-modules: turn the lazy `await import("…")` used for
        // optional native modules (clipboard, file system, sharing) into require() in tests only
        plugins:
            process.env.NODE_ENV === "test"
                ? ["@babel/plugin-transform-dynamic-import"]
                : [],
    };
};
