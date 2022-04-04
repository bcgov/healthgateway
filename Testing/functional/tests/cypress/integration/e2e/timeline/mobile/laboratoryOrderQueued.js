const { AuthMethod } = require("../../../../support/constants");

describe("Laboratory Orders Queued", () => {
    beforeEach(() => {
        cy.viewport("iphone-6");
        cy.restoreAuthCookies();
        cy.enableModules("AllLaboratory");
        cy.login(
            Cypress.env("keycloak.laboratory.queued.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Show Queued Alert Message", () => {
        cy.log("Verifying queued alert message displays");

        cy.get("[data-testid=laboratory-orders-queued-alert-message]").should(
            "be.visible"
        );
    });
});
