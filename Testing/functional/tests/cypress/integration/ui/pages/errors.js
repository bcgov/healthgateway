import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("Error Pages", () => {
    beforeEach(() => {
        cy.configureSettings({});
        cy.logout();
    });

    it("HTTP 401", () => {
        cy.visit("/unauthorized");
        cy.location("pathname").should("eq", "/unauthorized");
        cy.contains("h1", "401").should("be.visible");
    });

    it("HTTP 404", () => {
        cy.visit("/nonexistentpage");
        cy.location("pathname").should("eq", "/nonexistentpage");
        cy.contains("h1", "404").should("be.visible");
    });
});

describe("Banner Error", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Note/*", {
            fixture: "NoteService/dbError.json",
        }).as("getNotes");
        cy.configureSettings({
            datasets: [
                {
                    name: "note",
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
        cy.wait("@getNotes");
        cy.checkTimelineHasLoaded();
    });

    it("Shows error details when notes retrieval fails", () => {
        cy.get("[data-testid=errorBanner]").should("be.visible");
        cy.get("[data-testid=errorBanner]").contains(
            "Unable to retrieve notes"
        );
        cy.get("[data-testid=errorDetailsBtn]").should("be.visible").click();
        cy.get("[data-testid=error-details-span-1]").should("be.visible");
        cy.get("[data-testid=copyToClipBoardBtn]").should("be.visible");
    });
});
