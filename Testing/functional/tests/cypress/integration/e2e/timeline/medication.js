const { AuthMethod } = require("../../../support/constants");

describe("Medication", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "medication",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card Details", () => {
        cy.get("[data-testid=medicationTitle]").should("be.visible");
        cy.get("[data-testid=medication-practitioner]").should("not.exist");

        cy.get("[data-testid=medicationTitle]")
            .not(":contains('Pharmacist Assessment')")
            .first()
            .scrollIntoView()
            .click({ force: true });
        cy.get("[data-testid=medication-practitioner]").should("be.visible");
        cy.get("[data-testid=medication-directions]").should("be.visible");
        cy.get("[data-testid=pharmacist-outcome]").should("not.exist");

        cy.contains("[data-testid=medicationTitle]", "Pharmacist Assessment")
            .first()
            .scrollIntoView()
            .click({ force: true });
        cy.get("[data-testid=pharmacist-outcome]").should("be.visible");
    });
});
