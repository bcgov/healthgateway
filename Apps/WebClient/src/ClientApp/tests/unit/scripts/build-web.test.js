const { afterEach, describe, expect, it } = require("@jest/globals");
const fs = require("fs");
const os = require("os");
const path = require("path");

const { buildEnv, removeUnservedFiles } = require("../../../scripts/build-web");

describe("buildEnv", () => {
    it("defaults to the hosting server's /configuration with Snowplow on", () => {
        const env = buildEnv({ PATH: "/bin" });
        expect(env).toMatchObject({
            PATH: "/bin",
            EXPO_PUBLIC_WEB_CLIENT_BASE_URI: "/",
            EXPO_PUBLIC_SNOWPLOW_ENABLED: "true",
            CI: "1",
            EXPO_NO_TELEMETRY: "1",
        });
    });

    it("keeps values the caller's environment sets", () => {
        const env = buildEnv({
            EXPO_PUBLIC_WEB_CLIENT_BASE_URI: "https://cfg.test/",
            EXPO_PUBLIC_SNOWPLOW_ENABLED: "false",
        });
        expect(env.EXPO_PUBLIC_WEB_CLIENT_BASE_URI).toBe("https://cfg.test/");
        expect(env.EXPO_PUBLIC_SNOWPLOW_ENABLED).toBe("false");
    });
});

describe("removeUnservedFiles", () => {
    let dir;

    afterEach(() => fs.rmSync(dir, { recursive: true, force: true }));

    it("removes source maps and .ttf fonts at any depth and keeps the rest", () => {
        dir = fs.mkdtempSync(path.join(os.tmpdir(), "build-web-"));
        const files = [
            "index.html",
            "_expo/static/js/web/entry-abc.js",
            "_expo/static/js/web/entry-abc.js.map",
            "assets/fonts/Font.TTF",
            "fonts/Font.woff2",
        ];
        for (const file of files) {
            fs.mkdirSync(path.dirname(path.join(dir, file)), {
                recursive: true,
            });
            fs.writeFileSync(path.join(dir, file), "");
        }

        removeUnservedFiles(dir);

        const exists = (file) => fs.existsSync(path.join(dir, file));
        expect(files.filter(exists)).toEqual([
            "index.html",
            "_expo/static/js/web/entry-abc.js",
            "fonts/Font.woff2",
        ]);
    });
});
