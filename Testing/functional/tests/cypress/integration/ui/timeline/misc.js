const { AuthMethod } = require("../../../support/constants");

describe("Miscellaneous", () => {
    beforeEach(() => {
        cy.enableModules("");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Empty Timeline", () => {
        cy.get("#listControls")
            .find(".col")
            .contains("Displaying 0 out of 0 records");
    });
});
