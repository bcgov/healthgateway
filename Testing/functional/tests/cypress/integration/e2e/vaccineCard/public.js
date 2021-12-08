const { monthNames } = require("../../../support/constants");

const dobYearSelector =
    "[data-testid=dateOfBirthInput] [data-testid=formSelectYear]";
const dobMonthSelector =
    "[data-testid=dateOfBirthInput] [data-testid=formSelectMonth]";
const dobDaySelector =
    "[data-testid=dateOfBirthInput] [data-testid=formSelectDay]";
const dovYearSelector =
    "[data-testid=dateOfVaccineInput] [data-testid=formSelectYear]";
const dovMonthSelector =
    "[data-testid=dateOfVaccineInput] [data-testid=formSelectMonth]";
const dovDaySelector =
    "[data-testid=dateOfVaccineInput] [data-testid=formSelectDay]";

const feedbackPhnMustBeValidSelector = "[data-testid=feedbackPhnMustBeValid]";
const feedbackPhnIsRequiredSelector = "[data-testid=feedbackPhnIsRequired]";
const feedbackDobIsRequiredSelector = "[data-testid=feedbackDobIsRequired]";
const feedbackDovIsRequiredSelector = "[data-testid=feedbackDovIsRequired]";

const vaccinationStatusModule = "VaccinationStatus";
const vaccineCardUrl = "/vaccinecard";

function enterVaccineCardPHN(phn) {
    cy.get("[data-testid=phnInput]")
        .should("be.visible", "be.enabled")
        .type(phn);
}

function select(selector, value) {
    cy.get(selector).should("be.visible", "be.enabled").select(value);
}

function selectExist(selector, value) {
    cy.get(selector)
        .children("[value=" + value + "]")
        .should("exist");
}

function selectNotExist(selector, value) {
    cy.get(selector)
        .children("[value=" + value + "]")
        .should("not.exist");
}

function clickVaccineCardEnterButton() {
    cy.get("[data-testid=btnEnter]").should("be.enabled", "be.visible").click();
}

