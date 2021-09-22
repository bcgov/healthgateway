const { AuthMethod, localDevUri } = require("../../../support/constants");

const monthNames = [
    "January",
    "February",
    "March",
    "April",
    "May",
    "June",
    "July",
    "August",
    "September",
    "October",
    "November",
    "December",
];

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

const homeUrl = "/";
const vaccineCardUrl = "/vaccinecard";

const vaccinationStatusModule = "VaccinationStatus";

function enterVaccineCardPHN(phn) {
    cy.get("[data-testid=phnInput]")
        .should("be.visible", "be.enabled")
        .type(phn);
}

function select(selector, value) {
    cy.get(selector).should("be.visible", "be.enabled").select(value);
}

function selectNotExist(selector, value) {
    cy.get(selector).contains(value).should("not.exist");
}

function clickVaccineCardEnterButton() {
    cy.get("[data-testid=btnEnter]").should("be.enabled", "be.visible").click();
}

describe("Vaccine Card Page", () => {
    it("Landing Page - Vaccine Card Button does not exist - Vaccine Status module disabled", () => {
        cy.enableModules({});
        cy.visit(homeUrl);
        cy.get("[data-testid=btnVaccineCard]").should("not.exist");
    });

    it("Landing Page - Log In - Vaccine Status module disabled and vaccine card URL entered directly", () => {
        cy.enableModules({});
        cy.visit(vaccineCardUrl);

        cy.get("[data-testid=BCSCBtn]").should("be.visible", "be.enabled");
        cy.get("[data-testid=IDIRBtn]").should("be.visible", "be.enabled");
        cy.get("[data-testid=KeyCloakBtn]").should("be.visible", "be.enabled");
    });

    it("Landing Page - Vaccine Card Button Exists - Vaccine Status module enabled", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(homeUrl);
        cy.get("[data-testid=btnVaccineCard]").should("exist");
    });

    it("Landing Page - Vaccination Card - unauthenticated user", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        cy.get("[data-testid=vaccineCardFormTitle]").should("be.visible");
        cy.get("[data-testid=btnCancel]").should("be.visible");
        cy.get("[data-testid=btnEnter]").should("be.enabled", "be.visible");
        cy.get("[data-testid=btnPrivacyStatement]").should("be.visible");
        cy.get("[data-testid=btnLogin]").should("be.enabled", "be.visible");
    });

    it("Vaccination Card - Cancel - unauthenticated user", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        cy.get("[data-testid=btnCancel]").should("be.visible").click();

        cy.get("[data-testid=btnVaccineCard]").should("exist");
    });

    it("Vaccination Card - Login BC Services Card App - unauthenticated user", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        cy.get("[data-testid=btnLogin]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=BCSCBtn]").should("be.visible", "be.enabled");
        cy.get("[data-testid=IDIRBtn]").should("be.visible", "be.enabled");
        cy.get("[data-testid=KeyCloakBtn]").should("be.visible", "be.enabled");
    });

    it("Vaccination Card - Valid PHN via Select Year - unauthenticated user", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn"));

        select(dobYearSelector, "Year");

        cy.get("[data-testid=phnInput]")
            .should("be.visible", "be.enabled")
            .and("have.class", "form-control is-valid");
    });

    it("Vaccination Card - Invalid PHN via Select Year - unauthenticated user", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn").replace(/.$/, ""));

        select(dobYearSelector, "Year");

        cy.get(feedbackPhnMustBeValidSelector).should("be.visible");
    });

    it("Vaccination Card - PHN, DOB and DOV not entered via Click Enter - unauthenticated user", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        clickVaccineCardEnterButton();

        cy.get(feedbackPhnIsRequiredSelector).should("be.visible");
        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Vaccination Card - DOB and DOV not entered with Invalid PHN via Click Enter - unauthenticated user", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn").replace(/.$/, ""));

        clickVaccineCardEnterButton();

        cy.get(feedbackPhnMustBeValidSelector).should("be.visible");
        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Vaccination Card - DOB and DOV not entered via Click Enter - unauthenticated user", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn"));

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Vaccination Card - DOB and DOV cannot be future dated - unauthenticated user", () => {
        const d = new Date();
        const year = d.getFullYear();
        let monthNumber = d.getMonth();
        let month = monthNames[monthNumber];

        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(Cypress.env("phn"));

        // Test Future Year
        selectNotExist(dobYearSelector, (year + 1).toString());
        selectNotExist(dovYearSelector, (year + 1).toString());

        // Test Current Year
        select(dobYearSelector, year.toString());
        select(dovYearSelector, year.toString());

        // Test Current Month and Day
        select(dobMonthSelector, month);
        select(dovMonthSelector, month);
        select(dobDaySelector, d.getDate().toString());
        select(dovDaySelector, d.getDate().toString());

        // Test Future Date
        d.setDate(d.getDate() + 1);
        monthNumber = d.getMonth();
        month = monthNames[monthNumber === 11 ? 0 : monthNumber + 1];

        if (month > 0) {
            // Only execute if greater than 0. If 0, it means you've gone to January in new year.
            selectNotExist(dobMonthSelector, month);
            selectNotExist(dovMonthSelector, month);
        }

        selectNotExist(dobDaySelector, d.getDate().toString());
        selectNotExist(dovDaySelector, d.getDate().toString());
    });

    it("Vaccination Card - DOB Year and DOV not entered via Click Enter - unauthenticated user", () => {
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

    it("Vaccination Card - DOB Month and DOV not entered via Click Enter - unauthenticated user", () => {
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

    it("Vaccination Card - DOB Day and DOV not entered via Click Enter - unauthenticated user", () => {
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

    it("Vaccination Card - DOV Year and DOB not entered via Click Enter - unauthenticated user", () => {
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

    it("Vaccination Card - DOV Month and DOB not entered via Click Enter - unauthenticated user", () => {
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

    it("Vaccination Card - DOV Day and DOB not entered via Click Enter - unauthenticated user", () => {
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

    it("Vaccination Card - Not Found - unauthenticated user", () => {
        const phn = "9735352528";
        const dobYear = "1988";
        const dobMonth = "December";
        const dobDay = "20";
        const dovYear = "2021";
        const dovMonth = "February";
        const dovDay = "11";

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
        cy.get("[data-testid=statusNotFound]").should("be.visible");
    });

    it("Vaccination Card - Partially Vaccinated 1 Dose - unauthenticated user", () => {
        const phn = "9735361219";
        const dobYear = "1994";
        const dobMonth = "June";
        const dobDay = "9";
        const dovYear = "2021";
        const dovMonth = "January";
        const dovDay = "6";

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
        cy.get("[data-testid=dose-1]").should("be.visible");
    });

    it("Vaccination Card - Partially Vaccinated 2 Dose - unauthenticated user", () => {
        const phn = "9735352503";
        const dobYear = "1964";
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
        cy.get("[data-testid=statusPartiallyVaccinated]").should("be.visible");
        cy.get("[data-testid=dose-1]").should("be.visible");
        cy.get("[data-testid=dose-2]").scrollIntoView().should("be.visible");
    });

    it("Vaccination Card - Fully Vaccinated - unauthenticated user", () => {
        const phn = "9735353315";
        const dobYear = "1967";
        const dobMonth = "June";
        const dobDay = "2";
        const dovYear = "2021";
        const dovMonth = "January";
        const dovDay = "20";

        cy.enableModules(vaccinationStatusModule);
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

    it("Landing Page - Vaccination Card - Registered Keycloak authenticated user", () => {
        cy.enableModules([
            "Immunization",
            vaccinationStatusModule,
            "VaccinationStatusPdf",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );

        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");
        cy.get("[data-testid=statusPartiallyVaccinated]").should("be.visible");
        cy.get("[data-testid=dose-1]").should("be.visible");
        cy.get("[data-testid=dose-2]").scrollIntoView().should("be.visible");
    });

    it("Landing Page - Vaccination Card - Unregistered Keycloak authenticated user", () => {
        cy.enableModules(["Immunization", vaccinationStatusModule]);
        cy.login(
            Cypress.env("keycloak.unregistered.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );

        cy.get("[data-testid=noCovidImmunizationsText]")
            .should("be.visible")
            .contains("No records found");
    });
});
