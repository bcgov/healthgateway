const { afterEach, describe, expect, it } = require("@jest/globals");
const fs = require("fs");
const os = require("os");
const path = require("path");

const {
    bump,
    main,
    parseVersion,
    versionName,
} = require("../../../scripts/bump-version");

describe("bump", () => {
    const version = { major: 1, minor: 2, fix: 3, build: 30 };

    it.each([
        ["build", { major: 1, minor: 2, fix: 3, build: 31 }],
        ["fix", { major: 1, minor: 2, fix: 4, build: 31 }],
        ["minor", { major: 1, minor: 3, fix: 0, build: 31 }],
        ["major", { major: 2, minor: 0, fix: 0, build: 31 }],
    ])("%s keeps the build number counting", (part, expected) => {
        expect(bump(version, part)).toEqual(expected);
    });

    it("a version.json from before fix starts it at 0", () => {
        expect(bump({ major: 1, minor: 2, build: 30 }, "fix")).toEqual({
            major: 1,
            minor: 2,
            fix: 1,
            build: 31,
        });
    });

    it("rejects an unknown part", () => {
        expect(() => bump(version, "patch")).toThrow('Unknown part "patch"');
    });
});

describe("parseVersion", () => {
    it("reads the pipeline's whole numbers", () => {
        expect(parseVersion("3", "0", "1", "1234")).toEqual({
            major: 3,
            minor: 0,
            fix: 1,
            build: 1234,
        });
    });

    it("reads a zero-padded build from the run name as its number", () => {
        expect(parseVersion("3", "0", "0", "0057").build).toBe(57);
    });

    it("keeps a label, and treats an empty one as none", () => {
        expect(parseVersion("2", "0", "1", "7", "Custom")).toEqual({
            major: 2,
            minor: 0,
            fix: 1,
            build: 7,
            label: "Custom",
        });
        expect(parseVersion("2", "0", "1", "7", " ")).not.toHaveProperty(
            "label"
        );
    });

    it.each([
        ["", "0", "0", "1"],
        ["1", "x", "0", "1"],
        ["1", "0", "-1", "1"],
        ["1", "0", "0", "1.5"],
        ["1", "0", "0", undefined],
        ["1", "0", undefined, "7"],
    ])("rejects %s.%s.%s.%s", (major, minor, fix, build) => {
        expect(() => parseVersion(major, minor, fix, build)).toThrow(
            "must be a whole number"
        );
    });
});

describe("versionName", () => {
    it.each([
        [{ major: 3, minor: 0, fix: 0, build: 57 }, "3.0.0.0057"],
        [{ major: 2, minor: 1, fix: 4, build: 12345 }, "2.1.4.12345"],
        [{ major: 0, minor: 0, build: 0 }, "0.0.0.0000"],
    ])("%j is %s, as the run is named", (version, expected) => {
        expect(versionName(version)).toBe(expected);
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
        const file = versionFile({ major: 0, minor: 0, fix: 0, build: 0 });
        expect(main(["set", "3", "1", "2", "57"], file)).toBe("3.1.2.0057");
        expect(fs.readFileSync(file, "utf8")).toBe(
            '{\n    "major": 3,\n    "minor": 1,\n    "fix": 2,\n    "build": 57\n}\n'
        );
    });

    it("set writes a label after the build", () => {
        const file = versionFile({ major: 0, minor: 0, fix: 0, build: 0 });
        main(["set", "0", "1", "0", "8", "Custom"], file);
        expect(JSON.parse(fs.readFileSync(file, "utf8"))).toEqual({
            major: 0,
            minor: 1,
            fix: 0,
            build: 8,
            label: "Custom",
        });
    });

    it("raises the build by default", () => {
        const file = versionFile({ major: 0, minor: 0, fix: 0, build: 0 });
        expect(main([], file)).toBe("0.0.0.0001");
    });
});
