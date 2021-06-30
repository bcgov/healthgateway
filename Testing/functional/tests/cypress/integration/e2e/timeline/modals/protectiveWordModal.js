const { AuthMethod } = require("../../../../support/constants");

describe("Validate Modals Popup", () => {
    it("Protective Word Modal", () => {
        cy.enableModules("Medication");
        cy.login(
            Cypress.env("keycloak.protected.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
        cy.get("[data-testid=protectiveWordModal]").contains(
            "Restricted PharmaNet Records"
        );
        cy.get("[data-testid=protectiveWordModalText]").contains(
            "Please enter the protective word required to access these restricted PharmaNet records."
        );
        cy.get("[data-testid=protectiveWordModalMoreInfoText]").contains(
            "For more information visit"
        );
        cy.get("[data-testid=protectiveWordModalRulesHREF]")
            .should("be.visible")
            .should(
                "have.attr",
                "href",
                "https://www2.gov.bc.ca/gov/content/health/health-drug-coverage/pharmacare-for-bc-residents/pharmanet/protective-word-for-a-pharmanet-record"
            )
            .contains("protective-word-for-a-pharmanet-record");
        cy.get("[data-testid=protectiveWordContinueBtn]")
            .should("be.disabled")
            .contains("Continue");
        cy.get("[data-testid=protectiveWordModalErrorText]").should(
            "not.exist"
        );
        cy.get("[data-testid=protectiveWordInput]")
            .should("be.enabled")
            .type("WRONGKEYWORK");
        cy.get("[data-testid=protectiveWordContinueBtn]")
            .should("be.enabled")
            .click()
            .wait(250);
        cy.get("[data-testid=protectiveWordModalErrorText]").contains(
            "Invalid protective word. Try again."
        );
        cy.get("[data-testid=protectiveWordInput]")
            .clear()
            .should("be.enabled")
            .type("KEYWORD");
        cy.get("[data-testid=protectiveWordContinueBtn]")
            .should("be.enabled")
            .click()
            .wait(250);
        cy.get("[data-testid=protectiveWordModal]").should("not.exist");
        cy.contains("Gathering your health records ...");
    });

    it("Dismiss Protective Word", () => {
        cy.enableModules("Medication");
        cy.login(
            Cypress.env("keycloak.protected.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
        cy.get("[data-testid=protectiveWordModal] header:first")
            .find("button")
            .should("have.text", "×")
            .click();
        cy.get("[data-testid=protectiveWordModal]").should("not.exist");
    });
});
