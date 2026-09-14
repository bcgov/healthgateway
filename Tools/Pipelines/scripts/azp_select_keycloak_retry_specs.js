const fs = require("fs");
const path = require("path");

const resultsDirectory = process.argv[2];
const credentialPostTimeout =
    "cy.request() timed out waiting 60000ms for a response from your server";
const callbackTimeout =
    "Timed out after waiting 60000ms for your remote page to load";
const sessionSetupFailure =
    "This error occurred while creating the session. Because the session setup failed";
const beforeEachFailure = /error occurred during a before each hook/i;
const requestResponseTimeout =
    /cy\.request\(\) timed out waiting \d+ms for a response from your server\./;
const noResponseReceived = "No response was received within the timeout.";
const interceptRequestTimeout =
    /cy\.wait\(\) timed out waiting \d+ms for the 1st request to the route:/;
const noRequestOccurred = "No request ever occurred.";

function readFiles(directory) {
    return fs.readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
        const entryPath = path.join(directory, entry.name);
        return entry.isDirectory() ? readFiles(entryPath) : [entryPath];
    });
}

if (!resultsDirectory || !fs.existsSync(resultsDirectory)) {
    console.error("UI Cypress result artifacts were not found.");
    process.exit(1);
}

const specResultFiles = readFiles(resultsDirectory).filter((file) =>
    file.endsWith(".json")
);
if (specResultFiles.length === 0) {
    console.error("Cypress spec result artifacts were not found.");
    process.exit(1);
}

const failedTests = specResultFiles.flatMap((file) => {
    const result = JSON.parse(fs.readFileSync(file, "utf8"));
    return result.failedTests.map((test) => ({ ...test, spec: result.spec }));
});

if (failedTests.length === 0) {
    process.exit(0);
}

const retrySpecs = new Set();
for (const failedTest of failedTests) {
    const error = failedTest.displayError ?? "";
    const isKeycloakSessionTimeout =
        error.includes(sessionSetupFailure) &&
        (error.includes(credentialPostTimeout) || error.includes(callbackTimeout));
    const isBeforeEachSetupFailure = beforeEachFailure.test(error);
    const isRequestTransportTimeout =
        requestResponseTimeout.test(error) && error.includes(noResponseReceived);
    const isInterceptRequestTimeout =
        interceptRequestTimeout.test(error) && error.includes(noRequestOccurred);

    if (
        !isKeycloakSessionTimeout &&
        !isBeforeEachSetupFailure &&
        !isRequestTransportTimeout &&
        !isInterceptRequestTimeout
    ) {
        continue;
    }

    retrySpecs.add(failedTest.spec);
}

console.error(
    `Found ${failedTests.length} Cypress error result(s); selected ${retrySpecs.size} retriable spec(s).`
);
process.stdout.write([...retrySpecs].join("\n"));
