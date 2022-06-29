const { AuthMethod } = require("../../../support/constants");

describe("Immunization - With Refresh", () => {
    beforeEach(() => {
        let isLoading = false;
        cy.intercept("GET", "**/Immunization?*", (req) => {
            if (!isLoading) {
                req.reply({
                    fixture: "ImmunizationService/immunizationrefresh.json",
                });
            } else {
                req.reply({
                    fixture: "ImmunizationService/immunization.json",
                });
            }
            isLoading = !isLoading;
        });
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate COVID-19 Immunization Attachment Icons", () => {
        cy.log(
            "All valid COVID-19 immunizations should have attachment icons."
        );
        cy.get("[data-testid=cardBtn]")
            .closest("[data-testid=timelineCard]")
            .each((card) => {
                cy.wrap(card)
                    .find("[data-testid=attachmentIcon]")
                    .should("exist");
            });

        cy.log(
            "All cards with attachment icons should be valid COVID-19 immunizations."
        );
        cy.get("[data-testid=attachmentIcon]")
            .closest("[data-testid=timelineCard]")
            .each((card) => {
                cy.wrap(card).find("[data-testid=cardBtn]").should("exist");
            });
    });

    it("Validate Card Details", () => {
        cy.get("[data-testid=cardBtn]")
            .closest("[data-testid=timelineCard]")
            .first()
            .click();
        cy.get("[data-testid=immunizationTitle]").should("be.visible");
        cy.get("[data-testid=immunizationProductTitle]").should("be.visible");
        cy.get("[data-testid=immunizationProviderTitle]").should("be.visible");
        cy.get("[data-testid=immunizationLotTitle]").should("be.visible");

        // Verify Forecast
        cy.get("[data-testid=forecastDisplayName]")
            .first()
            .should("be.visible")
            .contains("Covid-19");
        cy.get("[data-testid=forecastDueDate]").first().should("be.visible");
        cy.get("[data-testid=forecastStatus]").first().should("be.visible");
    });
});

describe("Immunization", () => {
    it("Validate Empty Title", () => {
        cy.intercept("GET", "**/Immunization?*", {
            fixture: "ImmunizationService/immunizationEmptyName.json",
        });
        cy.enableModules("Immunization");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=immunizationTitle]")
            .should("be.visible")
            .should("have.text", "Immunization");
    });

    it("Validate Provincial VaccineProof Buttons", () => {
        cy.intercept("GET", "**/Immunization?*", {
            fixture: "ImmunizationService/immunization.json",
        });
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/timeline"
        );
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=cardBtn]").should("have.attr", "href", "/covid19");
    });
});
