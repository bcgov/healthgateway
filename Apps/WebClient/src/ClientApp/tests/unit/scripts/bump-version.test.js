const { afterEach, describe, expect, it } = require("@jest/globals");
const fs = require("fs");
const os = require("os");
const path = require("path");

const { bump, main, parseVersion } = require("../../../scripts/bump-version");

describe("bump", () => {
    const version = { major: 1, minor: 2, build: 30 };

    it.each([
        ["build", { major: 1, minor: 2, build: 31 }],
        ["minor", { major: 1, minor: 3, build: 31 }],
        ["major", { major: 2, minor: 0, build: 31 }],
    ])("%s keeps the build number counting", (part, expected) => {
        expect(bump(version, part)).toEqual(expected);
    });

    it("rejects an unknown part", () => {
        expect(() => bump(version, "patch")).toThrow('Unknown part "patch"');
    });
});

describe("parseVersion", () => {
    it("reads the pipeline's whole numbers", () => {
        expect(parseVersion("2", "0", "1234")).toEqual({
            major: 2,
            minor: 0,
            build: 1234,
        });
    });

    it("keeps a label, and treats an empty one as none", () => {
        expect(parseVersion("2", "0", "7", "Custom")).toEqual({
            major: 2,
            minor: 0,
            build: 7,
            label: "Custom",
        });
        expect(parseVersion("2", "0", "7", " ")).not.toHaveProperty("label");
    });

    it.each([
        ["", "0", "1"],
        ["1", "x", "1"],
        ["1", "0", "-1"],
        ["1", "0", "1.5"],
        ["1", "0", undefined],
    ])("rejects %s.%s.%s", (major, minor, build) => {
        expect(() => parseVersion(major, minor, build)).toThrow(
            "must be a whole number"
        );
    });
});

describe("main", () => {
    let dir;
    afterEach(() => {
        if (dir) fs.rmSync(dir, { recursive: true, force: true });
    });
    const versionFile = (version) => {
        dir = fs.mkdtempSync(path.join(os.tmpdir(), "version-"));
        const file = path.join(dir, "version.json");
        fs.writeFileSync(file, JSON.stringify(version));
        return file;
    };

    it("set writes the given version", () => {
        const file = versionFile({ major: 0, minor: 0, build: 0 });
        expect(main(["set", "2", "1", "57"], file)).toBe("2.1.57");
        expect(fs.readFileSync(file, "utf8")).toBe(
            '{\n    "major": 2,\n    "minor": 1,\n    "build": 57\n}\n'
        );
    });

    it("set writes a label after the build", () => {
        const file = versionFile({ major: 0, minor: 0, build: 0 });
        main(["set", "0", "1", "8", "Custom"], file);
        expect(JSON.parse(fs.readFileSync(file, "utf8"))).toEqual({
            major: 0,
            minor: 1,
            build: 8,
            label: "Custom",
        });
    });

    it("raises the build by default", () => {
        const file = versionFile({ major: 0, minor: 0, build: 0 });
        expect(main([], file)).toBe("0.0.1");
    });
});
