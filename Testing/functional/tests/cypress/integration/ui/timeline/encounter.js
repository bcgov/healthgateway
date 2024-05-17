import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("MSP Visits Rolloff", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Encounter/*", (req) => {
            req.reply({
                fixture: "EncounterService/encountersrolloff.json",
            });
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
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

    it("Verify rolloff message not visible", () => {
        let cards = cy.get("[data-testid=timelineCard");
        cards.first().click();
        cy.get("[data-testid=encounterRolloffAlert]").should("not.exist");
    });

    it("Verify rolloff message visible", () => {
        let cards = cy.get("[data-testid=timelineCard");
        cards.last().click();
        cy.get("[data-testid=encounterRolloffAlert]").should("be.visible");
    });
});
