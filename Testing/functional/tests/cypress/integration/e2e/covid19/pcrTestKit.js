import {
    getYear,
    getMonth,
    getDay,
    selectorShouldBeVisible,
    getPcrTestTakenTime,
    selectOption,
} from "../../../support/utils";
const { AuthMethod } = require("../../../support/constants");

const landingPagePath = "/";

const successfulTestKitCid = "DBE9D07A-8BAF-4513-8866-63ABD4EAB6E4";
const processedTestKitCid = "3BC58EAC-9241-4E23-9134-D451D7A18FFB";
const errorTestKitCid = "ABCDEFGH-1234-IJKL-5678-MNOPQRSTUVWX";

// data test id for all input fields in the form
const testKitCodeInput = "[data-testid=test-kit-code-input]";
const testTakenMinutesAgo = "[data-testid=test-taken-minutes-ago]";
const firstNameInput = "[data-testid=first-name-input]";
const lastNameInput = "[data-testid=last-name-input]";
const phnInput = "[data-testid=phn-input]";
const formSelectYear = "[data-testid=formSelectYear]";
const formSelectMonth = "[data-testid=formSelectMonth]";
const formSelectDay = "[data-testid=formSelectDay]";
const stressAddressInput = "[data-testid=pcr-street-address-input]";
const cityInput = "[data-testid=pcr-city-input]";
const zipInput = "[data-testid=pcr-zip-input]";
const registrationSuccessBanner = "[data-testid=registration-success-banner]";
const continueBtn = "[data-testid=btn-continue]";
const logoutBtn = "[data-testid=logoutBtn]";
const errorBanner = "[data-testid=errorBanner]";
const processedBanner = "[data-testid=alreadyProcessedBanner]";

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

describe("Authenticated PCR Test Kit Registration", () => {
    beforeEach(() => {
        cy.enableModules("PcrTest");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
    });

    it("Success with Test Kit CID", () => {
        cy.visit(`/pcrtest/${successfulTestKitCid}`);

        // populate the form with values from the fixture
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

    it("Already Processed with Test Kit CID", () => {
        cy.visit(`/pcrtest/${processedTestKitCid}`);

        // populate the form with values from the fixture
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

    it("Error with Test Kit CID", () => {
        cy.visit(`/pcrtest/${errorTestKitCid}`);

        // populate the form with values from the fixture
        cy.fixture("LaboratoryService/authenticatedPcrTestError.json").then(
            function (data) {
                selectOption(
                    testTakenMinutesAgo,
                    getPcrTestTakenTime(
                        data.resourcePayload.testTakenMinutesAgo
                    )
                );
            }
        );

        clickRegisterKitButton();

        selectorShouldBeVisible(errorBanner);
    });

    it("Error with Test Kit Code", () => {
        cy.visit("/pcrtest");

        // populate the form with values from the fixture
        cy.fixture("LaboratoryService/authenticatedPcrTestError.json").then(
            function (data) {
                cy.get(testKitCodeInput).type(
                    data.resourcePayload.shortCodeFirst +
                        "-" +
                        data.resourcePayload.shortCodeSecond
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

        selectorShouldBeVisible(errorBanner);
    });
});

describe("Unauthenticated PCR Test Kit Registration", () => {
    beforeEach(() => {
        cy.enableModules("PcrTest");
    });

    it("Success with Test Kit CID and PHN", () => {
        cy.visit(`/pcrtest/${successfulTestKitCid}`);
        clickManualRegistrationButton();

        // populate the form with values from the fixture
        cy.fixture("LaboratoryService/publicPcrTestValidPhn.json").then(
            function (data) {
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

        selectorShouldBeVisible(continueBtn);
        cy.get(continueBtn).click();
        cy.location("pathname").should("eq", landingPagePath);
    });

    it("Success with Test Kit CID and No PHN", () => {
        cy.visit(`/pcrtest/${successfulTestKitCid}`);
        clickManualRegistrationButton();

        // populate the form with values from the fixture
        cy.fixture("LaboratoryService/publicPcrTestNoValidPhn.json").then(
            function (data) {
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

        selectorShouldBeVisible(continueBtn);
        cy.get(continueBtn).click();
        cy.location("pathname").should("eq", landingPagePath);
    });

    it("Already Processed with Test Kit CID", () => {
        cy.visit(`/pcrtest/${processedTestKitCid}`);
        clickManualRegistrationButton();

        // populate the form with values from the fixture
        cy.fixture("LaboratoryService/publicPcrTestValidPhn.json").then(
            function (data) {
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

        selectorShouldBeVisible(processedBanner);
    });

    it("Error with Test Kit CID", () => {
        cy.visit(`/pcrtest/${errorTestKitCid}`);
        clickManualRegistrationButton();

        // populate the form with values from the fixture
        cy.fixture("LaboratoryService/publicPcrTestValidPhn.json").then(
            function (data) {
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

        selectorShouldBeVisible(errorBanner);
    });

    it("Error with Test Kit Code", () => {
        cy.visit("/pcrtest");
        clickManualRegistrationButton();

        // populate the form with values from the fixture
        cy.fixture("LaboratoryService/authenticatedPcrTestError.json").then(
            function (data) {
                cy.get(testKitCodeInput).type(
                    data.resourcePayload.shortCodeFirst +
                        "-" +
                        data.resourcePayload.shortCodeSecond
                );
                selectOption(
                    testTakenMinutesAgo,
                    getPcrTestTakenTime(
                        data.resourcePayload.testTakenMinutesAgo
                    )
                );
            }
        );
        cy.fixture("LaboratoryService/publicPcrTestValidPhn.json").then(
            function (data) {
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
            }
        );

        clickRegisterKitButton();

        selectorShouldBeVisible(errorBanner);
    });
});
