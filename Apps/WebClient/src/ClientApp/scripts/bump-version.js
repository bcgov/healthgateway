/* global __dirname */
// npm run version:bump [major|minor|build]: raises version.json. Major and minor keep counting builds: the build number
// never goes back, because the App Store and Google Play require each upload's to be higher.
// node scripts/bump-version.js set <major> <minor> <build> [label]: writes those values (the build pipeline, before it
// builds). The label ("Custom": not built from dev) is shown after the build number; without one, the file has none.
const fs = require("fs");
const path = require("path");

const versionFile = path.join(__dirname, "..", "version.json");

/** The next version after raising `part` (major, minor or build). */
function bump(version, part) {
    const next = { ...version };
    if (part === "major") {
        next.major += 1;
        next.minor = 0;
    } else if (part === "minor") {
        next.minor += 1;
    } else if (part !== "build") {
        throw new Error(`Unknown part "${part}": use major, minor or build`);
    }
    next.build += 1;
    return next;
}

/** The version a pipeline passes as text; each part must be a whole number of 0 or more. An empty label is none. */
function parseVersion(major, minor, build, label) {
    const parts = { major, minor, build };
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
        build: Number(build),
        ...(label?.trim() ? { label: label.trim() } : {}),
    };
}

function main(args, file = versionFile) {
    const [command = "build", ...rest] = args;
    const version =
        command === "set"
            ? parseVersion(...rest)
            : bump(JSON.parse(fs.readFileSync(file, "utf8")), command);
    fs.writeFileSync(file, `${JSON.stringify(version, null, 4)}\n`);
    return `${version.major}.${version.minor}.${version.build}`;
}

if (require.main === module) {
    try {
        console.log(main(process.argv.slice(2)));
    } catch (error) {
        console.error(error.message);
        process.exit(1);
    }
}

module.exports = { bump, parseVersion, main };
