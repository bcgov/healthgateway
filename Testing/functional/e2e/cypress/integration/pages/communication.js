const { AuthMethod } = require("../../support/constants");

describe("Communication", () => {
    beforeEach(() => {
        cy.enableModules([""]);
    });

    it("Landing Banner", () => {
        cy.visit("/");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("Test Banner");
        
    });

    it("InApp Banner Unauthenticated", () => {
        cy.visit("/faq");
        cy.get("[data-testid=communicationBanner]")
            .should("not.exist")

        cy.visit("/contact-us");
        cy.get("[data-testid=communicationBanner]")
            .should("not.exist")

        cy.visit("/release-notes");
        cy.get("[data-testid=communicationBanner]")
            .should("not.exist")

        cy.visit("/termsOfService");
        cy.get("[data-testid=communicationBanner]")
            .should("not.exist")

        cy.visit("/404");
        cy.get("[data-testid=communicationBanner]")
            .should("not.exist")
    });

    it("InApp Banner Authenticated", () => {    
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();


        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");

        cy.visit("/credentials");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");

        cy.visit("/dependents");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");
            
        cy.visit("/healthInsights");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");
    
        cy.visit("/reports");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");

        cy.visit("/profile");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");
    });

});
