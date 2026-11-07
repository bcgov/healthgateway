import {
    configureFakeSettings,
    fakeLogin,
    useFakeEnvironment,
} from "../../../support/functions/fakeEnvironment";
import { setupStandardFixtures } from "../../../support/functions/intercept";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();

describe("Report selection", () => {
    it("Enables export after a report type is selected", () => {
        cy.intercept("GET", "**/MedicationStatement/*", {
            fixture: "MedicationService/medicationStatement.json",
        }).as("getMedicationFixture");
        configureFakeSettings({
            datasets: [{ name: "medication", enabled: true }],
        });
        setupStandardFixtures();
        fakeLogin("/reports");

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
