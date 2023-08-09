const { AuthMethod } = require("../../../support/constants");
import {
    clickRegisterKitButton,
    getPcrTestTakenTime,
} from "../../../support/functions/pcrTestKit";

const pcrTestUrl = "/pcrtest";
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

// data test id for all input fields in the form
const testKitCodeInput = "[data-testid=test-kit-code-input]";
const testTakenMinutesAgo = "[data-testid=test-taken-minutes-ago]";
const cancelBtn = "[data-testid=btn-cancel]";
const registerKitBtn = "[data-testid=btn-register-kit]";
const pcrPrivacyStatement = "[data-testid=pcr-privacy-statement]";
const registrationSuccessBanner = "[data-testid=registration-success-banner]";
const logoutBtn = "[data-testid=logoutBtn]";
const headerLogOutBtn = "[data-testid=header-log-out-button]";
const errorBanner = "[data-testid=errorBanner]";
const processedBanner = "[data-testid=alreadyProcessedBanner]";

// data test id for input required validations
const feedbackTestKitCodeIsRequiredSelector =
    "[data-testid=feedback-testkitcode-is-required]";
const feedbackTestTakenIsRequiredSelector =
    "[data-testid=feedback-testtaken-is-required]";

// data test id for input invalid validations
const feedbackTestKitCodeValidSelector =
    "[data-testid=feedback-testkitcode-is-invalid]";

describe("Authenticated Pcr Test Registration", () => {
    beforeEach(() => {
        cy.configureSettings({
            covid19: {
                pcrTestEnabled: true,
            },
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.visit(pcrTestUrl);
        cy.intercept("POST", `**/Laboratory/${HDID}/LabTestKit`, {
            fixture: "LaboratoryService/authenticatedPcrTest.json",
        });
    });

    afterEach(() => {
        Cypress.session.clearAllSavedSessions();
    });

    it("Successful Test Kit", () => {
        // Authenticated PcrTest Registration Form
        cy.log("Validate Authenticated PcrTest Registration Form");
        cy.get(testKitCodeInput).should("be.visible");
        cy.get(testTakenMinutesAgo).should("be.visible");
        cy.get(cancelBtn).should("be.visible");
        cy.get(registerKitBtn).should("be.visible");
        cy.get(pcrPrivacyStatement).should("be.visible");

        // Required Validations
        cy.log("Validate Required Validations");
        clickRegisterKitButton();
        cy.get(feedbackTestKitCodeIsRequiredSelector).should("be.visible");
        cy.get(feedbackTestTakenIsRequiredSelector).should("be.visible");

        // Test Kit Code Invalid Validations
        cy.log("Validate Test Kit Code Input");
        cy.get(testKitCodeInput).type("111");
        clickRegisterKitButton();
        cy.get(feedbackTestKitCodeValidSelector).should("be.visible");

        // get the data in the fixture.
        cy.fixture("LaboratoryService/authenticatedPcrTest.json").then(
            (data) => {
                cy.get(testKitCodeInput).clear();
                cy.get(testKitCodeInput).type(
                    data.resourcePayload.shortCodeFirst +
                        "-" +
                        data.resourcePayload.shortCodeSecond
                );
                cy.get(testTakenMinutesAgo).select(
                    getPcrTestTakenTime(
                        data.resourcePayload.testTakenMinutesAgo
                    )
                );
            }
        );
        clickRegisterKitButton();
        cy.get(registrationSuccessBanner).should("be.visible");
        cy.get(logoutBtn).should("be.visible").click();
        cy.url().should("include", "/logout");
    });

    it("Log Out Using Header", () => {
        cy.get(headerLogOutBtn).should("be.visible").click();
        cy.url().should("include", "/logout");
    });
});

describe("Authenticated Pcr Test Registration with Error", () => {
    beforeEach(() => {
        Cypress.session.clearAllSavedSessions();
        cy.configureSettings({
            covid19: {
                pcrTestEnabled: true,
            },
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.visit(pcrTestUrl);
        cy.intercept("POST", `**/Laboratory/${HDID}/LabTestKit`, {
            fixture: "LaboratoryService/authenticatedPcrTestError.json",
        });
    });

    it("Error Registration Test Kit", () => {
        // get the data in the fixture.
        cy.fixture("LaboratoryService/authenticatedPcrTestError.json").then(
            (data) => {
                cy.get(testKitCodeInput).type(
                    data.resourcePayload.shortCodeFirst +
                        "-" +
                        data.resourcePayload.shortCodeSecond
                );
                cy.get(testTakenMinutesAgo).select(
                    getPcrTestTakenTime(
                        data.resourcePayload.testTakenMinutesAgo
                    )
                );
            }
        );
        clickRegisterKitButton();
        cy.get(errorBanner).should("be.visible");
    });
});

describe("Authenticated Pcr Test Registration Previously Processed", () => {
    beforeEach(() => {
        Cypress.session.clearAllSavedSessions();
        cy.configureSettings({
            covid19: {
                pcrTestEnabled: true,
            },
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.visit(pcrTestUrl);
        cy.intercept("POST", `**/Laboratory/${HDID}/LabTestKit`, {
            fixture: "LaboratoryService/authenticatedPcrTestDuplicate.json",
        });
    });

    it("Duplicate Test Kit Registration", () => {
        // get the data in the fixture.
        cy.fixture("LaboratoryService/authenticatedPcrTestDuplicate.json").then(
            (data) => {
                cy.get(testKitCodeInput).type(
                    data.resourcePayload.shortCodeFirst +
                        "-" +
                        data.resourcePayload.shortCodeSecond
                );
                cy.get(testTakenMinutesAgo).select(
                    getPcrTestTakenTime(
                        data.resourcePayload.testTakenMinutesAgo
                    )
                );
            }
        );
        clickRegisterKitButton();
        cy.get(processedBanner).should("be.visible");
    });
});
