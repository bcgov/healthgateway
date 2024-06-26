const { defineConfig } = require("cypress");
const { verifyDownloadTasks } = require("cy-verify-downloads");

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
        "keycloak.hthgtwy20.username": "hthgtwy20",
        "keycloak.laboratory.queued.username": "hthgtwy09",
        "keycloak.notfound.username": "hthgtwy03",
        "keycloak.protected.username": "protected",
        "keycloak.unregistered.username": "hthgtwy02",
        "idir.username": "hgateway",
        "idir.password": "",
        "keycloak.phsa.client": "",
        "keycloak.phsa.secret": "",
        phoneNumber: "",
        emailAddress: "fakeemail@healthgateway.gov.bc.ca",
        phn: "9735353315",
    },
    projectId: "ofnepc",
    trashAssetsBeforeRuns: true,
    e2e: {
        setupNodeEvents(on, _config) {
            on("task", verifyDownloadTasks);
        },
        baseUrl: "https://dev-classic.healthgateway.gov.bc.ca",
        specPattern: "cypress/integration/**/*.{js,jsx,ts,tsx}",
    },
});
