import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("Immunization - With Refresh", () => {
    beforeEach(() => {
        let isLoading = false;
        cy.intercept("GET", "**/Immunization?hdid=*", (req) => {
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
        cy.configureSettings({
            datasets: [
                {
                    name: "immunization",
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
            });
    });
});

describe("Immunization presentation", () => {
    it("Displays an empty title and valid and invalid doses", () => {
        const emptyTitleDate = "1988-Aug-08";
        const validDoseDate1 = "2021-Jul-14";
        const invalidDoseDate1 = "2021-Mar-30";

        cy.fixture("ImmunizationService/immunizationInvalidDoses.json").then(
            (fixture) => {
                // Reuse one response to cover both presentation cases so the
                // application and fixture-backed dataset only load once.
                fixture.resourcePayload.immunizations[0].immunization.name = "";
                cy.intercept("GET", "**/Immunization?hdid=*", fixture);
            }
        );
        cy.configureSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/timeline"
        );

        cy.contains("[data-testid=immunizationTitle]", emptyTitleDate)
            .should("be.visible")
            .should("include.text", "Immunizations");
        cy.get("[data-testid=entryCardDate]")
            .contains(validDoseDate1)
            .should("be.visible");
        cy.get("[data-testid=entryCardDate]")
            .contains(invalidDoseDate1)
            .should("be.visible");
    });
});
