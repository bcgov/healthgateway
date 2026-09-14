const { defineConfig } = require("cypress");
const { verifyDownloadTasks } = require("cy-verify-downloads");
const fs = require("fs");
const path = require("path");

module.exports = defineConfig({
    defaultCommandTimeout: 30000,
    blockHosts: ["spt.apps.gov.bc.ca"],
    retries: {
        runMode: 1,
        openMode: 0,
    },
    viewportWidth: 1920,
    viewportHeight: 1080,
    reporter: "mocha-junit-reporter",
    reporterOptions: {
        mochaFile: "reports/junit/test-results.[hash].xml",
        testsuitesTitle: false,
    },
    env: {
        baseWebClientUrl: "",
        "bcsc.username": "hthgtwy11",
        "bcsc.password": "",
        "keycloak.username": "healthgateway",
        "keycloak.password": "",
        "keycloak.accept.tos.username": "hthgtwy04",
        "keycloak.accountclosure.username": "AccountClosure",
        "keycloak.deceased.username": "hthgtwy19",
        "keycloak.healthgateway12.username": "healthgateway12",
        "keycloak.hlthgw401.username": "hlthgw401",
        "keycloak.hthgtwy06.username": "hthgtwy06",
        "keycloak.hthgtwy20.username": "hthgtwy20",
        "keycloak.laboratory.queued.username": "hthgtwy09",
        "keycloak.notfound.username": "hthgtwy03",
        "keycloak.protected.username": "protected",
        "keycloak.unregistered.username": "hthgtwy02",
        "idir.username": "hgateway",
        "idir.password": "",
        "keycloak.phsa.client": "",
        "keycloak.phsa.secret": "",
        "keycloak.erebus.client": "",
        "keycloak.erebus.secret": "",
        phoneNumber: "",
        emailAddress: "fakeemail@healthgateway.gov.bc.ca",
        phn: "9735353315",
    },
    projectId: "ofnepc",
    trashAssetsBeforeRuns: true,
    e2e: {
        setupNodeEvents(on, _config) {
            on("task", verifyDownloadTasks);
            on("after:spec", (spec, results) => {
                if (!results) {
                    return;
                }

                const resultsDirectory = path.join("reports", "spec-results");
                fs.mkdirSync(resultsDirectory, { recursive: true });
                fs.writeFileSync(
                    path.join(
                        resultsDirectory,
                        `${Buffer.from(spec.relative).toString("base64url")}.json`
                    ),
                    JSON.stringify({
                        spec: spec.relative,
                        failedTests: results.tests
                            // A beforeEach failure marks affected tests as skipped, but
                            // Cypress still attaches the setup error to each test result.
                            .filter(
                                (test) =>
                                    test.state === "failed" ||
                                    test.displayError !== null
                            )
                            .map((test) => ({
                                displayError: test.displayError,
                                title: test.title,
                            })),
                    })
                );
            });
        },
        baseUrl: "https://dev.healthgateway.gov.bc.ca",
        specPattern: "cypress/integration/**/*.{js,jsx,ts,tsx}",
    },
});
