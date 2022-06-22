const { defineConfig } = require("cypress");
const { isFileExist, findFiles } = require("cy-verify-downloads");

module.exports = defineConfig({
    e2e: {
        baseUrl: "https://mock-admin.healthgateway.gov.bc.ca",
        defaultCommandTimeout: 30000,
        blockHosts: ["spt.apps.gov.bc.ca"],
        specPattern: "cypress/integration/**/*.cy.{js,jsx,ts,tsx}",
        retries: {
            runMode: 1,
            openMode: 0,
        },
        viewportWidth: 1920,
        viewportHeight: 1080,
        env: {
            idir_username: "hgateway",
            idir_password: "",
        },
        trashAssetsBeforeRuns: true,
        setupNodeEvents(on, config) {
            on("task", { isFileExist, findFiles });
        },
        video: false,
    },
});
