const { AuthMethod } = require("../../../support/constants");
const fakeSMSNumber = "7781234567";
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

describe("User Profile", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );
    });

    it("Validate Login History Sorted Descending", () => {
        cy.intercept("GET", `**/v1/api/UserProfile/${HDID}`, (req) => {
            req.reply({
                fixture: "UserProfileService/userProfile.json",
            });
        });

        cy.get("[data-testid=lastLoginDateItem]")
            .first()
            .then(($dateItem) => {
                // 1st login date in the list
                const firstDate = new Date($dateItem.text().trim());
                cy.get("[data-testid=lastLoginDateItem]")
                    .eq(1)
                    .then(($dateItem) => {
                        // 2nd login date in the list
                        const secondDate = new Date($dateItem.text().trim());
                        expect(firstDate).to.be.gte(secondDate);
                        // 3rd login date in the list
                        cy.get("[data-testid=lastLoginDateItem]")
                            .eq(2)
                            .then(($dateItem) => {
                                const lastDate = new Date(
                                    $dateItem.text().trim()
                                );
                                expect(firstDate).to.be.gte(lastDate);
                                expect(secondDate).to.be.gte(lastDate);
                            });
                    });
            });
    });

    it("Email address", () => {
        cy.intercept("PUT", `**/v1/api/UserProfile/${HDID}/email`, {
            statusCode: 200,
            body: true,
        });

        cy.log("Edit email address");
        cy.get("[data-testid=editEmailBtn]").click();
        cy.get("[data-testid=emailInput]").type(Cypress.env("emailAddress"));
        cy.get("[data-testid=editEmailSaveBtn]").click();
        cy.get("[data-testid=emailStatusNotVerified]").should("be.visible");
        cy.get("[data-testid=resendEmailBtn]").should("be.visible");

        cy.log("Invalid email address");
        cy.get("[data-testid=editEmailBtn]").click();
        cy.get("[data-testid=editEmailSaveBtn]").should("be.disabled");
        cy.get("[data-testid=emailInvalidNewEqualsOld]").should("be.visible");
        cy.get("[data-testid=editEmailCancelBtn]").click();
        cy.get("[data-testid=emailInput]")
            .should("be.disabled")
            .should("have.value", Cypress.env("emailAddress"));

        cy.log("Clear/OptOut email address");
        cy.get("[data-testid=editEmailBtn]").click();
        cy.get("[data-testid=emailInput]").clear();
        cy.get("[data-testid=emailOptOutMessage]").should("be.visible");
        cy.get("[data-testid=editEmailSaveBtn]").click();
        cy.get("[data-testid=emailStatusOptedOut]").should("be.visible");
    });

    it("Verify SMS number", () => {
        cy.intercept("PUT", `**/v1/api/UserProfile/${HDID}/sms`, {
            statusCode: 200,
            body: true,
        });
        cy.intercept("GET", `**/v1/api/UserProfile/${HDID}/sms/validate/*`, {
            statusCode: 200,
            body: {
                resultStatus: 1,
                resourcePayload: true,
            },
        });

        cy.log("Verify SMS number");
        cy.intercept("GET", `**/v1/api/UserProfile/${HDID}`, (req) => {
            req.reply({
                fixture: "UserProfileService/userProfile.json",
            });
        });
        cy.get("[data-testid=smsStatusNotVerified]").should("be.visible");
        cy.get("[data-testid=verifySMSBtn]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get("[data-testid=verifySMSModalCodeInput]")
            .should("be.visible")
            .should("have.focus")
            .type("123456");

        cy.log("SMS number status Verified");
        cy.get("[data-testid=smsStatusNotVerified]").should("not.exist");
        cy.get("[data-testid=verifySMSBtn]").should("not.exist");
        cy.get("[data-testid=smsStatusVerified]").should("be.visible");

        cy.log("Edit SMS number");
        cy.get("[data-testid=editSMSBtn]").click();
        cy.get("[data-testid=smsInvalidNewEqualsOld]").should("be.visible");
        cy.get("[data-testid=smsNumberInput]").clear().type(fakeSMSNumber);
        cy.get("[data-testid=saveSMSEditBtn]").click();
        cy.get("[data-testid=verifySMSModal] button.close").click();
        cy.get("[data-testid=smsStatusNotVerified]").should("be.visible");
        cy.get("[data-testid=verifySMSBtn]")
            .should("be.visible")
            .should("be.enabled");

        cy.log("Clear/OptOut SMS number");
        cy.get("[data-testid=editSMSBtn]").click();
        cy.get("[data-testid=smsNumberInput]").clear();
        cy.get("[data-testid=smsOptOutMessage]").should("be.visible");
        cy.get("[data-testid=saveSMSEditBtn]").click();
        cy.get("[data-testid=smsStatusOptedOut]").should("be.visible");
    });
});
