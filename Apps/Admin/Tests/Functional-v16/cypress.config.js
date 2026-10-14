const { defineConfig } = require("cypress");
const { verifyDownloadTasks } = require("cy-verify-downloads");

module.exports = defineConfig({
    defaultBrowser: "chrome",
    projectId: "rccf87",
    defaultCommandTimeout: 30000,
    blockHosts: ["spt.apps.gov.bc.ca"],
    retries: {
        runMode: 1,
        openMode: 0,
    },
    viewportWidth: 1920,
    viewportHeight: 1080,
    expose: {
        idir_username: "hgateway",
        keycloak_username: "blazoradmin",
        keycloak_reviewer_username: "blazorreviewer",
        keycloak_support_username: "blazorsupport",
        keycloak_unauthorized_username: "healthgateway",
    },
    env: {
        idir_password: "",
        keycloak_password: "",
        keycloak_admin_client: "hg-keycloak",
        keycloak_admin_secret: "",
    },
    trashAssetsBeforeRuns: true,
    e2e: {
        setupNodeEvents(on, config) {
            on("task", verifyDownloadTasks);
            for (const key of Object.keys(config.expose)) {
                if (Object.hasOwn(config.env, key)) {
                    config.expose[key] = config.env[key];
                }
            }
            return config;
        },
        baseUrl: "https://dev-admin.healthgateway.gov.bc.ca",
        specPattern: "cypress/integration/**/*.cy.{js,jsx,ts,tsx}",
    },
});