describe("Public User - Vaccine Card Page", () => {
    it("Public Vaccination Card - Cancel", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        cy.get("[data-testid=btnCancel]").should("be.visible").click();

        cy.get("[data-testid=btnVaccineCard]").should("exist");
    });

    it("Public Vaccination Card - Login BC Services Card App", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        cy.get("[data-testid=btnLogin]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=vaccineCardFormTitle]").should("not.exist");
    });

    it("Public Vaccination Card - Valid PHN via Select Year", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn"));

        select(dobYearSelector, "Year");

        cy.get("[data-testid=phnInput]")
            .should("be.visible", "be.enabled")
            .and("have.class", "form-control is-valid");
    });

    it("Public Vaccination Card - Invalid PHN via Select Year", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn").replace(/.$/, ""));

        select(dobYearSelector, "Year");

        cy.get(feedbackPhnMustBeValidSelector).should("be.visible");
    });

    it("Public Vaccination Card - PHN, DOB and DOV not entered via Click Enter", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        clickVaccineCardEnterButton();

        cy.get(feedbackPhnIsRequiredSelector).should("be.visible");
        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Public Vaccination Card - DOB and DOV not entered with Invalid PHN via Click Enter", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn").replace(/.$/, ""));

        clickVaccineCardEnterButton();

        cy.get(feedbackPhnMustBeValidSelector).should("be.visible");
        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Public Vaccination Card - DOB and DOV not entered via Click Enter", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn"));

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Public Vaccination Card - DOB and DOV cannot be future dated", () => {
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

        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn"));

        d.setDate(d.getDate() + 1);
        const nextYear = d.getFullYear() === year ? year + 1 : d.getFullYear();
        const nextDay = d.getDate();
        cy.log(
            "Future Date - Next Year: " + nextYear + " - Next Day: " + nextDay
        );

        // Test Future Year does not exist
        cy.log("Testing Future Year does not exist.");
        selectNotExist(dobYearSelector, nextYear.toString());
        selectNotExist(dovYearSelector, nextYear.toString());

        // Test Current Year
        select(dobYearSelector, year.toString());
        select(dovYearSelector, year.toString());
        cy.log("Testing Current Year.");

        if (nextMonth > 1) {
            cy.log("Next Month is: " + nextMonth);
            cy.log(
                "Current year has been set in dropdown, so if next month is 1 - January, it means current month is December. Test Future Month does not exist. Month can only be current or past month for current year."
            );
            selectNotExist(dobMonthSelector, nextMonth);
            selectNotExist(dovMonthSelector, nextMonth);
        }

        cy.log("Test and set Current Month");
        select(dobMonthSelector, monthNames[monthNumber]);
        select(dovMonthSelector, monthNames[monthNumber]);

        if (nextDay > 1) {
            cy.log("Next Day: " + nextDay);
            cy.log(
                "Current Year and Month have been set. If next day is 1, it means previous day was last day of current month. Next Day is associated with the current month. Test Future Day in current month does not exist."
            );
            selectNotExist(dobDaySelector, nextDay);
            selectNotExist(dovDaySelector, nextDay);
        }
        //Test Current Day exists
        cy.log("Test Current Day exists.");
        selectExist(dobDaySelector, day);
        selectExist(dovDaySelector, day);
    });

    it("Public Vaccination Card - DOB Year and DOV not entered via Click Enter", () => {
        const dobMonth = "June";
        const dobDay = "15";

        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn"));

        select(dobMonthSelector, dobMonth);

        select(dobDaySelector, dobDay);

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Public Vaccination Card - DOB Month and DOV not entered via Click Enter", () => {
        const dobYear = "2021";
        const dobDay = "15";

        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn"));

        select(dobYearSelector, dobYear);

        select(dobDaySelector, dobDay);

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Public Vaccination Card - DOB Day and DOV not entered via Click Enter", () => {
        const dobYear = "2021";
        const dobMonth = "June";

        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn"));

        select(dobYearSelector, dobYear);

        select(dobMonthSelector, dobMonth);

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Public Vaccination Card - DOV Year and DOB not entered via Click Enter", () => {
        const dovMonth = "June";
        const dovDay = "15";

        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn"));

        select(dovMonthSelector, dovMonth);

        select(dovDaySelector, dovDay);

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Public Vaccination Card - DOV Month and DOB not entered via Click Enter", () => {
        const dovYear = "2021";
        const dovDay = "15";

        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn"));

        select(dovYearSelector, dovYear);

        select(dovDaySelector, dovDay);

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Public Vaccination Card - DOV Day and DOB not entered via Click Enter", () => {
        const dovYear = "2021";
        const dovMonth = "June";

        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn"));

        select(dovYearSelector, dovYear);

        select(dovMonthSelector, dovMonth);

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Public Vaccination Card - Partially Vaccinated", () => {
        const phn = "9735353315";
        const dobYear = "1967";
        const dobMonth = "June";
        const dobDay = "2";
        const dovYear = "2021";
        const dovMonth = "July";
        const dovDay = "4";

        cy.enableModules([
            "Immunization",
            vaccinationStatusModule,
            "VaccinationStatusPdf",
        ]);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(phn);

        select(dobYearSelector, dobYear);
        select(dobMonthSelector, dobMonth);
        select(dobDaySelector, dobDay);
        select(dovYearSelector, dovYear);
        select(dovMonthSelector, dovMonth);
        select(dovDaySelector, dovDay);

        clickVaccineCardEnterButton();

        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");
        cy.get("[data-testid=statusPartiallyVaccinated]").should("be.visible");
    });

    it("Public Vaccination Card - Fully Vaccinated", () => {
        const phn = "9735361219";
        const dobYear = "1994";
        const dobMonth = "June";
        const dobDay = "9";
        const dovYear = "2021";
        const dovMonth = "January";
        const dovDay = "20";

        cy.enableModules([
            "Immunization",
            vaccinationStatusModule,
            "VaccinationStatusPdf",
        ]);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(phn);

        select(dobYearSelector, dobYear);
        select(dobMonthSelector, dobMonth);
        select(dobDaySelector, dobDay);
        select(dovYearSelector, dovYear);
        select(dovMonthSelector, dovMonth);
        select(dovDaySelector, dovDay);

        clickVaccineCardEnterButton();

        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");
        cy.get("[data-testid=statusVaccinated]").should("be.visible");
    });
});
