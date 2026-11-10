/* global __dirname */
// npm run version:bump [major|minor|fix|build]: raises version.json. Raising major resets minor and fix, and raising
// minor resets fix; every bump also raises the build number, which never goes back, because the App Store and Google
// Play require each upload's to be higher.
// node scripts/bump-version.js set <major> <minor> <fix> <build> [label]: writes those values (the build pipeline,
// before it builds). The label ("Custom": not built from dev) is shown after the version; without one, the file has none.
// The stores get major.minor.fix as the version and the build number separately (app.config.ts).
const fs = require("fs");
const path = require("path");

const versionFile = path.join(__dirname, "..", "version.json");

/** The next version after raising `part` (major, minor, fix or build). */
function bump(version, part) {
    const next = { ...version, fix: version.fix ?? 0 };
    if (part === "major") {
        next.major += 1;
        next.minor = 0;
        next.fix = 0;
    } else if (part === "minor") {
        next.minor += 1;
        next.fix = 0;
    } else if (part === "fix") {
        next.fix += 1;
    } else if (part !== "build") {
        throw new Error(
            `Unknown part "${part}": use major, minor, fix or build`
        );
    }
    next.build += 1;
    return next;
}

/** The version a pipeline passes as text; each part must be a whole number of 0 or more. An empty label is none. */
function parseVersion(major, minor, fix, build, label) {
    const parts = { major, minor, fix, build };
    for (const [name, value] of Object.entries(parts)) {
        if (!/^\d+$/.test(value ?? "")) {
            throw new Error(
                `${name} must be a whole number of 0 or more, not "${value}"`
            );
        }
    }
    return {
        major: Number(major),
        minor: Number(minor),
        fix: Number(fix),
        build: Number(build),
        ...(label?.trim() ? { label: label.trim() } : {}),
    };
}

/** What the run is named, and the app shows after "v": major.minor.fix.build, the build zero-padded to 4 digits. */
function versionName(version) {
    const build = String(version.build).padStart(4, "0");
    return `${version.major}.${version.minor}.${version.fix ?? 0}.${build}`;
}

function main(args, file = versionFile) {
    const [command = "build", ...rest] = args;
    const version =
        command === "set"
            ? parseVersion(...rest)
            : bump(JSON.parse(fs.readFileSync(file, "utf8")), command);
    fs.writeFileSync(file, `${JSON.stringify(version, null, 4)}\n`);
    return versionName(version);
}

if (require.main === module) {
    try {
        console.log(main(process.argv.slice(2)));
    } catch (error) {
        console.error(error.message);
        process.exit(1);
    }
}

module.exports = { bump, parseVersion, versionName, main };
