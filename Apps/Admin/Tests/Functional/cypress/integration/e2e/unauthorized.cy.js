import { skipOn } from "@cypress/skip-test";

const username = Cypress.env("keycloak_unauthorized_username");
const password = Cypress.env("keycloak_password");

describe("Unauthorized", () => {
    it("Verify unauthorized user reaches page.", () => {
        skipOn("localhost");
        cy.logout();
        cy.visit("/");
        cy.get("[data-testid=sign-in-btn]")
            .should("be.visible")
            .should("not.be.disabled")
            .click();

        cy.origin(
            "https://dev.loginproxy.gov.bc.ca",
            { args: { username, password } },
            ({ username, password }) => {
                cy.get("#kc-page-title", { timeout: 10000 }).should(
                    "be.visible"
                );
                cy.get("#username").should("be.visible").type(username);
                cy.get("#password")
                    .should("be.visible")
                    .type(password, { log: false });
                cy.get("input[name=login]").should("be.visible").click();
                cy.url().should("include", "/unauthorized");
            }
        );
    });
});
