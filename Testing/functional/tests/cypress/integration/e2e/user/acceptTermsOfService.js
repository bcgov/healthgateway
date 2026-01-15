import { AuthMethod } from "../../../support/constants";
const defaultTimeout = 60001;

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
            "/home"
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
        cy.get("[data-testid=continue-btn]").should("be.enabled").click();
        cy.url().should("include", "/home");
    });
});

describe("Does not need to accept terms of service", () => {
    beforeEach(() => {
        cy.configureSettings({});
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
    });

    it("Validate accept terms of service is not required", () => {
        cy.url().should("include", "/home");
    });
});
