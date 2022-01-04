const { monthNames } = require("../../../support/constants");

const dobYearSelector =
    "[data-testid=dateOfBirthInput] [data-testid=formSelectYear]";
const dobMonthSelector =
    "[data-testid=dateOfBirthInput] [data-testid=formSelectMonth]";
const dobDaySelector =
    "[data-testid=dateOfBirthInput] [data-testid=formSelectDay]";
const collectionDateYearSelector =
    "[data-testid=dateOfCollectionInput] [data-testid=formSelectYear]";
const collectionDateMonthSelector =
    "[data-testid=dateOfCollectionInput] [data-testid=formSelectMonth]";
const collectionDateDaySelector =
    "[data-testid=dateOfCollectionInput] [data-testid=formSelectDay]";

const feedbackPhnMustBeValidSelector = "[data-testid=feedbackPhnMustBeValid]";
const feedbackPhnIsRequiredSelector = "[data-testid=feedbackPhnIsRequired]";
const feedbackDobIsRequiredSelector = "[data-testid=feedbackDobIsRequired]";
const feedbackCollectionDateIsRequiredSelector =
    "[data-testid=feedbackCollectionDateIsRequired]";

const publicLaboratoryResultModule = "PublicLaboratoryResult";
const laboratoryModule = "Laboratory";
const covidTestUrl = "/covidtest";

function selectOption(selector, option) {
    return cy.get(selector).should("be.visible", "be.enabled").select(option);
}

function selectShouldContain(selector, value) {
    cy.get(selector)
        .children("[value=" + value + "]")
        .should("exist");
}

function selectShouldNotContain(selector, value) {
    cy.get(selector)
        .children("[value=" + value + "]")
        .should("not.exist");
}

function enterCovidTestPHN(phn) {
    cy.get("[data-testid=phnInput]")
        .should("be.visible", "be.enabled")
        .type(phn);
}

function clickCovidTestEnterButton() {
    cy.get("[data-testid=btnEnter]").should("be.enabled", "be.visible").click();
}

