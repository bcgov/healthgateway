import {
    configureFakeSettings,
    fakeLogin,
    useFakeEnvironment,
} from "../../../support/functions/fakeEnvironment";
import { setupStandardFixtures } from "../../../support/functions/intercept";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();

describe("MSP Visits", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Encounter/*", (req) => {
            req.reply({
                fixture: "EncounterService/encountersrolloff.json",
            });
        }).as("getEncounters");
        configureFakeSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        fakeLogin("/timeline");
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
        let cards = cy.get("[data-testid=timelineCard]");
        cards.first().click();
        cy.get("[data-testid=encounterRolloffAlert]").should("not.exist");
    });

    it("Verify rolloff message visible", () => {
        let cards = cy.get("[data-testid=timelineCard]");
        cards.last().click();
        cy.get("[data-testid=encounterRolloffAlert]").should("be.visible");
    });
});

describe("Hospital Visits", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Encounter/HospitalVisit/*", {
            fixture: "EncounterService/hospitalVisits.json",
        }).as("getHospitalVisitsFixture");
        configureFakeSettings({
            datasets: [{ name: "hospitalVisit", enabled: true }],
        });
        setupStandardFixtures();
        fakeLogin("/timeline");
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
