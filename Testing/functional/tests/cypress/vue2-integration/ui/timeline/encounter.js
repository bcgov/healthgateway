const { AuthMethod } = require("../../../support/constants");

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
        cy.get("[data-testid=encounterRolloffAlert]").should("not.be.visible");
    });

    it("Verify rolloff message visible", () => {
        let cards = cy.get("[data-testid=timelineCard");
        cards.last().click();
        cy.get("[data-testid=encounterRolloffAlert]").should("be.visible");
    });
});
