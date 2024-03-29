const { AuthMethod } = require("../../../../support/constants");

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
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card Details for Mobile", () => {
        cy.get("[data-testid=timelineCard]").first().click();
        const entryDetails = cy.get("[data-testid=entryDetailsCard]");
        cy.get("[data-testid=backBtn]").should("be.visible");
        entryDetails
            .get("[data-testid=entryCardDetailsTitle]")
            .should("be.visible");
        entryDetails.get("[data-testid=entryCardDate]").should("be.visible");
        entryDetails
            .get("[data-testid=encounterClinicName")
            .should("be.visible");
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
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Hospital Visit Card Details", () => {
        cy.get("[data-testid=hospitalvisitTitle]").should("be.visible");
        cy.get("[data-testid=entryCardDetailsTitle")
            .first()
            .click({ force: true });
        cy.get("[data-testid=hospital-visit-location").should("be.visible");
        cy.get("[data-testid=hospital-visit-provider").should("be.visible");
        cy.get("[data-testid=hospital-visit-service").should("be.visible");
        cy.get("[data-testid=hospital-visit-date").should("be.visible");
        cy.get("[data-testid=hospital-visit-discharge-date").should(
            "be.visible"
        );
    });
});
