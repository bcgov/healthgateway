const { AuthMethod } = require("../../../support/constants");
const homeUrl = "/home";
const timelineUrl = "/timeline";

describe("Home Page", () => {
    beforeEach(() => {
        cy.enableModules([
            "MedicationRequest",
            "Medication",
            "Immunization",
            "Covid19LaboratoryOrder",
            "LaboratoryOrder",
            "Encounter",
            "Note",
        ]);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );
    });

    it("Home - Health Records Card link to Timeline", () => {
        cy.contains("[data-testid=card-button-title]", "Health Records")
            .parents("[data-testid=health-records-card]")
            .should("be.visible", "be.enabled")
            .click();

        cy.url().should("include", timelineUrl);
        // When a result is returned, the content placeholder will not be displayed.
        cy.get("[data-testid=content-placeholders]").should("be.visible");
        cy.get("[data-testid=loading-toast]").should("be.visible");
        cy.get("[data-testid=content-placeholders]").should("not.exist");
        cy.get("[data-testid=loading-toast]").should("be.visible");
        cy.get("[data-testid=displayCountText]").should("be.visible");
    });

    it("Home - Medication Card link to Timeline", () => {
        cy.contains("[data-testid=card-button-title]", "Medications")
            .parents("[data-testid=quick-link-card]")
            .should("be.visible", "be.enabled")
            .click();

        cy.url().should("include", timelineUrl);
        // Medication takes the longest to return results. The content placeholder will dispaly until Medication finishes.
        cy.get("[data-testid=content-placeholders]").should("be.visible");
        cy.get("[data-testid=loading-toast]").should("be.visible");
        cy.get("[data-testid=content-placeholders]").should("not.exist");
        cy.get("[data-testid=loading-toast]").should("not.exist");
        cy.get("[data-testid=displayCountText]").should("be.visible");
    });

    it("Home - Side Menu Timeline link to Timeline", () => {
        cy.get("[data-testid=timelineLabel]")
            .should("be.visible", "be.enabled")
            .click();

        cy.url().should("include", timelineUrl);
        // When a result is returned, the content placeholder will not be displayed.
        cy.get("[data-testid=content-placeholders]").should("be.visible");
        cy.get("[data-testid=loading-toast]").should("be.visible");
        cy.get("[data-testid=content-placeholders]").should("not.exist");
        cy.get("[data-testid=loading-toast]").should("be.visible");
        cy.get("[data-testid=displayCountText]").should("be.visible");
    });
});
