import { AuthMethod } from "../../../support/constants";
import {
    clickRegisterKitButton,
    getPcrTestTakenTime,
} from "../../../support/functions/pcrTestKit";
import { setupStandardFixtures } from "../../../support/functions/intercept";

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

describe("Authenticated Pcr Test Registration", () => {
    beforeEach(() => {
        cy.configureSettings({
            covid19: {
                pcrTestEnabled: true,
            },
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.visit(pcrTestUrl);
        cy.intercept("POST", `**/Laboratory/${HDID}/LabTestKit`, {
            fixture: "LaboratoryService/authenticatedPcrTestWithTestKit.json",
        });
    });

    afterEach(() => {
        Cypress.session.clearAllSavedSessions();
    });

    // Disable PCR test kit test - AB#16912
    it.skip("Successful Test Kit", () => {
        // Authenticated PcrTest Registration Form
        cy.log("Validate Authenticated PcrTest Registration Form");
        cy.get(testTakenMinutesAgo).should("be.visible");
        cy.get(cancelBtn).should("be.visible");
        cy.get(registerKitBtn).should("be.visible");
        cy.get(pcrPrivacyStatement).should("be.visible");

        // Required Validations
        cy.log("Validate Required Validations");
        clickRegisterKitButton();
        cy.get(testTakenMinutesAgo).should("have.class", "v-input--error");

        // get the data in the fixture.
        cy.fixture(
            "LaboratoryService/authenticatedPcrTestWithTestKit.json"
        ).then((data) => {
            cy.vSelect(
                testTakenMinutesAgo,
                getPcrTestTakenTime(data.resourcePayload.testTakenMinutesAgo)
            );
        });
        clickRegisterKitButton();
        cy.get(registrationSuccessBanner).should("be.visible");
        cy.get(logoutBtn).should("be.visible").click();
        cy.url().should("include", "/logout");
    });
});

describe("Authenticated Pcr Test Registration with Test Kit ID (Error)", () => {
    beforeEach(() => {
        cy.configureSettings({
            covid19: {
                pcrTestEnabled: true,
            },
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.visit(pcrTestUrl);
        cy.intercept("POST", `**/Laboratory/${HDID}/LabTestKit`, {
            fixture:
                "LaboratoryService/authenticatedPcrTestErrorWithTestKit.json",
        });
    });

    afterEach(() => {
        Cypress.session.clearAllSavedSessions();
    });

    // Disable PCR test kit test - AB#16912
    it.skip("Error Test Kit", () => {
        // get the data in the fixture.
        cy.fixture(
            "LaboratoryService/authenticatedPcrTestErrorWithTestKit.json"
        ).then((data) => {
            cy.vSelect(
                testTakenMinutesAgo,
                getPcrTestTakenTime(data.resourcePayload.testTakenMinutesAgo)
            );
        });
        clickRegisterKitButton();
        cy.get(errorBanner).should("be.visible");
    });
});

describe("Previously Registered Test Kit", () => {
    beforeEach(() => {
        cy.configureSettings({
            covid19: {
                pcrTestEnabled: true,
            },
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.visit(pcrTestUrl);
        cy.intercept("POST", `**/Laboratory/${HDID}/LabTestKit`, {
            fixture:
                "LaboratoryService/authenticatedPcrTestDuplicateWithTestKit.json",
        });
    });

    afterEach(() => {
        Cypress.session.clearAllSavedSessions();
    });

    // Disable PCR test kit test - AB#16912
    it.skip("Already Processed Test Kit", () => {
        // get the data in the fixture.
        cy.fixture(
            "LaboratoryService/authenticatedPcrTestDuplicateWithTestKit.json"
        ).then((data) => {
            cy.vSelect(
                testTakenMinutesAgo,
                getPcrTestTakenTime(data.resourcePayload.testTakenMinutesAgo)
            );
        });
        clickRegisterKitButton();
        cy.get(processedBanner).should("be.visible");
    });
});
