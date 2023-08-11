const { AuthMethod } = require("../../../support/constants");

describe("MSP Visits", () => {
    beforeEach(() => {
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

    it("Validate Encounter Card Details", () => {
        cy.get("[data-testid=timelineCard")
            .first()
            .within(() => {
                cy.get("[data-testid=healthvisitTitle]")
                    .should("be.visible")
                    .click({ force: true });
                cy.get("[data-testid=encounterClinicName]").should(
                    "be.visible"
                );
            });
    });
});

describe("Hospital Visits", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "hospitalVisit",
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

    it("Validate Hospital Visit Card Details", () => {
        cy.get("[data-testid=timelineCard")
            .first()
            .within(() => {
                cy.get("[data-testid=hospitalvisitTitle]")
                    .first()
                    .click({ force: true });
                cy.get("[data-testid=hospital-visit-location]").should(
                    "be.visible"
                );
                cy.get("[data-testid=hospital-visit-provider]").should(
                    "be.visible"
                );
                cy.get("[data-testid=hospital-visit-service]").should(
                    "be.visible"
                );
                cy.get("[data-testid=hospital-visit-date]").should(
                    "be.visible"
                );
                cy.get("[data-testid=hospital-visit-discharge-date]").should(
                    "be.visible"
                );
            });
    });
});
