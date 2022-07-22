import { skipOn } from "@cypress/skip-test";

const username = Cypress.env("idir_username");
const password = Cypress.env("idir_password");

describe("Authentication", () => {
    it("IDIR login and logout.", () => {
        skipOn("localhost");

        cy.log("IDIR login/logout test started.");
        cy.visit("/");

        cy.log("Clicking sign in button.");
        cy.get("[data-testid=sign-in-btn]")
            .should("be.visible")
            .should("not.be.disabled")
            .click();

        cy.log(`Authenticating as IDIR user ${username}`);
        cy.get("#zocial-idir")
            .should("be.visible")
            .should("not.be.disabled")
            .click();
        cy.get("#user").should("be.visible").type(username);
        cy.get("#password").should("be.visible").type(password, { log: false });
        cy.get("input[name=btnSubmit]").should("be.visible").click();
        cy.url().should("include", "/dashboard");
        cy.log("Successfully logged in to admin dashboard.");

        cy.log("Logging out.");
        cy.get("[data-testid=user-account-icon]").click();
        cy.get("[data-testid=logout-text-link]").within(() => {
            cy.get(".mud-nav-link").click();
        });
        cy.url().should("include", "/authentication/logged-out");
        cy.log("Successfully logged out.");

        cy.log("IDIR login/logout test finished.");
    });
});
