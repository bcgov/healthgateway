import {
    getYear,
    getMonth,
    getDay,
    selectorShouldBeVisible,
    selectorShouldNotExists,
    getPcrTestTakenTime,
    selectOption,
} from "../../../support/utils";

const pcrTestUrl = "/pcrtest";

// data test id for all input fields in the form
const testKitCodeInput = "[data-testid=test-kit-code-input]";
const firstNameInput = "[data-testid=first-name-input]";
const lastNameInput = "[data-testid=last-name-input]";
const phnInput = "[data-testid=phn-input]";
const pcrNoPhnInforBtn = "[data-testid=pcr-no-phn-info-button]";
const formSelectYear = "[data-testid=formSelectYear]";
const formSelectMonth = "[data-testid=formSelectMonth]";
const formSelectDay = "[data-testid=formSelectDay]";
const contactPhoneNumberInput = "[data-testid=contact-phone-number-input]";
const testTakenMinutesAgo = "[data-testid=test-taken-minutes-ago]";
const cancelBtn = "[data-testid=btn-cancel]";
const registerKitBtn = "[data-testid=btn-register-kit]";
const pcrPrivacyStatement = "[data-testid=pcr-privacy-statement]";
const registrationSuccessBanner = "[data-testid=registration-success-banner]";
const stressAddressInput = "[data-testid=pcr-street-address-input]";
const cityInput = "[data-testid=pcr-city-input]";
const zipInput = "[data-testid=pcr-zip-input]";

// data test id for input required validations
const feedbackTestKitCodeIsRequiredSelector =
    "[data-testid=feedback-testkitcode-is-required]";
const feedbackFirstNameIsRequiredSelector =
    "[data-testid=feedback-firstname-is-required]";
const feedbackLastNameIsRequiredSelector =
    "[data-testid=feedback-lastname-is-required]";
const feedbackPhnIsRequiredSelector = "[data-testid=feedback-phn-is-required]";
const feedbackDoBIsRequiredSelector = "[data-testid=feedback-dob-is-required]";
const feedbackTestTakenIsRequiredSelector =
    "[data-testid=feedback-testtaken-is-required]";
const feedbackStreetAddressIsRequiredSelector =
    "[data-testid=feedback-streetaddress-is-required]";
const feedbackCityIsRequiredSelector =
    "[data-testid=feedback-city-is-required]";
const feedbackPostalIsRequiredSelector =
    "[data-testid=feedback-postal-is-required]";

// data test id for input invalid validations
const feedbackTestKitCodeValidSelector =
    "[data-testid=feedback-testkitcode-is-invalid]";
const feedbackPhoneNumberValidSelector =
    "[data-testid=feedback-phonenumber-valid]";

function clickManualRegistrationButton() {
    cy.get("[data-testid=btn-manual]")
        .should("be.enabled", "be.visible")
        .click();
}

function clickRegisterKitButton() {
    cy.get("[data-testid=btn-register-kit]")
        .should("be.enabled", "be.visible")
        .click();
}

function inputFieldsShouldBeVisible() {
    selectorShouldBeVisible(testKitCodeInput);
    selectorShouldBeVisible(firstNameInput);
    selectorShouldBeVisible(lastNameInput);
    selectorShouldBeVisible(phnInput);
    selectorShouldBeVisible(pcrNoPhnInforBtn);
    selectorShouldBeVisible(formSelectYear);
    selectorShouldBeVisible(formSelectMonth);
    selectorShouldBeVisible(formSelectDay);
    selectorShouldBeVisible(contactPhoneNumberInput);
    selectorShouldBeVisible(testTakenMinutesAgo);
    selectorShouldBeVisible(cancelBtn);
    selectorShouldBeVisible(registerKitBtn);
    selectorShouldBeVisible(pcrPrivacyStatement);
}

function inputAddressFieldsNotExists() {
    selectorShouldNotExists(stressAddressInput);
    selectorShouldNotExists(cityInput);
    selectorShouldNotExists(zipInput);
}

