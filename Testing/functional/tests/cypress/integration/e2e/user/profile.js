const { AuthMethod } = require("../../../support/constants");
const fakeSMSNumber = "7781234567";

describe("User Profile", () => {
    const emailAddress =
        "healthgateway@mailinator" +
        Math.random().toString().substr(2, 5) +
        ".com";
    before(() => {
        cy.enableModules("Medication");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );
    });

    it("PHN Visible and Correct", () => {
        cy.get("[data-testid=PHN]").should("be.visible").contains("9735353315");
    });

    it("Edit email address", () => {
        cy.intercept(
            "PUT",
            "/v1/api/UserProfile/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/email",
            {
                statusCode: 200,
                body: true,
            }
        );
        cy.get("[data-testid=editEmailBtn]").click();
        cy.get("[data-testid=emailInput]").type(Cypress.env("emailAddress"));
        cy.get("[data-testid=editEmailSaveBtn]").click();
        cy.get("[data-testid=emailStatusNotVerified]").should("be.visible");
        cy.get("[data-testid=resendEmailBtn]").should("be.visible");
    });

    it("Invalid email address", () => {
        cy.intercept(
            "PUT",
            "/v1/api/UserProfile/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/email",
            {
                statusCode: 200,
                body: true,
            }
        );
        cy.get("[data-testid=editEmailBtn]").click();
        cy.get("[data-testid=editEmailSaveBtn]").should("be.disabled");
        cy.get("[data-testid=emailInvalidNewEqualsOld]").should("be.visible");
        cy.get("[data-testid=editEmailCancelBtn]").click();
        cy.get("[data-testid=emailInput]")
            .should("be.disabled")
            .should("have.value", Cypress.env("emailAddress"));
    });

    it("Clear/OptOut email address", () => {
        cy.intercept(
            "PUT",
            "/v1/api/UserProfile/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/email",
            {
                statusCode: 200,
                body: true,
            }
        );
        cy.get("[data-testid=editEmailBtn]").click();
        cy.get("[data-testid=emailInput]").clear();
        cy.get("[data-testid=emailOptOutMessage]").should("be.visible");
        cy.get("[data-testid=editEmailSaveBtn]").click();
        cy.get("[data-testid=emailStatusOptedOut]").should("be.visible");
    });

    it("Verify SMS number", () => {
        cy.get("[data-testid=smsStatusNotVerified]").should("be.visible");
        cy.get("[data-testid=verifySMSBtn]")
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get('[data-testid="countdownText"]').contains(/\d{1,2}s$/); // has 1 or 2 digits before the last 's' character

        cy.get("[data-testid=verifySMSModal] button.close").click();
        cy.get("[data-testid=verifySMSBtn]")
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get("[data-testid=verifySMSModalCodeInput]")
            .should("be.visible")
            .should("have.focus")
            .type("123456"); //seed data

        cy.get("[data-testid=smsStatusVerified]").should("be.visible");
    });

    it("Edit sms number", () => {
        cy.intercept(
            "PUT",
            "/v1/api/UserProfile/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/sms",
            {
                statusCode: 200,
                body: true,
            }
        );
        cy.get("[data-testid=editSMSBtn]").click();
        cy.get("[data-testid=smsInvalidNewEqualsOld]").should("be.visible");
        cy.get("[data-testid=smsNumberInput]").clear().type(fakeSMSNumber);
        cy.get("[data-testid=saveSMSEditBtn]").click();
        cy.get('[data-testid="countdownText"]').contains(/\d{1,2}s$/); // has 1 or 2 digits before the last 's' character

        cy.get("[data-testid=verifySMSModal] button.close").click();
        cy.get("[data-testid=smsStatusNotVerified]").should("be.visible");
        cy.get("[data-testid=verifySMSBtn]")
            .should("be.visible")
            .should("be.enabled");
    });

    it("Invalid sms number", () => {
        cy.get("[data-testid=editSMSBtn]").click();
        cy.get("[data-testid=saveSMSEditBtn]").should("be.disabled");
        cy.get("[data-testid=smsInvalidNewEqualsOld]").should("be.visible");
        cy.get("[data-testid=cancelSMSEditBtn]").click();
        cy.get("[data-testid=smsNumberInput]")
            .should("be.disabled")
            .invoke("val")
            .then((value) =>
                expect(value.replace(/\D+/g, "")).to.eq(fakeSMSNumber)
            );
    });

    it("Clear/OptOut sms number", () => {
        cy.intercept(
            "PUT",
            "/v1/api/UserProfile/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/sms",
            {
                statusCode: 200,
                body: true,
            }
        );
        cy.get("[data-testid=editSMSBtn]").click();
        cy.get("[data-testid=smsNumberInput]").clear();
        cy.get("[data-testid=smsOptOutMessage]").should("be.visible");
        cy.get("[data-testid=saveSMSEditBtn]").click();
        cy.get("[data-testid=smsStatusOptedOut]").should("be.visible");
    });
});
