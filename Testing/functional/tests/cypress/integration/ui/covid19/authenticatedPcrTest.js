import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";
import {
    clickRegisterKitButton,
    getPcrTestTakenTime,
} from "../../../support/functions/pcrTestKit";

const pcrTestUrl = "/pcrtest";
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

// data test id for all input fields in the form
const testKitCodeField = "[data-testid=test-kit-code-input]";
const testKitCodeInput = `${testKitCodeField} input`;
const testTakenMinutesAgo = "[data-testid=test-taken-minutes-ago]";
const cancelBtn = "[data-testid=btn-cancel]";
const registerKitBtn = "[data-testid=btn-register-kit]";
const pcrPrivacyStatement = "[data-testid=pcr-privacy-statement]";
const registrationSuccessBanner = "[data-testid=registration-success-banner]";
const logoutBtn = "[data-testid=logoutBtn]";
const headerLogOutBtn = "[data-testid=header-log-out-button]";
const errorBanner = "[data-testid=errorBanner]";
const processedBanner = "[data-testid=alreadyProcessedBanner]";

describe("Authenticated Pcr Test Registration", () => {
    beforeEach(() => {
        setupStandardFixtures();

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

    // Disable PCR test kit test - AB#16912
    it.skip("Successful Test Kit", () => {
        // Authenticated PcrTest Registration Form
        cy.log("Validate Authenticated PcrTest Registration Form");
        cy.get(testKitCodeField).should("be.visible");
        cy.get(testTakenMinutesAgo).should("be.visible");
        cy.get(cancelBtn).should("be.visible");
        cy.get(registerKitBtn).should("be.visible");
        cy.get(pcrPrivacyStatement).should("be.visible");

        // Required Validations
        cy.log("Validate Required Validations");
        clickRegisterKitButton();
        cy.get(testKitCodeField).should("have.class", "v-input--error");

        // Test Kit Code Invalid Validations
        cy.log("Validate Test Kit Code Input");
        cy.get(testKitCodeInput).type("111");
        clickRegisterKitButton();
        cy.get(testKitCodeField).should("have.class", "v-input--error");

        // get the data in the fixture.
        cy.fixture("LaboratoryService/authenticatedPcrTest.json").then(
            (data) => {
                cy.get(testKitCodeInput).clear();
                cy.get(testKitCodeInput).type(
                    data.resourcePayload.shortCodeFirst +
                        "-" +
                        data.resourcePayload.shortCodeSecond
                );
                cy.vSelect(
                    testTakenMinutesAgo,
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

    // Disable PCR test kit test - AB#16912
    it.skip("Log Out Using Header", () => {
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

        setupStandardFixtures();

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

    // Disable PCR test kit test - AB#16912
    it.skip("Error Registration Test Kit", () => {
        // get the data in the fixture.
        cy.fixture("LaboratoryService/authenticatedPcrTestError.json").then(
            (data) => {
                cy.get(testKitCodeInput).type(
                    data.resourcePayload.shortCodeFirst +
                        "-" +
                        data.resourcePayload.shortCodeSecond
                );
                cy.vSelect(
                    testTakenMinutesAgo,
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

        setupStandardFixtures();

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

    // Disable PCR test kit test - AB#16912
    it.skip("Duplicate Test Kit Registration", () => {
        // get the data in the fixture.
        cy.fixture("LaboratoryService/authenticatedPcrTestDuplicate.json").then(
            (data) => {
                cy.get(testKitCodeInput).type(
                    data.resourcePayload.shortCodeFirst +
                        "-" +
                        data.resourcePayload.shortCodeSecond
                );
                cy.vSelect(
                    testTakenMinutesAgo,
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
