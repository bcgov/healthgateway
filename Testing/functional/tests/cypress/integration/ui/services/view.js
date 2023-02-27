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

    // Minimum to test skeleton of services page
    it("View should show title and header", () => {
        cy.get("h1#subject").should("have.text", "Services");
        cy.get("h5.my-3").should("be.visible");
        cy.url().should("include", servicesTestsConstants.servicesUrl);
    });
});
