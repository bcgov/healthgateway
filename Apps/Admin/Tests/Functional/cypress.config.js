const { defineConfig } = require("cypress");
const { isFileExist, findFiles } = require("cy-verify-downloads");

module.exports = defineConfig({
    projectId: "rccf87",
    e2e: {
        baseUrl: "https://dev-admin.healthgateway.gov.bc.ca",
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
            keycloak_username: "blazoradmin",
            keycloak_password: "",
            keycloak_unauthorized_username: "healthgateway",
        },
        trashAssetsBeforeRuns: true,
        setupNodeEvents(on, config) {
            on("task", { isFileExist, findFiles });
        },
        experimentalSessionAndOrigin: true,
    },
});
