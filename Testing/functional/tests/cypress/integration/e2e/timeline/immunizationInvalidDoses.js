const { AuthMethod, localDevUri } = require("../../../support/constants");

describe("Timeline - Immunization - Invalid Doses", () => {
    before(() => {
        cy.enableModules("Immunization");
    });

    it("Timeline - Show all doses for Partially Vaccinated with 1 Valid Dose and 2 Invalid Doses - Registered BCSC user", () => {
        const validDoseDate1 = "Jul 14";
        const invalidDoseDate1 = "Apr 2";
        const invalidDoseDate2 = "Mar 30";
        if (Cypress.config().baseUrl != localDevUri) {
            cy.login(
                Cypress.env("bcsc.20.username"),
                Cypress.env("bcsc.20.password"),
                AuthMethod.BCSC,
                "/timeline"
            );
            cy.get("[data-testid=entryCardDate]")
                .contains(validDoseDate1)
                .should("be.visible");
            cy.get("[data-testid=entryCardDate]")
                .contains(invalidDoseDate1)
                .should("be.visible");
            cy.get("[data-testid=entryCardDate]")
                .contains(invalidDoseDate2)
                .should("be.visible");
        } else {
            cy.log("Cannot test via BCSC UI Login as running locally");
        }
    });
});
