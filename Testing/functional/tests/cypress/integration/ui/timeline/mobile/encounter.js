import { AuthMethod } from "../../../../support/constants";
import { setupStandardFixtures } from "../../../../support/functions/intercept";

function configureAndLogin(dataset, endpoint, fixture, alias) {
    cy.intercept("GET", endpoint, { fixture }).as(alias);
    cy.configureSettings({
        datasets: [{ name: dataset, enabled: true }],
    });
    setupStandardFixtures();
    cy.viewport("iphone-6");
    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak
    );
    cy.wait(`@${alias}`);
    cy.checkTimelineHasLoaded();
}

describe("Encounter cards for mobile", () => {
    it("Displays MSP visit details", () => {
        configureAndLogin(
            "healthVisit",
            "**/Encounter/*",
            "EncounterService/encounters.json",
            "getEncountersFixture"
        );

        cy.get("[data-testid=timelineCard]").first().click();
        cy.get("[data-testid=entryDetailsCard]").within(() => {
            cy.get("[data-testid=entryCardDetailsTitle]").should("be.visible");
            cy.get("[data-testid=entryCardDate]").should("be.visible");
            cy.get("[data-testid=encounterClinicName]").should("be.visible");
        });
        cy.get("[data-testid=backBtn]").should("be.visible");
    });

    it("Displays hospital visit details", () => {
        configureAndLogin(
            "hospitalVisit",
            "**/Encounter/HospitalVisit/*",
            "EncounterService/hospitalVisits.json",
            "getHospitalVisitsFixture"
        );

        cy.get("[data-testid=hospitalvisitTitle]")
            .first()
            .click({ force: true });
        cy.get("[data-testid=entryDetailsCard]").within(() => {
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
});