describe("Public User - Covid19 Test Result Page", () => {
    beforeEach(() => {
        cy.enableModules([laboratoryModule, publicLaboratoryResultModule]);
        cy.visit(covidTestUrl);
    });

    it("Public Covid19 Test - Cancel", () => {
        cy.get("[data-testid=btnCancel]").should("be.visible").click();
        cy.get("[data-testid=covid-test-button]").should("exist");
    });

    it("Public Covid19 Test - Login BC Services Card App", () => {
        cy.get("[data-testid=btnLogin]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=vaccineCardFormTitle]").should("not.exist");
    });

    it("Public Covid19 Test - Valid PHN via Select Year", () => {
        enterCovidTestPHN(Cypress.env("phn"));
        selectOption(dobYearSelector, "Year");
        cy.get("[data-testid=phnInput]")
            .should("be.visible", "be.enabled")
            .and("have.class", "form-control is-valid");
    });

    it("Public Covid19 Test - Invalid PHN via Select Year", () => {
        enterCovidTestPHN(Cypress.env("phn").replace(/.$/, ""));
        selectOption(dobYearSelector, "Year");
        cy.get(feedbackPhnMustBeValidSelector).should("be.visible");
    });

    it("Public Covid19 Test - PHN, DOB and Collection Date not entered via Click Enter", () => {
        clickCovidTestEnterButton();
        cy.get(feedbackPhnIsRequiredSelector).should("be.visible");
        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackCollectionDateIsRequiredSelector).should("be.visible");
    });

    it("Public Covid19 Test - DOB and Collection Date not entered with Invalid PHN via Click Enter", () => {
        enterCovidTestPHN(Cypress.env("phn").replace(/.$/, ""));
        clickCovidTestEnterButton();
        cy.get(feedbackPhnMustBeValidSelector).should("be.visible");
        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackCollectionDateIsRequiredSelector).should("be.visible");
    });

    it("Public Covid19 Test - DOB and Collection Date not entered via Click Enter", () => {
        enterCovidTestPHN(Cypress.env("phn"));
        clickCovidTestEnterButton();
        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackCollectionDateIsRequiredSelector).should("be.visible");
    });

    it("Public Covid19 Test - DOB and Collection Date cannot be future dated", () => {
        const d = new Date();
        const year = d.getFullYear();
        const monthNumber = d.getMonth(); //Maps to monthNames constant array. Index starts at 0
        const month = d.getMonth() + 1; //0 is January but UI index starts at 1 for January
        const nextMonth = d.getMonth() === 11 ? 1 : month + 1; //When current month is December, next month will be January
        const day = d.getDate();
        cy.log(
            "Current Date: " +
                d.toDateString() +
                " - Year: " +
                year +
                " - Month Number: " +
                monthNumber +
                " - Month: " +
                month +
                " - Next Month: " +
                nextMonth +
                " - Day: " +
                day
        );

        enterCovidTestPHN(Cypress.env("phn"));

        d.setDate(d.getDate() + 1);
        const nextYear = d.getFullYear() === year ? year + 1 : d.getFullYear();
        const nextDay = d.getDate();
        cy.log(
            "Future Date - Next Year: " + nextYear + " - Next Day: " + nextDay
        );

        // Test Future Year does not exist
        cy.log("Testing Future Year does not exist.");
        selectShouldNotContain(dobYearSelector, nextYear.toString());
        selectShouldNotContain(collectionDateYearSelector, nextYear.toString());

        // Test Current Year
        selectOption(dobYearSelector, year.toString());
        selectOption(collectionDateYearSelector, year.toString());
        cy.log("Testing Current Year.");

        if (nextMonth > 1) {
            cy.log("Next Month is: " + nextMonth);
            cy.log(
                "Current year has been set in dropdown, so if next month is 1 - January, it means current month is December. Test Future Month does not exist. Month can only be current or past month for current year."
            );
            selectShouldNotContain(dobMonthSelector, nextMonth);
            selectShouldNotContain(collectionDateMonthSelector, nextMonth);
        }

        cy.log("Test and set Current Month");
        selectOption(dobMonthSelector, monthNames[monthNumber]);
        selectOption(collectionDateMonthSelector, monthNames[monthNumber]);

        if (nextDay > 1) {
            cy.log("Next Day: " + nextDay);
            cy.log(
                "Current Year and Month have been set. If next day is 1, it means previous day was last day of current month. Next Day is associated with the current month. Test Future Day in current month does not exist."
            );
            selectShouldNotContain(dobDaySelector, nextDay);
            selectShouldNotContain(collectionDateDaySelector, nextDay);
        }
        //Test Current Day exists
        cy.log("Test Current Day exists.");
        selectShouldContain(dobDaySelector, day);
        selectShouldContain(collectionDateDaySelector, day);
    });

    it("Public Covid19 Test - DOB Year and Collection Date not entered via Click Enter", () => {
        const dobMonth = "June";
        const dobDay = "15";

        enterCovidTestPHN(Cypress.env("phn"));
        selectOption(dobMonthSelector, dobMonth);
        selectOption(dobDaySelector, dobDay);
        clickCovidTestEnterButton();
        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackCollectionDateIsRequiredSelector).should("be.visible");
    });

    it("Public Covid19 Test - DOB Month and Collection Date not entered via Click Enter", () => {
        const dobYear = "2021";
        const dobDay = "15";

        enterCovidTestPHN(Cypress.env("phn"));
        selectOption(dobYearSelector, dobYear);
        selectOption(dobDaySelector, dobDay);
        clickCovidTestEnterButton();
        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackCollectionDateIsRequiredSelector).should("be.visible");
    });

    it("Public Covid19 Test - DOB Day and Collection Date not entered via Click Enter", () => {
        const dobYear = "2021";
        const dobMonth = "June";

        enterCovidTestPHN(Cypress.env("phn"));
        selectOption(dobYearSelector, dobYear);
        selectOption(dobMonthSelector, dobMonth);
        clickCovidTestEnterButton();
        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackCollectionDateIsRequiredSelector).should("be.visible");
    });

    it("Public Covid19 Test - Collection Date Month and DOB not entered via Click Enter", () => {
        const collectionDateYear = "2021";
        const collectionDateDay = "15";

        enterCovidTestPHN(Cypress.env("phn"));
        selectOption(collectionDateYearSelector, collectionDateYear);
        selectOption(collectionDateDaySelector, collectionDateDay);
        clickCovidTestEnterButton();
        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackCollectionDateIsRequiredSelector).should("be.visible");
    });

    it("Public Covid19 Test - Collection Date Day and DOB not entered via Click Enter", () => {
        const collectionDateYear = "2021";
        const collectionDateMonth = "June";

        enterCovidTestPHN(Cypress.env("phn"));
        selectOption(collectionDateYearSelector, collectionDateYear);
        selectOption(collectionDateMonthSelector, collectionDateMonth);
        clickCovidTestEnterButton();
        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackCollectionDateIsRequiredSelector).should("be.visible");
    });

    it("Public Covid19 Test Results - Click another test", () => {
        const phn = "9735361219 ";
        const dobYear = "1994";
        const dobMonth = "June";
        const dobDay = "9";
        const collectionDateYear = "2021";
        const collectionDateMonth = "January";
        const collectionDateDay = "20";

        cy.enableModules(["Laboratory", "PublicLaboratoryResult"]);
        cy.visit("/covidTest");
        cy.intercept("GET", "**/v1/api/PublicLaboratory/CovidTests", (req) => {
            req.reply({
                fixture: "LaboratoryService/covidTest.json",
            });
        });

        enterCovidTestPHN(phn);
        selectOption(dobYearSelector, dobYear);
        selectOption(dobMonthSelector, dobMonth);
        selectOption(dobDaySelector, dobDay);
        selectOption(collectionDateYearSelector, collectionDateYear);
        selectOption(collectionDateMonthSelector, collectionDateMonth);
        selectOption(collectionDateDaySelector, collectionDateDay);

        clickCovidTestEnterButton();
        cy.get("[data-testid=public-covid-test-result-form-title]").should(
            "be.visible"
        );
        cy.get("[data-testid=public-display-name]").should("be.visible");
        cy.get("[data-testid=public-display-name]").contains(
            "BONNET PROTERVITY"
        );
        cy.get("[data-testid=test-type-1]").should("be.visible");
        cy.get("[data-testid=test-type-1]").contains("Nasal");
        cy.get("[data-testid=test-outcome-span-1]").should("be.visible");
        cy.get("[data-testid=test-outcome-span-1]").contains("NoOrderFound");
        cy.get("[data-testid=collection-date-1]").should("be.visible");
        cy.get("[data-testid=collection-date-1]").contains(
            "2021-Dec-01, 4:07 PM"
        );
        cy.get("[data-testid=test-status]").should("be.visible");
        cy.get("[data-testid=test-status]").contains("Corrected");
        cy.get("[data-testid=result-date-1]").should("be.visible");
        cy.get("[data-testid=result-date-1]").contains("2021-Dec-01, 4:07 PM");
        cy.get("[data-testid=reporting-lab]").should("be.visible");
        cy.get("[data-testid=reporting-lab]").contains("Fha");
        cy.get("[data-testid=result-description]").should("be.visible");

        cy.get("[data-testid=btnCheckAnotherTest]").should("be.visible");
        cy.get("[data-testid=btnLogin]").should("be.visible");

        cy.get("[data-testid=btnCheckAnotherTest]").click();
        cy.get("[data-testid=phnInput]").should("be.visible");
    });

    it("Public Covid19 Test - Empty Results", () => {
        const phn = "9735361219 ";
        const dobYear = "1994";
        const dobMonth = "June";
        const dobDay = "9";
        const collectionDateYear = "2021";
        const collectionDateMonth = "January";
        const collectionDateDay = "20";

        cy.intercept("GET", "**/v1/api/PublicLaboratory/CovidTests", (req) => {
            req.reply({
                fixture: "LaboratoryService/emptyCovidTest.json",
            });
        });

        enterCovidTestPHN(phn);

        selectOption(dobYearSelector, dobYear);
        selectOption(dobMonthSelector, dobMonth);
        selectOption(dobDaySelector, dobDay);
        selectOption(collectionDateYearSelector, collectionDateYear);
        selectOption(collectionDateMonthSelector, collectionDateMonth);
        selectOption(collectionDateDaySelector, collectionDateDay);
        clickCovidTestEnterButton();
        cy.get("[data-testid=public-covid-test-result-form-title]").should(
            "be.visible"
        );
        cy.get("[data-testid=btnCheckAnotherTest]").should("be.visible");
        cy.get("[data-testid=btnLogin]").should("be.visible");
    });

    it("Public Covid19 Test - Data Mismatch", () => {
        const phn = "9735361219 ";
        const dobYear = "1994";
        const dobMonth = "June";
        const dobDay = "9";
        const collectionDateYear = "2021";
        const collectionDateMonth = "January";
        const collectionDateDay = "20";

        cy.intercept("GET", "**/v1/api/PublicLaboratory/CovidTests", (req) => {
            req.reply({
                fixture: "LaboratoryService/dataMismatchCovidTest.json",
            });
        });

        enterCovidTestPHN(phn);
        selectOption(dobYearSelector, dobYear);
        selectOption(dobMonthSelector, dobMonth);
        selectOption(dobDaySelector, dobDay);
        selectOption(collectionDateYearSelector, collectionDateYear);
        selectOption(collectionDateMonthSelector, collectionDateMonth);
        selectOption(collectionDateDaySelector, collectionDateDay);
        clickCovidTestEnterButton();
        cy.get("[data-testid=error-text-description]").should("be.visible");
    });

    it("Public Covid19 Test - Invalid", () => {
        const phn = "9735361219 ";
        const dobYear = "1994";
        const dobMonth = "June";
        const dobDay = "9";
        const collectionDateYear = "2021";
        const collectionDateMonth = "January";
        const collectionDateDay = "20";

        cy.intercept("GET", "**/v1/api/PublicLaboratory/CovidTests", (req) => {
            req.reply({
                fixture: "LaboratoryService/invalidCovidTest.json",
            });
        });

        enterCovidTestPHN(phn);
        selectOption(dobYearSelector, dobYear);
        selectOption(dobMonthSelector, dobMonth);
        selectOption(dobDaySelector, dobDay);
        selectOption(collectionDateYearSelector, collectionDateYear);
        selectOption(collectionDateMonthSelector, collectionDateMonth);
        selectOption(collectionDateDaySelector, collectionDateDay);
        clickCovidTestEnterButton();
        cy.get("[data-testid=error-text-description]").should("be.visible");
    });

    it("Public Covid19 Test - Empty Results and Check another test", () => {
        const phn = "9735361219 ";
        const dobYear = "1994";
        const dobMonth = "June";
        const dobDay = "9";
        const collectionDateYear = "2021";
        const collectionDateMonth = "January";
        const collectionDateDay = "20";

        cy.intercept("GET", "**/v1/api/PublicLaboratory/CovidTests", (req) => {
            req.reply({
                fixture: "LaboratoryService/emptyCovidTest.json",
            });
        });

        enterCovidTestPHN(phn);

        selectOption(dobYearSelector, dobYear);
        selectOption(dobMonthSelector, dobMonth);
        selectOption(dobDaySelector, dobDay);
        selectOption(collectionDateYearSelector, collectionDateYear);
        selectOption(collectionDateMonthSelector, collectionDateMonth);
        selectOption(collectionDateDaySelector, collectionDateDay);

        clickCovidTestEnterButton();
        cy.get("[data-testid=public-covid-test-result-form-title]").should(
            "be.visible"
        );
        cy.get("[data-testid=btnLogin]").should("be.visible");
        cy.get("[data-testid=btnCheckAnotherTest]").should("be.visible");
        cy.get("[data-testid=btnCheckAnotherTest]").click();
        cy.get("[data-testid=phnInput]").should("be.visible");
    });
});
