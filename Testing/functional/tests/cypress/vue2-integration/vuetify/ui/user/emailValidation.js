const { AuthMethod } = require("../../../../support/constants");

describe("User Email Verification", () => {
    beforeEach(() => {
        const baseUrl =
            "**/UserProfile/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        cy.intercept("GET", `${baseUrl}/email/validate/valid`, {
            fixture: "WebClientService/EmailValidation/valid.json",
        });
        cy.intercept("GET", `${baseUrl}/email/validate/invalid`, {
            fixture: "WebClientService/EmailValidation/invalid.json",
        });
        cy.intercept("GET", `${baseUrl}/email/validate/expired`, {
            fixture: "WebClientService/EmailValidation/expired.json",
        });
        cy.intercept("GET", "**/UserProfile/*").as("getUserProfile");
        cy.configureSettings({});
    });

    it("Check verified email invite", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/validateEmail/valid"
        );
        cy.get("[data-testid=verifyingInvite]").should("not.exist");
        cy.get("[data-testid=verifiedInvite]").should("be.visible");
        cy.get("[data-testid=continueButton]").click();
        cy.wait("@getUserProfile").then((interception) => {
            assert.equal(interception.response.statusCode, 200);
        });
        cy.url().should("include", "/home");
    });

    it("Check already verified email invite", () => {
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
