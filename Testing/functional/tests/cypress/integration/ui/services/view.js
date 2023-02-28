const servicesTestsConstants = require("./servicesTestConstants");
const { AuthMethod } = require("../../../support/constants");

describe("Authenticated Services View", () => {
    beforeEach(() => {
        cy.configureSettings({
            services: {
                enabled: true,
            },
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.visit(servicesTestsConstants.servicesUrl);
    });

    afterEach(() => {
        Cypress.session.clearAllSavedSessions();
    });

    it("The page should load correctly", () => {
        cy.url().should("include", servicesTestsConstants.servicesUrl);
    });
});
