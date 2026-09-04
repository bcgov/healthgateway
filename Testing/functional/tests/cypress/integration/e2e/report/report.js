import { AuthMethod } from "../../../support/constants";
import { confirmAndVerifyReportDownload } from "../../../support/functions/report";

const defaultTimeout = 60000;

// Keep one user report as a representative E2E download flow. Dataset-specific
// report rendering and sorting are covered by fixture-backed UI specs. Add
// another E2E report only when it exercises a distinct integration path.
describe("Report download integration", () => {
    it("Downloads a Special Authority report", () => {
        cy.setupDownloads();
        cy.configureSettings({
            datasets: [{ name: "specialAuthorityRequest", enabled: true }],
        });
        cy.intercept("GET", "**/MedicationRequest/*").as(
            "getSpecialAuthorityRequests"
        );
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );

        cy.vSelect("[data-testid=report-type]", "Special Authority");
        cy.wait("@getSpecialAuthorityRequests", { timeout: defaultTimeout });
        cy.get("[data-testid=medication-request-report-table]").should(
            "be.visible"
        );

        cy.get("[data-testid=export-record-btn]").click();
        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();
        confirmAndVerifyReportDownload();
    });
});
