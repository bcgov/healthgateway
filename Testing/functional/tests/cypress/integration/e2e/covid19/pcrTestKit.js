const { AuthMethod } = require("../../../support/constants");
import { formatToDatePickerInput } from "../../../support/functions/datePicker";
import {
    clickManualRegistrationButton,
    clickRegisterKitButton,
    getPcrTestTakenTime,
} from "../../../support/functions/pcrTestKit";

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
const formDobInput = "[data-testid=dob-input]";
const stressAddressInput = "[data-testid=pcr-street-address-input]";
const cityInput = "[data-testid=pcr-city-input]";
const zipInput = "[data-testid=pcr-zip-input]";
const registrationSuccessBanner = "[data-testid=registration-success-banner]";
const continueBtn = "[data-testid=btn-continue]";
const logoutBtn = "[data-testid=logoutBtn]";
const errorBanner = "[data-testid=errorBanner]";
const processedBanner = "[data-testid=alreadyProcessedBanner]";

describe("Authenticated PCR Test Kit Registration", () => {
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
    });

    afterEach(() => {
        Cypress.session.clearAllSavedSessions();
    });

    // Disable PCR test kit test - AB#16912
    it.skip("Success with Test Kit CID", () => {
        cy.visit(`/pcrtest/${successfulTestKitCid}`);

        // populate the form with values from the fixture
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

    // Disable PCR test kit test - AB#16912
    it.skip("Already Processed with Test Kit CID", () => {
        cy.visit(`/pcrtest/${processedTestKitCid}`);

        // populate the form with values from the fixture
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

    // Disable PCR test kit test - AB#16912
    it.skip("Error with Test Kit CID", () => {
        cy.visit(`/pcrtest/${errorTestKitCid}`);

        // populate the form with values from the fixture
        cy.fixture("LaboratoryService/authenticatedPcrTestError.json").then(
            (data) => {
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

    // Disable PCR test kit test - AB#16912
    it.skip("Error with Test Kit Code", () => {
        cy.visit("/pcrtest");

        // populate the form with values from the fixture
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

describe("Unauthenticated PCR Test Kit Registration", () => {
    beforeEach(() => {
        cy.configureSettings({
            covid19: {
                pcrTestEnabled: true,
            },
        });
        cy.logout();
    });

    // Disable PCR test kit test - AB#16912
    it.skip("Success with Test Kit CID and PHN", () => {
        cy.visit(`/pcrtest/${successfulTestKitCid}`);
        clickManualRegistrationButton();

        // populate the form with values from the fixture
        cy.fixture("LaboratoryService/publicPcrTestValidPhn.json").then(
            (data) => {
                cy.get(firstNameInput).type(data.resourcePayload.firstName);
                cy.get(lastNameInput).type(data.resourcePayload.lastName);
                cy.get(phnInput).type(data.resourcePayload.phn);

                cy.get(formDobInput).type(
                    formatToDatePickerInput(data.resourcePayload.dob)
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

        cy.get(continueBtn).should("be.visible");
        cy.get(continueBtn).click();
        cy.location("pathname").should("eq", landingPagePath);
    });

    // Disable PCR test kit test - AB#16912
    it.skip("Success with Test Kit CID and No PHN", () => {
        cy.visit(`/pcrtest/${successfulTestKitCid}`);
        clickManualRegistrationButton();

        // populate the form with values from the fixture
        cy.fixture("LaboratoryService/publicPcrTestNoValidPhn.json").then(
            (data) => {
                cy.get(firstNameInput).type(data.resourcePayload.firstName);
                cy.get(lastNameInput).type(data.resourcePayload.lastName);

                cy.get("[data-testid=phn-checkbox] input[type=checkbox]").click(
                    { force: true }
                );

                cy.get(stressAddressInput).type(
                    data.resourcePayload.streetAddress
                );
                cy.get(cityInput).type(data.resourcePayload.city);
                cy.get(zipInput).type(data.resourcePayload.postalOrZip);

                cy.get(formDobInput).type(
                    formatToDatePickerInput(data.resourcePayload.dob)
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

        cy.get(continueBtn).should("be.visible");
        cy.get(continueBtn).click();
        cy.location("pathname").should("eq", landingPagePath);
    });

    // Disable PCR test kit test - AB#16912
    it.skip("Already Processed with Test Kit CID", () => {
        cy.visit(`/pcrtest/${processedTestKitCid}`);
        clickManualRegistrationButton();

        // populate the form with values from the fixture
        cy.fixture("LaboratoryService/publicPcrTestValidPhn.json").then(
            (data) => {
                cy.get(firstNameInput).type(data.resourcePayload.firstName);
                cy.get(lastNameInput).type(data.resourcePayload.lastName);
                cy.get(phnInput).type(data.resourcePayload.phn);

                cy.get(formDobInput).type(
                    formatToDatePickerInput(data.resourcePayload.dob)
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

    // Disable PCR test kit test - AB#16912
    it.skip("Error with Test Kit CID", () => {
        cy.visit(`/pcrtest/${errorTestKitCid}`);
        clickManualRegistrationButton();

        // populate the form with values from the fixture
        cy.fixture("LaboratoryService/publicPcrTestValidPhn.json").then(
            (data) => {
                cy.get(firstNameInput).type(data.resourcePayload.firstName);
                cy.get(lastNameInput).type(data.resourcePayload.lastName);
                cy.get(phnInput).type(data.resourcePayload.phn);

                cy.get(formDobInput).type(
                    formatToDatePickerInput(data.resourcePayload.dob)
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

    // Disable PCR test kit test - AB#16912
    it.skip("Error with Test Kit Code", () => {
        cy.visit("/pcrtest");
        clickManualRegistrationButton();

        // populate the form with values from the fixture
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
        cy.fixture("LaboratoryService/publicPcrTestValidPhn.json").then(
            (data) => {
                cy.get(firstNameInput).type(data.resourcePayload.firstName);
                cy.get(lastNameInput).type(data.resourcePayload.lastName);
                cy.get(phnInput).type(data.resourcePayload.phn);

                cy.get(formDobInput).type(
                    formatToDatePickerInput(data.resourcePayload.dob)
                );
            }
        );

        clickRegisterKitButton();

        cy.get(errorBanner).should("be.visible");
    });
});
