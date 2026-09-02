import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("Clinical Document cards", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/ClinicalDocument/*", {
            fixture: "ClinicalDocumentService/clinicalDocument.json",
        });
        cy.configureSettings({
            datasets: [{ name: "clinicalDocument", enabled: true }],
        });
        setupStandardFixtures();
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Displays clinical document details", () => {
        cy.get("[data-testid=clinicaldocumentTitle]")
            .first()
            .should("be.visible")
            .click({ force: true });
        cy.get("[data-testid=clinical-document-discipline]").should(
            "be.visible"
        );
        cy.get("[data-testid=clinical-document-facility]").should("be.visible");
    });
});
