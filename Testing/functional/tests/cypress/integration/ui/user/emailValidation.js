import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("User Email Verification", () => {
    beforeEach(() => {
        const baseUrl =
            "**/UserProfile/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        cy.intercept("GET", `${baseUrl}/email/validate/valid?api-version=2.0`, {
            body: true,
        });
        cy.intercept(
            "GET",
            `${baseUrl}/email/validate/invalid?api-version=2.0`,
            {
                statusCode: 409,
            }
        );
        cy.intercept(
            "GET",
            `${baseUrl}/email/validate/expired?api-version=2.0`,
            {
                body: false,
            }
        );
        cy.configureSettings({});
    });

    it("Check verified email invite", () => {
        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/validateEmail/valid"
        );
        cy.get("[data-testid=verifyingInvite]").should("not.exist");
        cy.get("[data-testid=verifiedInvite]").should("be.visible");
        cy.get("[data-testid=continueButton]").click();
        cy.url().should("include", "/home");
    });

    it("Check already verified email invite", () => {
        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/validateEmail/invalid"
        );
        cy.get("[data-testid=verifyingInvite]").should("not.exist");
        cy.get("[data-testid=alreadyVerifiedInvite]").should("be.visible");
        cy.get("[data-testid=continueButton]").click();
        cy.url().should("include", "/home");
    });

    it("Check expired email invite", () => {
        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/validateEmail/expired"
        );
        cy.get("[data-testid=verifyingInvite]").should("not.exist");
        cy.get("[data-testid=expiredInvite]").should("be.visible");
        cy.get("[data-testid=continueButton]").click();
        cy.url().should("include", "/profile");
    });
});
