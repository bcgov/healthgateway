import { AuthMethod } from "../../../support/constants";
const defaultTimeout = 60000;

describe("Need to accept terms of service", () => {
    it("Validate accept terms of service", () => {
        cy.configureSettings({});
        cy.intercept("GET", "**/UserProfile/termsofservice?api-version=2.0").as(
            "getTermsOfService"
        );
        cy.login(
            Cypress.env("keycloak.accept.tos.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home",
            { waitForInitialDataLoad: false }
        );
        cy.wait("@getTermsOfService", { timeout: defaultTimeout });

        cy.get("[data-testid=tos-page-title]")
            .should("exist")
            .contains("Update to our Terms of Service");

        cy.url().should("include", "/acceptTermsOfService");

        cy.get("[data-testid=tos-text-area-component]").should("be.visible");

        cy.get("[data-testid=accept-tos-checkbox] input").should("be.enabled");
        cy.get("[data-testid=continue-btn]").should("be.disabled");

        cy.get("[data-testid=accept-tos-checkbox] input")
            .should("be.enabled")
            .check({ force: true });

        cy.intercept(
            "PUT",
            "**/UserProfile/*/acceptedterms?api-version=2.0"
        ).as("updateAcceptedTerms");
        cy.intercept("GET", "**/UserProfile/*?api-version=2.0").as(
            "getUpdatedUserProfile"
        );

        cy.get("[data-testid=continue-btn]").should("be.enabled").click();
        cy.wait("@updateAcceptedTerms", { timeout: defaultTimeout })
            .its("response.statusCode")
            .should("eq", 200);
        cy.wait("@getUpdatedUserProfile", { timeout: defaultTimeout })
            .its("response.statusCode")
            .should("eq", 200);
        cy.location("pathname", { timeout: defaultTimeout }).should(
            "eq",
            "/home"
        );
    });
});
