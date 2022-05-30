const { AuthMethod } = require("../../../support/constants");
const HDID = "K6HL4VX67CZ2PGSZ2ZOIR4C3PGMFFBW5CIOXM74D6EQ7RYYL7P4A";

describe("Need to accept terms of service", () => {
    beforeEach(() => {
        cy.intercept("GET", `**/UserProfile/${HDID}`, (req) => {
            req.reply({
                fixture: "UserProfileService/userProfileAcceptTos.json",
            });
        });
        cy.login(
            Cypress.env("keycloak.accept.tos.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
    });

    it("Validate accept terms of service", () => {
        cy.url().should("include", "/acceptTermsOfService");

        cy.get("[data-testid=tos-page-title]").should("be.visible");
        cy.get("[data-testid=tos-text-area-component]").should("be.visible");

        cy.get("[data-testid=accept-tos-checkbox]").should("be.enabled");
        cy.get("[data-testid=continue-btn]").should("be.disabled");

        cy.get("[data-testid=accept-tos-checkbox]")
            .should("be.enabled")
            .check({ force: true });
        cy.get("[data-testid=continue-btn]").should("be.enabled");
    });
});

describe("Does not need to accept terms of service", () => {
    beforeEach(() => {
        cy.intercept("GET", `**/UserProfile/${HDID}`, (req) => {
            req.reply({
                fixture: "UserProfileService/userProfile.json",
            });
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
    });

    it("Validate accept terms of service", () => {
        cy.url().should("include", "/home");
    });
});
