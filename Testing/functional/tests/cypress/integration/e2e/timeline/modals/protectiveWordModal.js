const { AuthMethod } = require("../../../../support/constants");

describe("Protective Word Modal", () => {
    it("Validates the protective word and retrieves medication records", () => {
        cy.configureSettings({
            datasets: [{ name: "medication", enabled: true }],
        });
        cy.login(
            Cypress.env("keycloak.protected.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.contains(
            "[data-testid=protectiveWordModal]",
            "Restricted PharmaNet Records"
        ).should("be.visible");
        cy.get("[data-testid=protectiveWordModalText]")
            .should(
                "contain",
                "Please enter the protective word required to access these restricted PharmaNet records."
            )
            .and("contain", "For more information visit");
        cy.get("[data-testid=protectiveWordModalRulesHREF]")
            .should("be.visible")
            .and(
                "have.attr",
                "href",
                "https://www2.gov.bc.ca/gov/content/health/health-drug-coverage/pharmacare-for-bc-residents/pharmanet/protective-word-for-a-pharmanet-record"
            );
        cy.get("[data-testid=protectiveWordContinueBtn]").should("be.disabled");

        cy.get("[data-testid=protectiveWordInput] input").type("WRONGKEYWORK");
        cy.get("[data-testid=protectiveWordContinueBtn]")
            .should("be.enabled")
            .click();
        cy.contains(
            "[data-testid=protectiveWordInput]",
            "Invalid protective word. Try again."
        ).should("be.visible");

        cy.get("[data-testid=protectiveWordInput] input")
            .clear()
            .type("KEYWORD");
        cy.get("[data-testid=protectiveWordContinueBtn]")
            .should("be.enabled")
            .click();
        cy.get("[data-testid=protectiveWordModal]").should("not.exist");
        cy.contains("Retrieving your health records");
    });
});