function inputAddressFieldsShouldBeVisible() {
    selectorShouldBeVisible(stressAddressInput);
    selectorShouldBeVisible(cityInput);
    selectorShouldBeVisible(zipInput);
}

describe("Public PcrTest Registration Form", () => {
    beforeEach(() => {
        cy.enableModules("PcrTest");
        cy.visit(pcrTestUrl);
    });

    it("Register options buttons are available", () => {
        selectorShouldBeVisible("[data-testid=btn-login]");
        selectorShouldBeVisible("[data-testid=btn-manual]");
    });

    it("Inputs in the form are visible with valid PHN", () => {
        cy.get("[data-testid=btn-manual]").should("be.visible").click();
        inputFieldsShouldBeVisible();
        inputAddressFieldsNotExists();
    });

    it("Inputs in the form are visible with no valid PHN", () => {
        cy.get("[data-testid=btn-manual]").should("be.visible").click();
        inputFieldsShouldBeVisible();
        cy.get("[data-testid=phn-checkbox]")
            .should("be.enabled")
            .check({ force: true });
        inputAddressFieldsShouldBeVisible();
    });

    it("Required Validations are visible with valid PHN", () => {
        clickManualRegistrationButton();
        clickRegisterKitButton();
        selectorShouldBeVisible(feedbackTestKitCodeIsRequiredSelector);
        selectorShouldBeVisible(feedbackFirstNameIsRequiredSelector);
        selectorShouldBeVisible(feedbackLastNameIsRequiredSelector);
        selectorShouldBeVisible(feedbackPhnIsRequiredSelector);
        selectorShouldBeVisible(feedbackDoBIsRequiredSelector);
        selectorShouldBeVisible(feedbackTestTakenIsRequiredSelector);
        selectorShouldNotExists(feedbackStreetAddressIsRequiredSelector);
        selectorShouldNotExists(feedbackCityIsRequiredSelector);
        selectorShouldNotExists(feedbackPostalIsRequiredSelector);
    });

    it("No PHN available required validations", () => {
        clickManualRegistrationButton();
        cy.get("[data-testid=phn-checkbox]")
            .should("be.enabled")
            .check({ force: true });
        inputAddressFieldsShouldBeVisible();
        clickRegisterKitButton();

        // Address information should be visible.
        selectorShouldBeVisible(feedbackStreetAddressIsRequiredSelector);
        selectorShouldBeVisible(feedbackCityIsRequiredSelector);
        selectorShouldBeVisible(feedbackPostalIsRequiredSelector);
    });

    it("Test Kit Code Invalid Validations", () => {
        clickManualRegistrationButton();
        cy.get(testKitCodeInput).type("111");
        cy.get(firstNameInput).type("Princess");
        selectorShouldBeVisible(feedbackTestKitCodeValidSelector);
    });

    it("Phone number Invalid Validations", () => {
        clickManualRegistrationButton();
        cy.get(contactPhoneNumberInput).type(" ");
        selectOption(testTakenMinutesAgo, getPcrTestTakenTime(5));
        selectorShouldBeVisible(feedbackPhoneNumberValidSelector);
    });
});

