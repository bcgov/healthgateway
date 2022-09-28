const { AuthMethod } = require("../../../support/constants");

describe("Banner Error", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Note/*", {
            fixture: "WebClientService/dbError.json",
        });
        cy.enableModules("Note");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Verify banner error", () => {
        cy.get("[data-testid=singleErrorHeader]").should("be.visible");
        cy.get("[data-testid=singleErrorHeader]").contains(
            "Unable to retrieve notes"
        );
        cy.get("[data-testid=errorDetailsBtn]").should("be.visible");
        cy.get("[data-testid=viewDetailsIcon]").should("be.visible");

        cy.get("[data-testid=errorDetailsBtn]").click();

        cy.get("[data-testid=viewDetailsIcon]").should("be.not.visible");
        cy.get("[data-testid=hideDetailsIcon]").should("be.visible");

        cy.get("[data-testid=error-details-span-1]").should("be.visible");

        cy.get("[data-testid=copyToClipBoardBtn]").should("be.visible").click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageOkBtn]").click();
        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });
});
