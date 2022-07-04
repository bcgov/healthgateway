const { AuthMethod } = require("../../../support/constants");

describe("Filters", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Immunization?*", {
            fixture: "ImmunizationService/immunization.json",
        });
        cy.intercept("GET", "**/Encounter/*", {
            fixture: "EncounterService/encounters.json",
        });
        cy.enableModules(["Immunization", "Encounter"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Verify filtered record count", () => {
        cy.get("[data-testid=timeline-record-count]").contains(
            "Displaying 25 out of 32 records"
        );

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=Immunization-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get("[data-testid=immunizationTitle]").should("be.visible");

        cy.get("[data-testid=timeline-record-count]").contains(
            "Displaying 9 out of 9 records"
        );

        cy.get("[data-testid=clear-filters-button]").click();
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterStartDateInput] input")
            .clear()
            .focus()
            .type("2022-JUN-09");
        cy.get("[data-testid=filterEndDateInput] input")
            .clear()
            .focus()
            .type("2022-JUN-09");
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
    });
});
