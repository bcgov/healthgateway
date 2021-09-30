const { AuthMethod, localDevUri } = require("../../../support/constants");

describe("Export Reports - Immunizations - Invalid Doses", () => {
    before(() => {
        cy.enableModules("Immunization");
    });

    it("Immunization Report - Show all doses for Partially Vaccinated with 1 Valid Dose and 2 Invalid Doses - Registered BCSC user", () => {
        const validDoseDate1 = "2021-Jul-14";
        const invalidDoseDate1 = "2021-Mar-30";
        const invalidDoseDate2 = "2021-Apr-02";
        if (Cypress.config().baseUrl != localDevUri) {
            cy.login(
                Cypress.env("bcsc.20.username"),
                Cypress.env("bcsc.20.password"),
                AuthMethod.BCSC,
                "/reports"
            );
            cy.get("[data-testid=reportType]")
                .should("be.enabled", "be.visible")
                .select("Immunizations");
            cy.get("[data-testid=timelineLoading]").should("be.visible");
            cy.get("[data-testid=timelineLoading]").should("not.be.visible");
            cy.get("[data-testid=immunizationDateItem]")
                .contains(validDoseDate1)
                .should("be.visible");
            cy.get("[data-testid=immunizationDateItem]")
                .contains(invalidDoseDate1)
                .should("be.visible");
            cy.get("[data-testid=immunizationDateItem]")
                .contains(invalidDoseDate2)
                .should("be.visible");
        } else {
            cy.log("Cannot test via BCSC UI Login as running locally");
        }
    });
});
