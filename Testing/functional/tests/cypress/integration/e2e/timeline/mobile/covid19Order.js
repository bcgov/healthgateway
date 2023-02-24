const { AuthMethod } = require("../../../../support/constants");

describe("COVID-19 Orders", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "covid19TestResult",
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

    it("Validate Download", () => {
        cy.contains("[data-testid=entryCardDate]", "2020-Dec-03")
            .first()
            .scrollIntoView()
            .should("be.visible")
            .parents("[data-testid=timelineCard]")
            .click();

        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=covid-result-download-btn]")
                    .should("be.visible")
                    .click({ force: true });
            });

        cy.get("[data-testid=genericMessageSubmitBtn]")
            .should("be.visible")
            .click({ force: true });

        cy.verifyDownload("COVID_Result_2020_12_03-02_00.pdf");

        cy.get("[data-testid=backBtn]").click({ force: true });
    });
});
