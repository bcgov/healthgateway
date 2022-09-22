const { AuthMethod } = require("../../../support/constants");

describe("Export Reports - Immunizations - Invalid Doses", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Immunization?*", {
            fixture: "ImmunizationService/immunizationInvalidDoses.json",
        });
        cy.enableModules(["Immunization"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Immunization Report - Partially Vaccinated 1 Valid Dose and 1 Invalid Dose - Keycloak user", () => {
        const validDoseDate1 = "2021-Jul-14";
        const invalidDoseDate1 = "2021-Mar-30";

        cy.get("[data-testid=reportType]").select("Immunizations");

        cy.get("[data-testid=immunizationDateItem]", { timeout: 60000 })
            .contains(validDoseDate1)
            .should("be.visible");
        cy.get("[data-testid=immunizationDateItem]")
            .contains(invalidDoseDate1)
            .should("be.visible");
    });
});
