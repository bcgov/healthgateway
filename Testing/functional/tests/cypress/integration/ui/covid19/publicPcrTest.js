import { monthNames } from "../../../support/constants";
import {
    clickManualRegistrationButton,
    clickRegisterKitButton,
    getPcrTestTakenTime,
} from "../../../support/functions/pcrTestKit";

const landingPagePath = "/";
const pcrTestUrl = "/pcrtest";

// data test id for all input fields in the form
const testKitCodeField = "[data-testid=test-kit-code-input]";
const testKitCodeInput = `${testKitCodeField} input`;
const firstNameField = "[data-testid=first-name-input]";
const firstNameInput = `${firstNameField} input`;
const lastNameField = "[data-testid=last-name-input]";
const lastNameInput = `${lastNameField} input`;
const phnField = "[data-testid=phn-input]";
const phnInput = `${phnField} input`;
const pcrNoPhnInforBtn = "[data-testid=pcr-no-phn-info-button]";
const contactPhoneNumberField = "[data-testid=contact-phone-number-input]";
const contactPhoneNumberInput = `${contactPhoneNumberField} input`;
const testTakenMinutesAgo = "[data-testid=test-taken-minutes-ago]";
const cancelBtn = "[data-testid=btn-cancel]";
const registerKitBtn = "[data-testid=btn-register-kit]";
const pcrPrivacyStatement = "[data-testid=pcr-privacy-statement]";
const registrationSuccessBanner = "[data-testid=registration-success-banner]";
const continueBtn = "[data-testid=btn-continue]";
const stressAddressField = "[data-testid=pcr-street-address-input]";
const stressAddressInput = "[data-testid=pcr-street-address-input] input";
const cityField = "[data-testid=pcr-city-input]";
const cityInput = "[data-testid=pcr-city-input] input";
const zipField = "[data-testid=pcr-zip-input]";
const zipInput = "[data-testid=pcr-zip-input] input";
const dobField = "[data-testid=dob-input]";
const dobInput = "[data-testid=dob-input] input";

function inputFieldsShouldBeVisible() {
    cy.get(testKitCodeField).should("be.visible");
    cy.get(firstNameField).should("be.visible");
    cy.get(lastNameField).should("be.visible");
    cy.get(phnField).should("be.visible");
    cy.get(pcrNoPhnInforBtn).should("be.visible");
    cy.get(dobField).should("be.visible");
    cy.get(contactPhoneNumberField).should("be.visible");
    cy.get(testTakenMinutesAgo).should("be.visible");
    cy.get(cancelBtn).should("be.visible");
    cy.get(registerKitBtn).should("be.visible");
    cy.get(pcrPrivacyStatement).should("be.visible");
}

function inputAddressFieldsNotExists() {
    cy.get(stressAddressField).should("not.exist");
    cy.get(cityField).should("not.exist");
    cy.get(zipField).should("not.exist");
}

function inputAddressFieldsShouldBeVisible() {
    cy.get(stressAddressField).should("be.visible");
    cy.get(cityField).should("be.visible");
    cy.get(zipField).should("be.visible");
}

function populateDatePicker(selector, dateValue) {
    const date = new Date(dateValue);
    const year = date.getFullYear();
    const month = monthNames[date.getMonth()].substring(0, 3).toUpperCase();
    const day = date.getDate();

    cy.get(selector).type(`${year}-${month}-${day}`).blur();
}

describe("Public PcrTest Registration Form", () => {
    beforeEach(() => {
        cy.configureSettings({
            covid19: {
                pcrTestEnabled: true,
            },
        });
        cy.logout();
        cy.visit(pcrTestUrl);
    });

    // Disable PCR test kit test - AB#16912
    it.skip("Validate button options and form inputs", () => {
        // Register option buttons are available
        cy.log("Check all button visibility.");
        cy.get("[data-testid=btn-login]").should("be.visible");
        cy.get("[data-testid=btn-manual]").should("be.visible");

        // Log in button should not be present in header
        cy.get("[data-testid=loginBtn]").should("not.exist");

        // Inputs in the form not including address fields are visible when phn checkbox unchecked
        cy.log("Click manual button.");
        clickManualRegistrationButton();
        //cy.get("[data-testid=btn-manual]").should("be.visible").click();
        inputFieldsShouldBeVisible();
        inputAddressFieldsNotExists();

        // Inputs in the form including address fields are visible when phn checkbox checked
        cy.log("Check off phn checkbox.");
        cy.get("[data-testid=phn-checkbox] input")
            .should("be.enabled")
            .check({ force: true });
        inputFieldsShouldBeVisible();
        inputAddressFieldsShouldBeVisible();

        // Inputs in the form not including address fields are visible when phn checkbox unchecked
        cy.log("Un-check phn checkbox.");
        cy.get("[data-testid=phn-checkbox] input")
            .should("be.enabled")
            .uncheck({ force: true });
        inputFieldsShouldBeVisible();
        inputAddressFieldsNotExists();

        // Required Validations are visible when phn checkbox unchecked
        cy.log("Click registration kit button.");
        clickRegisterKitButton();
        cy.get(testKitCodeField).should("have.class", "v-input--error");
        cy.get(testTakenMinutesAgo).should("have.class", "v-input--error");
        cy.get(firstNameField).should("have.class", "v-input--error");
        cy.get(lastNameField).should("have.class", "v-input--error");
        cy.get(phnField).should("have.class", "v-input--error");

        // Check off phn checkbox. Inputs in the form including address fields are visible when phn checkbox checked
        cy.get("[data-testid=phn-checkbox] input")
            .should("be.enabled")
            .check({ force: true });
        inputAddressFieldsShouldBeVisible();

        // Uncheck phn checkbox. Inputs in the form not including address fields are visible when phn checkbox unchecked
        cy.get("[data-testid=phn-checkbox] input")
            .should("be.enabled")
            .uncheck({ force: true });
        inputFieldsShouldBeVisible();
        inputAddressFieldsNotExists();

        // Test Kit Code Invalid Validations
        cy.get(testKitCodeInput).type("111");
        cy.get(firstNameInput).type("Princess");
        cy.get(testKitCodeField).should("have.class", "v-input--error");

        // Phone number Invalid Validations
        cy.get(testKitCodeInput).clear();
        cy.get(testKitCodeInput).type("45YFKE7-EMQY1");
        cy.get(contactPhoneNumberInput).type(" ");
        cy.vSelect(testTakenMinutesAgo, getPcrTestTakenTime(5));
    });
});

