import { AuthMethod } from "../../../support/constants";
const registrationPath = "/registration";
const homePath = "/home";
const invalidEmail = "gov.bc.ca";
const invalidPhone = "250";

describe("Registration Page", () => {
    it("Minimum age error", () => {
        cy.configureSettings({});
        cy.login(
            Cypress.env("keycloak.hlthgw401.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );
        cy.get("[data-testid=minimumAgeErrorText]").should("be.visible");
        cy.location("pathname").should("eq", registrationPath);
    });

    // AB#16927 Disable notifications while aligning Classic with Salesforce version
    it.skip("Registering leads to home page and opens app tour", () => {
        cy.configureSettings({});
        cy.login(
            Cypress.env("keycloak.unregistered.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );

        cy.contains("#subject", "Registration").should("be.visible");
        cy.location("pathname").should("eq", registrationPath);

        cy.get("[data-testid=sidebar]").should("not.exist");
        cy.get("[data-testid=footer]").should("not.exist");

        cy.get("[data-testid=emailCheckbox] input")
            .should("be.enabled")
            .check();
        cy.get("[data-testid=emailInput]")
            .should("be.visible", "be.enabled")
            .type(invalidEmail);
        cy.get("[data-testid=emailInput]").within(() => {
            cy.get("div").contains("Invalid email").should("be.visible");
        });

        cy.get("[data-testid=emailConfirmationInput]")
            .should("be.visible", "be.enabled")
            .type(invalidEmail);
        cy.get("[data-testid=emailConfirmationInput]").within(() => {
            cy.get("div").contains("Invalid email").should("be.visible");
        });

        cy.get("[data-testid=sms-checkbox] input").should("be.enabled").check();
        cy.get("[data-testid=smsNumberInput]")
            .should("be.visible", "be.enabled")
            .type(invalidPhone);
        cy.get("[data-testid=smsNumberInput]").within(() => {
            cy.get("div").contains("Invalid phone number").should("be.visible");
        });

        cy.get("[data-testid=emailInput]")
            .should("be.visible", "be.enabled")
            .clear()
            .type(Cypress.env("emailAddress"));
        cy.get("[data-testid=emailInput]").within(() => {
            cy.get("div").contains("Invalid email").should("not.exist");
        });

        cy.get("[data-testid=emailConfirmationInput]")
            .should("be.visible", "be.enabled")
            .clear()
            .type(Cypress.env("emailAddress"));
        cy.get("[data-testid=emailConfirmationInput]").within(() => {
            cy.get("div").contains("Invalid email").should("not.exist");
        });

        cy.get("[data-testid=smsNumberInput]")
            .should("be.visible", "be.enabled")
            .clear()
            .type(Cypress.env("phoneNumber"));
        cy.get("[data-testid=smsNumberInput]").within(() => {
            cy.get("div").contains("Invalid phone number").should("not.exist");
        });

        cy.get("[data-testid=smsNumberInput] input")
            .should("be.visible")
            .clear();
        cy.get('[data-testid="sms-checkbox"] input')
            .should("be.enabled")
            .uncheck({ force: true });
        cy.get("[data-testid=smsNumberInput] input").should("have.value", "");

        cy.get("[data-testid=acceptCheckbox] input")
            .should("be.enabled")
            .check();
        cy.get("[data-testid=registerButton]")
            .should("be.visible", "be.enabled")
            .click();
        cy.location("pathname").should("eq", homePath);
        cy.get("[data-testid=app-tour-modal").should("be.visible");
        cy.get("[data-testid=app-tour-skip]").click();
        cy.get("[data-testid=incomplete-profile-banner]").should("be.visible");
    });

    it("Validate Closed Profile Registration", () => {
        cy.configureSettings({});
        cy.login(
            Cypress.env("keycloak.accountclosure.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );
        cy.get("[data-testid=patient-retrieval-error]")
            .should("exist")
            .contains("Error retrieving user information");
        cy.url().should("include", "/patientRetrievalError");
    });
});
