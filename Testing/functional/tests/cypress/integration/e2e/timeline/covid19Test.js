const { AuthMethod } = require("../../../support/constants");
const {
    validateAttachmentDownload,
    validateFileDownload,
} = require("../../../support/functions/timeline");

describe("COVID-19 Test Results", () => {
    beforeEach(() => {
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
        cy.checkTimelineHasLoaded();
    });

    it("Validate file download", () => {
        cy.contains("[data-testid=entryCardDate]", "2020-Dec-03")
            .parents("[data-testid=timelineCard]")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                validateFileDownload("[data-testid=covid-result-download-btn]");
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
