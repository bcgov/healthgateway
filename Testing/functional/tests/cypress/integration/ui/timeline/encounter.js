import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("MSP Visits", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Encounter/*", (req) => {
            req.reply({
                fixture: "EncounterService/encountersrolloff.json",
            });
        }).as("getEncounters");
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
        cy.wait("@getEncounters");
        cy.checkTimelineHasLoaded();
    });

    it("Displays encounter card details", () => {
        cy.get("[data-testid=timelineCard]")
            .first()
            .within(() => {
                cy.get("[data-testid=healthvisitTitle]").click({ force: true });
                cy.get("[data-testid=encounterClinicName]").should(
                    "be.visible"
                );
            });
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

describe("Hospital Visits", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Encounter/HospitalVisit/*", {
            fixture: "EncounterService/hospitalVisits.json",
        }).as("getHospitalVisitsFixture");
        cy.configureSettings({
            datasets: [{ name: "hospitalVisit", enabled: true }],
        });
        setupStandardFixtures();
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.wait("@getHospitalVisitsFixture");
        cy.checkTimelineHasLoaded();
    });

    it("Displays hospital visit card details", () => {
        cy.get("[data-testid=hospitalvisitTitle]")
            .first()
            .click({ force: true });
        [
            "hospital-visit-location",
            "hospital-visit-provider",
            "hospital-visit-service",
            "hospital-visit-date",
            "hospital-visit-discharge-date",
        ].forEach((testId) => {
            cy.get(`[data-testid=${testId}]`).should("be.visible");
        });
    });
});
