// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
const { AuthMethod } = require("./constants");
Cypress.Commands.add(
    "login",
    (username, password, authMethod = AuthMethod.BCSC, path = "/login") => {
        cy.visit(path);
        if (authMethod == AuthMethod.BCSC) {
            cy.log(`Authenticating as BC Services Card user ${username}`);
            cy.get("#BCSCBtn")
                .should("be.visible")
                .should("have.text", "BC Services Card")
                .click();
            cy.url().should(
                "contains",
                "https://idtest.gov.bc.ca/login/entry#start"
            );
            cy.get("#tile_btn_virtual_device_div_id > h2").click();
            cy.get("#csn").click();
            cy.get("#csn").type(username);
            cy.get("#continue").click();
            cy.url().should(
                "contains",
                "https://idtest.gov.bc.ca/login/identify"
            );
            cy.get("#passcode").click();
            cy.get("#passcode").type(password);
            cy.get("#btnSubmit").click();
            cy.get("#btnSubmit").click();
        } else {
            cy.log(`Authenticating as KeyCloak user ${username}`);
            cy.get("#KeyCloakBtn")
                .should("be.visible")
                .should("have.text", "KeyCloak")
                .click();
            cy.get("#username").type(username);
            cy.get("#password").type(password);
            cy.get("#kc-login").click();
        }

        cy.get('#subject')
            .should('have.text', 'Health Care Timeline');
    }
);

Cypress.Commands.add("checkTimelineHasLoaded", () => {
    cy.url().should(
        "contains",
        "/timeline"
    );
    cy.get("[data-testid=timelineLoading]").should("not.exist");
});

Cypress.Commands.add("closeCovidModal", () => {
    cy.get("[data-testid=covidModal] .close").click();
});
