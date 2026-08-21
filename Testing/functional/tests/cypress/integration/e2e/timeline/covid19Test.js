const { AuthMethod } = require("../../../support/constants");
const {
    validateAttachmentDownload,
    validateFileDownload,
} = require("../../../support/functions/timeline");

describe("COVID-19 Test Results", () => {
    function waitForCovid19TestResults() {
        return cy
            .wait("@getCovid19TestResults", { timeout: 120000 })
            .its("response.statusCode")
            .should("eq", 200);
    }

    beforeEach(() => {
        cy.intercept("GET", "**/Laboratory/Covid19Orders?hdid=*").as(
            "getCovid19TestResults"
        );
        cy.configureSettings({
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        waitForCovid19TestResults();
        cy.checkTimelineHasLoaded();
    });

    it("Validate file download", () => {
        cy.contains("[data-testid=entryCardDate]", "2020-Dec-03")
            .parents("[data-testid=timelineCard]")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                validateFileDownload(
                    "[data-testid=covid-result-download-btn]",
                    true,
                    60000
                );
            });
    });

    it("Validate attachment download", () => {
        cy.contains("[data-testid=entryCardDate]", "2020-Dec-03")
            .parents("[data-testid=timelineCard]")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                validateAttachmentDownload();
            });
    });
});
