import {
    selectorShouldBeVisible,
    getPcrTestTakenTime,
    selectOption,
} from "../../../support/utils";
const { AuthMethod } = require("../../../support/constants");

const pcrTestUrl = "/pcrtest/222BAAB1-8C6E-4FA1-86ED-C4E3517A16A2";
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

// data test id for all input fields in the form
const testTakenMinutesAgo = "[data-testid=test-taken-minutes-ago]";
const cancelBtn = "[data-testid=btn-cancel]";
const registerKitBtn = "[data-testid=btn-register-kit]";
const pcrPrivacyStatement = "[data-testid=pcr-privacy-statement]";
const registrationSuccessBanner = "[data-testid=registration-success-banner]";
const logoutBtn = "[data-testid=logoutBtn]";
const errorBanner = "[data-testid=errorBanner]";
const processedBanner = "[data-testid=alreadyProcessedBanner]";

// data test id for input required validations
const feedbackTestTakenIsRequiredSelector =
    "[data-testid=feedback-testtaken-is-required]";

// data test id for input invalid validations
const feedbackTestKitCodeValidSelector =
    "[data-testid=feedback-testkitcode-is-invalid]";

function clickRegisterKitButton() {
    cy.get("[data-testid=btn-register-kit]")
        .should("be.enabled", "be.visible")
        .click();
}

describe("Authenticated Pcr Test Registration", () => {
    beforeEach(() => {
        cy.enableModules("PcrTest");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.visit(pcrTestUrl);
        cy.intercept("POST", `**/v1/api/Laboratory/${HDID}/LabTestKit`, {
            fixture: "LaboratoryService/authenticatedPcrTestWithTestKit.json",
        });
    });

    it("Authenticated PcrTest Registration Form", () => {
        selectorShouldBeVisible(testTakenMinutesAgo);
        selectorShouldBeVisible(cancelBtn);
        selectorShouldBeVisible(registerKitBtn);
        selectorShouldBeVisible(pcrPrivacyStatement);
    });

    it("Required Validations", () => {
        clickRegisterKitButton();
        selectorShouldBeVisible(feedbackTestTakenIsRequiredSelector);
    });

    it("Successful Test Kit", () => {
        // get the data in the fixture.
        cy.fixture(
            "LaboratoryService/authenticatedPcrTestWithTestKit.json"
        ).then(function (data) {
            selectOption(
                testTakenMinutesAgo,
                getPcrTestTakenTime(data.resourcePayload.testTakenMinutesAgo)
            );
        });
        clickRegisterKitButton();
        selectorShouldBeVisible(registrationSuccessBanner);
        selectorShouldBeVisible(logoutBtn);
        cy.get(logoutBtn).click();
        cy.url().should("include", "/logout");
    });
});

describe("Authenticated Pcr Test Registration with Test Kit ID (Error)", () => {
    beforeEach(() => {
        cy.enableModules("PcrTest");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.visit(pcrTestUrl);
        cy.intercept("POST", `**/v1/api/Laboratory/${HDID}/LabTestKit`, {
            fixture:
                "LaboratoryService/authenticatedPcrTestErrorWithTestKit.json",
        });
    });

    it("Authenticated PcrTest Registration Form", () => {
        selectorShouldBeVisible(testTakenMinutesAgo);
        selectorShouldBeVisible(cancelBtn);
        selectorShouldBeVisible(registerKitBtn);
        selectorShouldBeVisible(pcrPrivacyStatement);
    });

    it("Required Validations", () => {
        clickRegisterKitButton();
        selectorShouldBeVisible(feedbackTestTakenIsRequiredSelector);
    });

    it("Error Test Kit", () => {
        // get the data in the fixture.
        cy.fixture(
            "LaboratoryService/authenticatedPcrTestErrorWithTestKit.json"
        ).then(function (data) {
            selectOption(
                testTakenMinutesAgo,
                getPcrTestTakenTime(data.resourcePayload.testTakenMinutesAgo)
            );
        });
        clickRegisterKitButton();
        selectorShouldBeVisible(errorBanner);
    });
});

describe("Previously Registered Test Kit", () => {
    beforeEach(() => {
        cy.enableModules("PcrTest");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.visit(pcrTestUrl);
        cy.intercept("POST", `**/v1/api/Laboratory/${HDID}/LabTestKit`, {
            fixture:
                "LaboratoryService/authenticatedPcrTestDuplicateWithTestKit.json",
        });
    });

    it("Already Processed Test Kit", () => {
        // get the data in the fixture.
        cy.fixture(
            "LaboratoryService/authenticatedPcrTestDuplicateWithTestKit.json"
        ).then(function (data) {
            selectOption(
                testTakenMinutesAgo,
                getPcrTestTakenTime(data.resourcePayload.testTakenMinutesAgo)
            );
        });
        clickRegisterKitButton();
        selectorShouldBeVisible(processedBanner);
    });
});
