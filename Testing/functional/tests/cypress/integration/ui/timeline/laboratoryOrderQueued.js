import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("Laboratory Orders Queued", () => {
    beforeEach(() => {
        // A queued response is a ClientApp display state, so a fixture provides
        // deterministic coverage without requiring the PHSA queued-test account.
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrdersQueued.json",
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "labResult",
                    enabled: true,
                },
            ],
        });
        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Shows the queued alert and an empty timeline", () => {
        cy.get("[data-testid=laboratory-orders-queued-alert-message]").should(
            "be.visible"
        );
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
    });
});
