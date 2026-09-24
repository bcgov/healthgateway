import { skipOn } from "@cypress/skip-test";
const { AuthMethod } = require("../../../support/constants");

describe("Authentication", () => {
    beforeEach(() => {
        cy.configureSettings({});
    });

    afterEach(() => {
        Cypress.session.clearAllSavedSessions();
    });

    it("BCSC UI Login", () => {
        cy.login(
            Cypress.env("bcsc.username"),
            Cypress.env("bcsc.password"),
            AuthMethod.BCSC,
            "/home"
        );

        cy.location("pathname", { timeout: 30000 }).should((pathname) => {
            expect(["/home", "/acceptTermsOfService"]).to.include(pathname);
        });

        cy.location("pathname").then((pathname) => {
            if (pathname === "/acceptTermsOfService") {
                cy.get("[data-testid=accept-tos-checkbox] input")
                    .should("be.enabled")
                    .check({ force: true });

                cy.contains("button", "Continue").click({ force: true });

                cy.location("pathname", { timeout: 30000 }).should(
                    "eq",
                    "/home"
                );
            }
        });

        cy.location("pathname").should("eq", "/home");

        cy.get("[data-testid=headerDropdownBtn]").click();
        cy.get("[data-testid=logoutBtn]")
            .should("be.visible")
            .should("not.be.disabled");
    });

    it("KeyCloak UI Login", () => {
        skipOn("localhost");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloakUI,
            "/home"
        );
        cy.url().should("include", "/home");

        cy.get("[data-testid=headerDropdownBtn]").click();
        cy.get("[data-testid=logoutBtn]")
            .should("be.visible")
            .should("not.be.disabled");
    });

    it("KeyCloak Deceased Login", () => {
        cy.login(
            Cypress.env("keycloak.deceased.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.url().should("include", "/patientRetrievalError");
    });

    it("Keycloak Login and Logout", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.url().should("include", "/home");
        cy.get("[data-testid=headerDropdownBtn]").click();
        cy.get("[data-testid=logoutBtn]").click();
        cy.get("[data-testid=ratingModalSkipBtn]").click();
        cy.get("[data-testid=logout-complete-msg]").should("be.visible");
        cy.get("[data-testid=loginBtn]")
            .should("be.visible")
            .should("not.be.disabled");
    });

    it("IDIR Blocked", () => {
        skipOn("localhost");
        cy.logout();
        cy.visit("/login");
        cy.log(`Authenticating as IDIR user ${Cypress.env("idir.username")}`);
        cy.url().should("include", "/login");
        cy.get("[data-testid=IDIRBtn]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.origin(
            "https://logontest7.gov.bc.ca",
            {
                args: {
                    username: Cypress.env("idir.username"),
                    password: Cypress.env("idir.password"),
                },
            },
            ({ username, password }) => {
                cy.get("#idirLogo", { timeout: 10000 }).should("be.visible");
                cy.get("#user").should("be.visible").type(username);
                cy.get("#password")
                    .should("be.visible")
                    .type(password, { log: false });
                cy.get('input[name="btnSubmit"]').should("be.visible").click();
            }
        );

        cy.url({ timeout: 10000 }).should("include", "idirLoggedIn");
        cy.contains("h1", "403");
        cy.contains("h2", "IDIR Login");
    });
});
