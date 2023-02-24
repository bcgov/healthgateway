import {
    clickManualRegistrationButton,
    clickRegisterKitButton,
    getPcrTestTakenTime,
} from "../../../support/functions/pcrTestKit";

const landingPagePath = "/";
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
const continueBtn = "[data-testid=btn-continue]";
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

function inputFieldsShouldBeVisible() {
    cy.get(testKitCodeInput).should("be.visible");
    cy.get(firstNameInput).should("be.visible");
    cy.get(lastNameInput).should("be.visible");
    cy.get(phnInput).should("be.visible");
    cy.get(pcrNoPhnInforBtn).should("be.visible");
    cy.get(formSelectYear).should("be.visible");
    cy.get(formSelectMonth).should("be.visible");
    cy.get(formSelectDay).should("be.visible");
    cy.get(contactPhoneNumberInput).should("be.visible");
    cy.get(testTakenMinutesAgo).should("be.visible");
    cy.get(cancelBtn).should("be.visible");
    cy.get(registerKitBtn).should("be.visible");
    cy.get(pcrPrivacyStatement).should("be.visible");
}

function inputAddressFieldsNotExists() {
    cy.get(stressAddressInput).should("not.exist");
    cy.get(cityInput).should("not.exist");
    cy.get(zipInput).should("not.exist");
}

function inputAddressFieldsShouldBeVisible() {
    cy.get(stressAddressInput).should("be.visible");
    cy.get(cityInput).should("be.visible");
    cy.get(zipInput).should("be.visible");
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

    it("Validate button options and form inputs", () => {
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
        cy.get("[data-testid=phn-checkbox]")
            .should("be.enabled")
            .check({ force: true });
        inputFieldsShouldBeVisible();
        inputAddressFieldsShouldBeVisible();

        // Inputs in the form not including address fields are visible when phn checkbox unchecked
        cy.log("Un-check phn checkbox.");
        cy.get("[data-testid=phn-checkbox]")
            .should("be.enabled")
            .uncheck({ force: true });
        inputFieldsShouldBeVisible();
        inputAddressFieldsNotExists();

        // Required Validations are visible when phn checkbox unchecked
        cy.log("Click registration kit button.");
        clickRegisterKitButton();
        cy.get(feedbackTestKitCodeIsRequiredSelector).should("be.visible");
        cy.get(feedbackFirstNameIsRequiredSelector).should("be.visible");
        cy.get(feedbackLastNameIsRequiredSelector).should("be.visible");
        cy.get(feedbackPhnIsRequiredSelector).should("be.visible");
        cy.get(feedbackDoBIsRequiredSelector).should("be.visible");
        cy.get(feedbackTestTakenIsRequiredSelector).should("be.visible");
        cy.get(feedbackStreetAddressIsRequiredSelector).should("not.exist");
        cy.get(feedbackCityIsRequiredSelector).should("not.exist");
        cy.get(feedbackPostalIsRequiredSelector).should("not.exist");

        // Check off phn checkbox. Inputs in the form including address fields are visible when phn checkbox checked
        cy.get("[data-testid=phn-checkbox]")
            .should("be.enabled")
            .check({ force: true });
        inputAddressFieldsShouldBeVisible();

        // Required Validations for Address information are visible when phn checkbox checked.
        cy.get(feedbackStreetAddressIsRequiredSelector).should("be.visible");
        cy.get(feedbackCityIsRequiredSelector).should("be.visible");
        cy.get(feedbackPostalIsRequiredSelector).should("be.visible");

        // Uncheck phn checkbox. Inputs in the form not including address fields are visible when phn checkbox unchecked
        cy.get("[data-testid=phn-checkbox]")
            .should("be.enabled")
            .uncheck({ force: true });
        inputFieldsShouldBeVisible();
        inputAddressFieldsNotExists();

        // Test Kit Code Invalid Validations
        cy.get(testKitCodeInput).type("111");
        cy.get(firstNameInput).type("Princess");
        cy.get(feedbackTestKitCodeValidSelector).should("be.visible");

        // Phone number Invalid Validations
        cy.get(testKitCodeInput).clear();
        cy.get(testKitCodeInput).type("45YFKE7-EMQY1");
        cy.get(contactPhoneNumberInput).type(" ");
        cy.get(testTakenMinutesAgo).select(getPcrTestTakenTime(5));
        cy.get(feedbackPhoneNumberValidSelector).should("be.visible");
        cy.get(feedbackTestKitCodeValidSelector).should("not.exist");
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

    it("Successful Test Kit with Valid PHN", () => {
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

                cy.populateDateDropdowns(
                    formSelectYear,
                    formSelectMonth,
                    formSelectDay,
                    data.resourcePayload.dob
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

    it("Successful Test Kit with no Valid PHN", () => {
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

                cy.get("[data-testid=phn-checkbox]")
                    .should("be.enabled")
                    .check({ force: true });

                cy.get(stressAddressInput).type(
                    data.resourcePayload.streetAddress
                );
                cy.get(cityInput).type(data.resourcePayload.city);
                cy.get(zipInput).type(data.resourcePayload.postalOrZip);

                cy.populateDateDropdowns(
                    formSelectYear,
                    formSelectMonth,
                    formSelectDay,
                    data.resourcePayload.dob
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

        cy.get(continueBtn).should("be.visible").click();
        cy.location("pathname").should("eq", landingPagePath);
    });
});

describe("Public PcrTest Registration Submission Module Disabled", () => {
    beforeEach(() => {
        cy.configureSettings({});
        cy.logout();
        cy.visit(pcrTestUrl);
    });

    it("HTTP 401", () => {
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

    it("Register options buttons are available", () => {
        cy.get("[data-testid=btn-login]").should("be.visible");
        cy.get("[data-testid=btn-manual]").should("be.visible");
    });
});

describe("Public PcrTest Registration with Test Kit Id with Disable Module", () => {
    beforeEach(() => {
        cy.configureSettings({});
        cy.logout();
        cy.visit(`${pcrTestUrl}/222BAAB1-8C6E-4FA1-86ED-C4E3517A16A2`);
    });

    it("HTTP 401", () => {
        cy.visit("/unauthorized");
        cy.contains("h1", "401");
    });
});
