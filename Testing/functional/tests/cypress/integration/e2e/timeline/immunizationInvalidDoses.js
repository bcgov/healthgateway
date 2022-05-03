const { AuthMethod } = require("../../../support/constants");

describe("Timeline - Immunization - Invalid Doses", () => {
    beforeEach(() => {
        cy.enableModules("Immunization");
        cy.login(
            Cypress.env("keycloak.invaliddoses.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/timeline"
        );
    });

    it("Timeline - Partially Vaccinated 1 Valid Dose and 1 Invalid Dose - Keycloak user", () => {
        const validDoseDate1 = "2021-Jul-14";
        const invalidDoseDate1 = "2021-Mar-30";

        cy.get("[data-testid=entryCardDate]")
            .contains(validDoseDate1)
            .should("be.visible");
        cy.get("[data-testid=entryCardDate]")
            .contains(invalidDoseDate1)
            .should("be.visible");
    });
});
