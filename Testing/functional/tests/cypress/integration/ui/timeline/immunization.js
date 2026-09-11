import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("Immunization presentation", () => {
    it("Displays an empty title, valid and invalid doses, and forecast details", () => {
        const emptyTitleDate = "1988-Aug-08";
        const validDoseDate1 = "2021-Jul-14";
        const invalidDoseDate1 = "2021-Mar-30";
        const forecastImmunization = "COVID-19 Non-replicating Viral Vector";
        const forecastDueDate = "2021-08-11";

        cy.fixture("ImmunizationService/immunizationInvalidDoses.json").then(
            (fixture) => {
                // Reuse one response to cover both presentation cases so the
                // application and fixture-backed dataset only load once.
                fixture.immunizations[0].immunization.name = "";
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

        cy.contains("[data-testid=timelineCard]", validDoseDate1)
            .click()
            .within(() => {
                cy.contains("h3", "Forecast").should("be.visible");
                cy.get("[data-testid=forecastDisplayName]").should(
                    "contain.text",
                    forecastImmunization
                );
                cy.get("[data-testid=forecastDueDate]").should(
                    "contain.text",
                    forecastDueDate
                );
            });

        cy.contains("[data-testid=timelineCard]", invalidDoseDate1)
            .click()
            .within(() => {
                cy.contains("h3", "Forecast").should("not.exist");
                cy.get("[data-testid=forecastDisplayName]").should("not.exist");
                cy.get("[data-testid=forecastDueDate]").should("not.exist");
            });
    });
});
