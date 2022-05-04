const { AuthMethod } = require("../../../support/constants");
const registrationPath = "/registration";
const profilePath = "/profile";

describe("Registration Page", () => {
    it("Minimum Age error", () => {
        cy.enableModules("Medication");
        cy.login(
            Cypress.env("keycloak.hlthgw401.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.location("pathname").should("eq", registrationPath);
        cy.get("[data-testid=minimumAgeErrorText]").should("be.visible");
    });

    it("Client Registration error", () => {
        cy.enableModules("Medication");
        cy.login(
            Cypress.env("keycloak.healthgateway12.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.location("pathname").should("eq", registrationPath);
        cy.get("[data-testid=clientRegistryErrorText]").should("be.visible");
    });

    it("No sidebar or footer", () => {
        cy.enableModules("Medication");
        cy.login(
            Cypress.env("keycloak.unregistered.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.location("pathname").should("eq", registrationPath);
        cy.get("[data-testid=sidebar]").should("not.exist");
        cy.get("[data-testid=footer]").should("not.exist");
    });

    it("Registration goes to Verify Phone and Email", () => {
        cy.enableModules("Medication");
        cy.login(
            Cypress.env("keycloak.unregistered.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.location("pathname").should("eq", registrationPath);
        cy.get("[data-testid=emailCheckbox]")
            .should("be.enabled")
            .check({ force: true });
        cy.get("[data-testid=emailInput]")
            .should("be.visible", "be.enabled")
            .type(Cypress.env("emailAddress"));
        cy.get("[data-testid=emailConfirmationInput]")
            .should("be.visible", "be.enabled")
            .type(Cypress.env("emailAddress"));
        cy.get("[data-testid=smsNumberInput]")
            .should("be.visible", "be.enabled")
            .type(Cypress.env("phoneNumber"));
        cy.get("[data-testid=acceptCheckbox]")
            .should("be.enabled")
            .check({ force: true });
        cy.get("[data-testid=registerButton]")
            .should("be.visible", "be.enabled")
            .click();
        cy.location("pathname").should("eq", profilePath);
        cy.get("[data-testid=verifySMSModalText]").should("be.visible");
        cy.get("[data-testid=verifyEmailTxt]").should("be.visible");
    });

    it("Validate Closed Profile Registration", () => {
        cy.login(
            Cypress.env("keycloak.accountclosure.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.url().should("include", "/registration");
    });
});
