const { AuthMethod } = require("../../support/constants");
const registrationPage = "/registration"
const profilePage = "/profile"

describe("Registration Page", () => {
    beforeEach(() => {
    })
    
    it("Minimum Age error", () => {
        cy.enableModule("Medication").then(() => {
            cy.login(Cypress.env('keycloak.hlthgw401.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloakUI, registrationPage);
        });
        cy.url().should("include", registrationPage);
        cy.get('[data-testid="minimumAgeErrorText"]').should('be.visible');
    });

    it("Client Registration error", () => {
        cy.enableModule("Medication").then(() => {
            cy.login(Cypress.env('keycloak.healthgateway12.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloakUI, registrationPage);   
        });
        cy.url().should("include", registrationPage);
        cy.get('[data-testid="clientRegistryErrorText"]').should('be.visible');
    });

    it("No sidebar or footer", () => {
        cy.enableModule("Medication").then(() => {
            cy.login(Cypress.env('keycloak.unregistered.username'), 
                     Cypress.env('keycloak.password'), 
                     AuthMethod.KeyCloakUI);
        });
        cy.url().should("include", registrationPage);
        cy.contains("footer").should("not.exist");
        cy.get('[data-testid="sidebar"]').should('not.be.visible');
    });

    it("Registration goes to Verify Phone", () => {
        cy.enableModule("Medication").then(() => {
            cy.login(Cypress.env('keycloak.unregistered.username'), 
                     Cypress.env('keycloak.password'), 
                     AuthMethod.KeyCloakUI);
        });
        cy.url().should("include", registrationPage);
        cy.get('[data-testid="emailCheckbox"]')
            .should('be.visible')
            .click();
        cy.get('[data-testid="smsNumberInput"]')
            .should('be.visible', 'be.enabled')
            .type(Cypress.env('phoneNumber'));
        cy.get('[data-testid="acceptCheckbox"]')
            .should('be.visible')
            .click();
        cy.get('[data-testid="registerButton"]')
            .should('be.visible', 'be.enabled')
            .click();
        cy.url().should("include", profilePage);
        cy.get('[data-testid="verifySMSModal"]')
            .should('be.visible');
    });
});