describe("Public PcrTest Registration Submission with Valid PHN", () => {
    beforeEach(() => {
        cy.enableModules("PcrTest");
        cy.visit(pcrTestUrl);
        cy.intercept("POST", "**/v1/api/PublicLaboratory/LabTestKit", {
            fixture: "LaboratoryService/publicPcrTestValidPhn.json",
        });
    });

    it("Successful Test Kit with Valid PHN", () => {
        clickManualRegistrationButton();
        // get the data in the fixture.
        cy.fixture("LaboratoryService/publicPcrTestValidPhn.json").then(
            function (data) {
                cy.get(testKitCodeInput).type(
                    data.resourcePayload.shortCodeFirst +
                        "-" +
                        data.resourcePayload.shortCodeSecond
                );
                cy.get(firstNameInput).type(data.resourcePayload.firstName);
                cy.get(lastNameInput).type(data.resourcePayload.lastName);
                cy.get(phnInput).type(data.resourcePayload.phn);

                selectOption(
                    formSelectYear,
                    getYear(data.resourcePayload.dob).toString()
                );
                selectOption(
                    formSelectMonth,
                    getMonth(data.resourcePayload.dob).toString()
                );
                selectOption(
                    formSelectDay,
                    getDay(data.resourcePayload.dob).toString()
                );
                selectOption(
                    testTakenMinutesAgo,
                    getPcrTestTakenTime(
                        data.resourcePayload.testTakenMinutesAgo
                    )
                );
            }
        );

        clickRegisterKitButton();

        selectorShouldBeVisible(registrationSuccessBanner);
    });
});

describe("Public PcrTest Registration Submission with no valid PHN", () => {
    beforeEach(() => {
        cy.enableModules("PcrTest");
        cy.visit(pcrTestUrl);
        cy.intercept("POST", "**/v1/api/PublicLaboratory/LabTestKit", {
            fixture: "LaboratoryService/publicPcrTestNoValidPhn.json",
        });
    });

    it("Successful Test Kit with no Valid PHN", () => {
        clickManualRegistrationButton();
        // get the data in the fixture.
        cy.fixture("LaboratoryService/publicPcrTestNoValidPhn.json").then(
            function (data) {
                cy.get(testKitCodeInput).type(
                    data.resourcePayload.shortCodeFirst +
                        "-" +
                        data.resourcePayload.shortCodeSecond
                );
                cy.get(firstNameInput).type(data.resourcePayload.firstName);
                cy.get(lastNameInput).type(data.resourcePayload.lastName);

                cy.get("[data-testid=phn-checkbox]")
                    .should("be.enabled")
                    .check({ force: true });

                cy.get(stressAddressInput).type(
                    data.resourcePayload.streetAddress
                );
                cy.get(cityInput).type(data.resourcePayload.city);
                cy.get(zipInput).type(data.resourcePayload.postalOrZip);

                selectOption(
                    formSelectYear,
                    getYear(data.resourcePayload.dob).toString()
                );
                selectOption(
                    formSelectMonth,
                    getMonth(data.resourcePayload.dob).toString()
                );
                selectOption(
                    formSelectDay,
                    getDay(data.resourcePayload.dob).toString()
                );
                selectOption(
                    testTakenMinutesAgo,
                    getPcrTestTakenTime(
                        data.resourcePayload.testTakenMinutesAgo
                    )
                );
            }
        );

        clickRegisterKitButton();

        selectorShouldBeVisible(registrationSuccessBanner);
    });
});

describe("Public PcrTest Registration Submission Module Disabled", () => {
    beforeEach(() => {
        cy.enableModules();
        cy.visit(pcrTestUrl);
    });

    it("HTTP 401", () => {
        cy.visit("/unauthorized");
        cy.contains("h1", "401");
    });
});

describe("Public PcrTest Registration with Test Kit Id", () => {
    beforeEach(() => {
        cy.enableModules("PcrTest");
        cy.visit(`${pcrTestUrl}/222BAAB1-8C6E-4FA1-86ED-C4E3517A16A2`);
    });

    it("Register options buttons are available", () => {
        selectorShouldBeVisible("[data-testid=btn-login]");
        selectorShouldBeVisible("[data-testid=btn-manual]");
    });
});

describe("Public PcrTest Registration with Test Kit Id with Disable Module", () => {
    beforeEach(() => {
        cy.enableModules();
        cy.visit(`${pcrTestUrl}/222BAAB1-8C6E-4FA1-86ED-C4E3517A16A2`);
    });

    it("HTTP 401", () => {
        cy.visit("/unauthorized");
        cy.contains("h1", "401");
    });
});
