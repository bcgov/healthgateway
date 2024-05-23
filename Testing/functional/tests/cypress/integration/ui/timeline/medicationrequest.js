import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("Medication Request", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "specialAuthorityRequest",
                    enabled: true,
                },
            ],
        });
        cy.intercept("GET", "**/MedicationRequest/*", {
            fixture: "MedicationService/medicationRequest.json",
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card Details", () => {
        cy.get("[data-testid=specialauthorityrequestTitle]").should(
            "be.visible"
        );
        cy.get("[data-testid=medicationPractitioner]").should("not.exist");
        cy.get("[data-testid=entryCardDetailsTitle]")
            .first()
            .click({ force: true });
        cy.get("[data-testid=medicationPractitioner]").should("be.visible");
    });
});
