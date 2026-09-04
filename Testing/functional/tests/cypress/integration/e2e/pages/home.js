import { AuthMethod } from "../../../support/constants";
import { prepareImmunizationWait } from "../../../support/functions/timeline";

const homeUrl = "/home";
const reportsUrl = "/reports";
const healthGatewayHdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3JLWMGUMERKI72A";
const recommendationsHdid =
    "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";

describe("Home Page", () => {
    it("Home - Immunization Card Link to Download Immunization", () => {
        cy.configureSettings({
            homepage: {
                showImmunizationRecordLink: true,
            },
            datasets: [{ name: "immunization", enabled: true }],
        });
        const waitForImmunizations = prepareImmunizationWait(healthGatewayHdid);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=immunization-record-card-button]")
            .should("be.visible")
            .click();

        cy.location("pathname").should("eq", reportsUrl);
        waitForImmunizations();
        cy.get("[data-testid=immunization-history-report-table]").should(
            "be.visible"
        );
        cy.get("[data-testid=immunizationDateItem]")
            .last()
            .contains(/\d{4}-[A-Z][a-z]{2}-\d{2}/);
    });

    it("Home - Link should open recommendations dialog", () => {
        cy.configureSettings({
            homepage: {
                showRecommendationsLink: true,
            },
            datasets: [{ name: "immunization", enabled: true }],
        });
        const waitForImmunizations =
            prepareImmunizationWait(recommendationsHdid);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=recommendations-card-button]")
            .should("be.visible")
            .click();
        waitForImmunizations();
        cy.get("[data-testid=recommendation-history-report-table]")
            .should("be.visible")
            .find("tbody tr")
            .should("have.length.at.least", 1);
    });
});
