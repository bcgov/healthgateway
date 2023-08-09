const { AuthMethod } = require("../../../support/constants");

describe("Communication", () => {
    beforeEach(() => {
        cy.configureSettings({});
    });

    it("Landing Banner", () => {
        cy.logout();
        cy.visit("/");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("Test Banner");

        cy.visit("/release-notes");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("Test Banner");

        cy.visit("/termsOfService");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("Test Banner");

        cy.visit("/404");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("Test Banner");
    });

    it("InApp Banner", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");

        cy.visit("/dependents");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");

        cy.visit("/reports");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");

        cy.visit("/profile");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");
    });
});
