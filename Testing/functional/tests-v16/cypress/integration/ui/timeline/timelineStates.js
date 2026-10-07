import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("Banner Error", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Note/*", {
            fixture: "NoteService/dbError.json",
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "note",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.env(["keycloak.password"]).then(
            ({ "keycloak.password": password }) => {
                cy.login(
                    Cypress.expose("keycloak.username"),
                    password,
                    AuthMethod.KeyCloak,
                    "/timeline",
                    "default",
                    { cacheAcrossSpecs: true }
                );
            }
        );
        cy.checkTimelineHasLoaded();
    });

    it("Verify banner error", () => {
        cy.get("[data-testid=errorBanner]").should("be.visible");
        cy.get("[data-testid=errorBanner]").contains(
            "Unable to retrieve notes"
        );
        cy.get("[data-testid=errorDetailsBtn]").should("be.visible");

        cy.get("[data-testid=errorDetailsBtn]").click();

        cy.get("[data-testid=error-details-span-1]").should("be.visible");

        cy.get("[data-testid=copyToClipBoardBtn]").should("be.visible");
    });
});

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
        cy.env(["keycloak.password"]).then(
            ({ "keycloak.password": password }) => {
                cy.login(
                    Cypress.expose("keycloak.username"),
                    password,
                    AuthMethod.KeyCloak,
                    "/timeline",
                    "default",
                    { cacheAcrossSpecs: true }
                );
            }
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
