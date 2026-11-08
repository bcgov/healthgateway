/* global __dirname */
// npm run version:bump [major|minor|build]: raises version.json. Major and minor keep counting builds: the build number
// never goes back, because the App Store and Google Play require each upload's to be higher.
const fs = require("fs");
const path = require("path");

const file = path.join(__dirname, "..", "version.json");
const version = JSON.parse(fs.readFileSync(file, "utf8"));
const part = process.argv[2] ?? "build";

if (part === "major") {
    version.major += 1;
    version.minor = 0;
} else if (part === "minor") {
    version.minor += 1;
} else if (part !== "build") {
    console.error(`Unknown part "${part}": use major, minor or build`);
    process.exit(1);
}
version.build += 1;

fs.writeFileSync(file, `${JSON.stringify(version, null, 4)}\n`);
console.log(`${version.major}.${version.minor}.${version.build}`);
