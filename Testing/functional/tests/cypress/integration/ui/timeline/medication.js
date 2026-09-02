import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("Medication cards", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/MedicationStatement/*", {
            fixture: "MedicationService/medicationStatement.json",
        });
        cy.configureSettings({
            datasets: [{ name: "medication", enabled: true }],
        });
        setupStandardFixtures();
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Displays prescription and pharmacist assessment details", () => {
        cy.get("[data-testid=medicationTitle]")
            .not(":contains('Pharmacist Assessment')")
            .first()
            .click({ force: true });
        cy.get("[data-testid=medication-practitioner]").should("be.visible");
        cy.get("[data-testid=medication-directions]").should("be.visible");
        cy.get("[data-testid=pharmacist-outcome]").should("not.exist");

        cy.contains("[data-testid=medicationTitle]", "Pharmacist Assessment")
            .first()
            .click({ force: true });
        cy.get("[data-testid=pharmacist-outcome]").should("be.visible");
    });
});
