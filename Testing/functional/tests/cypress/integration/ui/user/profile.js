const { AuthMethod } = require("../../../support/constants");
const fakeSMSNumber = "7781234567";
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

describe("User Profile", () => {
    beforeEach(() => {
        cy.intercept("GET", `**/UserProfile/${HDID}`, {
            fixture: "UserProfileService/userProfile.json",
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );
    });

    it("Validate Login History Sorted Descending", () => {
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
        cy.intercept("PUT", `**/UserProfile/${HDID}/email`, {
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
        cy.intercept("PUT", `**/UserProfile/${HDID}/sms`, {
            statusCode: 200,
            body: true,
        });
        cy.intercept("GET", `**/UserProfile/${HDID}/sms/validate/*`, {
            statusCode: 200,
            body: {
                resultStatus: 1,
                resourcePayload: true,
            },
        });

        cy.log("Verify SMS number");
        cy.get("[data-testid=smsStatusNotVerified]").should("be.visible");
        cy.intercept("GET", `**/UserProfile/${HDID}`, {
            fixture: "UserProfileService/userProfileSMSVerified.json",
        });
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
        cy.intercept("GET", `**/UserProfile/${HDID}`, {
            fixture: "UserProfileService/userProfile.json",
        });
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

describe("User Profile - Validate Address", () => {
    beforeEach(() => {
        cy.intercept("GET", `**/UserProfile/${HDID}`, {
            fixture: "UserProfileService/userProfile.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
    });

    it("Verify user has combined address", () => {
        cy.intercept(
            "GET",
            `**/Patient/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A*`,
            {
                fixture: "PatientService/patientCombinedAddress.json",
            }
        );
        cy.visit("/profile");
        cy.get("[data-testid=postal-address-label]")
            .should("be.visible")
            .contains("Address");
        cy.get("[data-testid=postal-address-div]")
            .should("be.visible")
            .contains("3815 HILLSPOINT STREET");
        cy.get("[data-testid=postal-address-div]")
            .should("be.visible")
            .contains("CHATHAM, BC, V0G8B8");

        cy.get("[data-testid=physical-address-section").should("not.exist");
    });

    it("Verify user has different addresses", () => {
        cy.intercept(
            "GET",
            `**/Patient/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A*`,
            {
                fixture: "PatientService/patientDifferentAddress.json",
            }
        );
        cy.visit("/profile");

        // Postal Address
        cy.get("[data-testid=postal-address-label]")
            .should("be.visible")
            .contains("Mailing Address");
        cy.get("[data-testid=postal-address-div]")
            .should("be.visible")
            .contains("8128 WILD CREEK DRIVE");
        cy.get("[data-testid=postal-address-div]")
            .should("be.visible")
            .contains("HOLBORN, BC, V3A3P1");

        // Physical Address
        cy.get("[data-testid=physical-address-label]").should("be.visible");
        cy.get("[data-testid=physical-address-div]")
            .should("be.visible")
            .contains("1234 LEE AVENUE");
        cy.get("[data-testid=physical-address-div]")
            .should("be.visible")
            .contains("VICTORIA, BC, V9C6P1");
    });

    it("Verify user has no address", () => {
        cy.intercept(
            "GET",
            `**/Patient/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A*`,
            {
                fixture: "PatientService/patientNoAddress.json",
            }
        );
        cy.visit("/profile");

        // Postal Address
        cy.get("[data-testid=postal-address-label]")
            .should("be.visible")
            .contains("Address");
        cy.get("[data-testid=no-postal-address-text]").should("be.visible");

        // Physical Address
        cy.get("[data-testid=physical-address-section").should("not.exist");
    });

    it("Verify user has only physical address", () => {
        cy.intercept(
            "GET",
            `**/Patient/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A*`,
            {
                fixture: "PatientService/patientOnlyPhysicalAddress.json",
            }
        );
        cy.visit("/profile");

        // Postal Address
        cy.get("[data-testid=postal-address-label]")
            .should("be.visible")
            .contains("Mailing Address");
        cy.get("[data-testid=no-postal-address-text]").should("be.visible");

        // Phsyical Address
        cy.get("[data-testid=physical-address-label]").should("be.visible");
        cy.get("[data-testid=physical-address-div]")
            .should("be.visible")
            .contains("4790 RAD CLOSE");
        cy.get("[data-testid=physical-address-div]")
            .should("be.visible")
            .contains("HYTHE , BC, V5K 2X0");
    });

    it("Verify user has only postal address", () => {
        cy.intercept(
            "GET",
            `**/Patient/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A*`,
            {
                fixture: "PatientService/patientOnlyPostalAddress.json",
            }
        );
        cy.visit("/profile");

        // Postal Address
        cy.get("[data-testid=postal-address-label]")
            .should("be.visible")
            .contains("Mailing Address");
        cy.get("[data-testid=postal-address-div]")
            .should("be.visible")
            .contains("7826 UPPER AUSTIN LODGE L");
        cy.get("[data-testid=postal-address-div]")
            .should("be.visible")
            .contains("AXBRIDGE, BC, T8C 8G7");

        // Physical Address
        cy.get("[data-testid=physical-address-label]").should("be.visible");
        cy.get("[data-testid=no-physical-address-text]").should("be.visible");
    });
});
