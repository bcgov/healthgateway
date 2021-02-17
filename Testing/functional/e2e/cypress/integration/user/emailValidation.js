const { AuthMethod } = require("../../support/constants")

describe('User Email Verification', () => {
    beforeEach(() => {
        const baseUrl = "**/v1/api/UserProfile/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        cy.intercept("GET", `${baseUrl}/email/validate/valid`, { fixture: "WebClientService/EmailValidation/valid.json" });
        cy.intercept("GET", `${baseUrl}/email/validate/invalid`, { fixture: "WebClientService/EmailValidation/invalid.json" });
        cy.intercept("GET", `${baseUrl}/email/validate/expired`, { fixture: "WebClientService/EmailValidation/expired.json" });
    })

    it('Check verified email invite', () => {
        cy.login(Cypress.env('keycloak.username'),
            Cypress.env('keycloak.password'),
            AuthMethod.KeyCloak,
            "/validateEmail/valid");
        cy.get("[data-testid=verifingInvite]").should('not.exist');
        cy.get('[data-testid=verifiedInvite]').should('be.visible');
    })

    it('Check invalid email invite', () => {
        cy.login(Cypress.env('keycloak.username'),
            Cypress.env('keycloak.password'),
            AuthMethod.KeyCloak,
            "/validateEmail/invalid");
        cy.get("[data-testid=verifingInvite]").should('not.exist');
        cy.get('[data-testid=invalidInvite]').should('be.visible');
    })

    it('Check expired email invite', () => {
        cy.login(Cypress.env('keycloak.username'),
            Cypress.env('keycloak.password'),
            AuthMethod.KeyCloak,
            "/validateEmail/expired");
        cy.get("[data-testid=verifingInvite]").should('not.exist');
        cy.get('[data-testid=expiredInvite]').should('be.visible');
        cy.get('[data-testid=profileLink]').click();
        cy.url().should("include", "/profile");        
    })

})