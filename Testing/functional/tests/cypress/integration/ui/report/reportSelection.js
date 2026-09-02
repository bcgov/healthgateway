import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("Report selection", () => {
    it("Enables export after a report type is selected", () => {
        cy.intercept("GET", "**/MedicationStatement/*", {
            fixture: "MedicationService/medicationStatement.json",
        }).as("getMedicationFixture");
        cy.configureSettings({
            datasets: [{ name: "medication", enabled: true }],
        });
        setupStandardFixtures();
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );

        cy.get("[data-testid=export-record-btn]").should("be.disabled");
        cy.get("[data-testid=info-text]").should("be.visible");
        cy.get("[data-testid=info-image]").should("be.visible");

        cy.vSelect("[data-testid=report-type]", "Medications");
        cy.wait("@getMedicationFixture");
        cy.get("[data-testid=medication-history-report-table]").should(
            "be.visible"
        );
        cy.get("[data-testid=export-record-btn]").should("be.enabled");
    });
});
