// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
require("cy-verify-downloads").addCustomCommand();

Cypress.Commands.add("logout", () => {
    cy.log(`Logging out of Admin.`);
    cy.get("[data-testid=user-account-icon]").click();
    cy.get("[data-testid=logout-text-link]").within(() => {
        cy.get(".mud-nav-link").click();
    });
    cy.url().should("include", "/authentication/logged-out");
    cy.log(`Successfully logged out of Admin.`);
});

Cypress.Commands.add("login", (username, password) => {
    cy.log(`Logging into Admin.`);
    cy.visit("/");
    cy.log("Clicking signin button.");
    cy.get("[data-testid=sign-in-btn]")
        .should("be.visible")
        .should("not.be.disabled")
        .click();
    cy.log(`Authenticating as IDIR user ${Cypress.env("idir_username")}`);
    cy.get("#zocial-idir")
        .should("be.visible")
        .should("not.be.disabled")
        .click();
    cy.log("Username: " + username);
    cy.log("Password: " + password);
    cy.get("#user").should("be.visible").type(username);
    cy.get("#password").should("be.visible").type(password);
    cy.get('input[name="btnSubmit"]').should("be.visible").click();
    cy.url().should("include", "/dashboard");
    cy.log(`Successfully logged into Admin dashboard.`);
});

Cypress.Commands.add("readConfig", () => {
    cy.log(`Reading Environment Configuration`);
    return cy
        .request(`${Cypress.config("baseUrl")}/v1/api/Configuration`)
        .should((response) => {
            expect(response.status).to.eq(200);
        })
        .its("body");
});

Cypress.Commands.overwrite(
    "select",
    (originalFn, subject, valueOrTextOrIndex, options) => {
        cy.wrap(subject).should("be.visible", "be.enabled");
        cy.wrap(originalFn(subject, valueOrTextOrIndex, options));
    }
);
