const { AuthMethod } = require("../../../support/constants");
const registrationPath = "/registration";
const homePath = "/home";

describe("Registration Page", () => {
    it("Minimum age error", () => {
        cy.configureSettings({});
        cy.login(
            Cypress.env("keycloak.hlthgw401.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );
        cy.location("pathname").should("eq", registrationPath);
        cy.get("[data-testid=minimumAgeErrorText]").should("be.visible");
    });

    it("No sidebar or footer", () => {
        cy.configureSettings({});
        cy.login(
            Cypress.env("keycloak.unregistered.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );
        cy.location("pathname").should("eq", registrationPath);
        cy.get("[data-testid=sidebar]").should("not.exist");
        cy.get("[data-testid=footer]").should("not.exist");
    });

    it("Registering leads to home page and opens app tour", () => {
        cy.login(
            Cypress.env("keycloak.unregistered.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );
        cy.location("pathname").should("eq", registrationPath);
        cy.get("[data-testid=emailCheckbox] input")
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
        cy.get("[data-testid=acceptCheckbox] input")
            .should("be.enabled")
            .check({ force: true })
            .wait(500);
        cy.get("[data-testid=registerButton]")
            .should("be.visible", "be.enabled")
            .click();
        cy.location("pathname").should("eq", homePath);
        cy.get("[data-testid=app-tour-modal").should("be.visible");
        cy.get("[data-testid=app-tour-skip]").click();
        cy.get("[data-testid=incomplete-profile-banner]").should("be.visible");
    });

    it("Validate Closed Profile Registration", () => {
        cy.configureSettings({});
        cy.login(
            Cypress.env("keycloak.accountclosure.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );
        cy.url().should("include", "/patientRetrievalError");
    });
});
