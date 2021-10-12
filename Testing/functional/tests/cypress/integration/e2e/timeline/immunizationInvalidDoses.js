const { AuthMethod, localDevUri } = require("../../../support/constants");

describe("Timeline - Immunization - Invalid Doses", () => {
    before(() => {
        cy.enableModules("Immunization");
    });

    it("Timeline - Partially Vaccinated 1 Valid Dose and 2 Invalid Doses - Keycloak user", () => {
        const validDoseDate1 = "Jul 14";
        const invalidDoseDate1 = "Apr 2";
        const invalidDoseDate2 = "Mar 30";

        cy.login(
            Cypress.env("keycloak.invaliddoses.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
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
    });
});
