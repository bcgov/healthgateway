const { AuthMethod } = require("../../../support/constants");

describe("Export Reports - Immunizations - Invalid Doses", () => {
    before(() => {
        cy.enableModules("Immunization");
    });

    it("Immunization Report - Partially Vaccinated 1 Valid Dose and 1 Invalid Dose - Keycloak user", () => {
        const validDoseDate1 = "2021-Jul-14";
        const invalidDoseDate1 = "2021-Mar-30";

        cy.login(
            Cypress.env("keycloak.invaliddoses.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );

        cy.get("[data-testid=reportType]")
            .should("be.enabled", "be.visible")
            .select("Immunizations");

        cy.get("[data-testid=timelineLoading]").should("be.visible");

        cy.get("[data-testid=immunizationDateItem]", { timeout: 60000 })
            .contains(validDoseDate1)
            .should("be.visible");
        cy.get("[data-testid=immunizationDateItem]")
            .contains(invalidDoseDate1)
            .should("be.visible");
    });
});
