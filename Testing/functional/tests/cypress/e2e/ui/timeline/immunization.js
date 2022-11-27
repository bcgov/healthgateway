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

    it("Validate Card Details", () => {
        cy.get("[data-testid=timelineCard]")
            .first()
            .click()
            .within(() => {
                cy.get("[data-testid=immunizationTitle]").should("be.visible");
                cy.get("[data-testid=immunizationProductTitle]").should(
                    "be.visible"
                );
                cy.get("[data-testid=immunizationProviderTitle]").should(
                    "be.visible"
                );
                cy.get("[data-testid=immunizationLotTitle]").should(
                    "be.visible"
                );

                // Verify Forecast
                cy.get("[data-testid=forecastDisplayName]")
                    .first()
                    .should("be.visible")
                    .contains("Covid-19");
                cy.get("[data-testid=forecastDueDate]")
                    .first()
                    .should("be.visible");
                cy.get("[data-testid=forecastStatus]")
                    .first()
                    .should("be.visible");
            });
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
});

describe("Timeline - Immunization - Invalid Doses", () => {
    it("Timeline - Partially Vaccinated 1 Valid Dose and 1 Invalid Dose", () => {
        const validDoseDate1 = "2021-Jul-14";
        const invalidDoseDate1 = "2021-Mar-30";

        cy.intercept("GET", "**/Immunization?*", {
            fixture: "ImmunizationService/immunizationInvalidDoses.json",
        });
        cy.enableModules(["Immunization"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/timeline"
        );

        cy.get("[data-testid=entryCardDate]")
            .contains(validDoseDate1)
            .should("be.visible");
        cy.get("[data-testid=entryCardDate]")
            .contains(invalidDoseDate1)
            .should("be.visible");
    });
});