describe("Public PcrTest Registration Submission with Valid PHN", () => {
    beforeEach(() => {
        cy.configureSettings({
            covid19: {
                pcrTestEnabled: true,
            },
        });
        cy.logout();
        cy.visit(pcrTestUrl);
        cy.intercept("POST", "**/PublicLaboratory/LabTestKit", {
            fixture: "LaboratoryService/publicPcrTestValidPhn.json",
        });
    });

    // Disable PCR test kit test - AB#16912
    it.skip("Successful Test Kit with Valid PHN", () => {
        clickManualRegistrationButton();
        // get the data in the fixture.
        cy.fixture("LaboratoryService/publicPcrTestValidPhn.json").then(
            (data) => {
                cy.get(testKitCodeInput).type(
                    data.resourcePayload.shortCodeFirst +
                        "-" +
                        data.resourcePayload.shortCodeSecond
                );
                cy.get(firstNameInput).type(data.resourcePayload.firstName);
                cy.get(lastNameInput).type(data.resourcePayload.lastName);
                cy.get(phnInput).type(data.resourcePayload.phn);

                populateDatePicker(dobInput, data.resourcePayload.dob);
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

        cy.get(continueBtn).should("be.visible").click();
        cy.location("pathname").should("eq", landingPagePath);
    });
});

describe("Public PcrTest Registration Submission with no valid PHN", () => {
    beforeEach(() => {
        cy.configureSettings({
            covid19: {
                pcrTestEnabled: true,
            },
        });
        cy.logout();
        cy.visit(pcrTestUrl);
        cy.intercept("POST", "**/PublicLaboratory/LabTestKit", {
            fixture: "LaboratoryService/publicPcrTestNoValidPhn.json",
        });
    });

    // Disable PCR test kit test - AB#16912
    it.skip("Successful Test Kit with no Valid PHN", () => {
        clickManualRegistrationButton();
        // get the data in the fixture.
        cy.fixture("LaboratoryService/publicPcrTestNoValidPhn.json").then(
            (data) => {
                cy.get(testKitCodeInput).type(
                    data.resourcePayload.shortCodeFirst +
                        "-" +
                        data.resourcePayload.shortCodeSecond
                );
                cy.get(firstNameInput).type(data.resourcePayload.firstName);
                cy.get(lastNameInput).type(data.resourcePayload.lastName);

                cy.get("[data-testid=phn-checkbox] input")
                    .should("be.enabled")
                    .check({ force: true });

                cy.get(stressAddressInput).type(
                    data.resourcePayload.streetAddress
                );
                cy.get(cityInput).type(data.resourcePayload.city);
                cy.get(zipInput).type(data.resourcePayload.postalOrZip);

                populateDatePicker(dobInput, data.resourcePayload.dob);
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

        cy.get(continueBtn).should("be.visible").click();
        cy.location("pathname").should("eq", landingPagePath);
    });
});

describe("Public PcrTest Registration with Feature Disabled", () => {
    beforeEach(() => {
        cy.configureSettings({});
        cy.logout();
        cy.visit(pcrTestUrl);
    });

    // Disable PCR test kit test - AB#16912
    it.skip("HTTP 401", () => {
        cy.visit("/unauthorized");
        cy.contains("h1", "401");
    });
});

describe("Public PcrTest Registration with Test Kit Id", () => {
    beforeEach(() => {
        cy.configureSettings({
            covid19: {
                pcrTestEnabled: true,
            },
        });
        cy.logout();
        cy.visit(`${pcrTestUrl}/222BAAB1-8C6E-4FA1-86ED-C4E3517A16A2`);
    });

    // Disable PCR test kit test - AB#16912
    it.skip("Register options buttons are available", () => {
        cy.get("[data-testid=btn-login]").should("be.visible");
        cy.get("[data-testid=btn-manual]").should("be.visible");
    });
});

describe("Public PcrTest Registration with Test Kit Id with Feature Disabled", () => {
    beforeEach(() => {
        cy.configureSettings({});
        cy.logout();
        cy.visit(`${pcrTestUrl}/222BAAB1-8C6E-4FA1-86ED-C4E3517A16A2`);
    });

    // Disable PCR test kit test - AB#16912
    it.skip("HTTP 401", () => {
        cy.visit("/unauthorized");
        cy.contains("h1", "401");
    });
});
