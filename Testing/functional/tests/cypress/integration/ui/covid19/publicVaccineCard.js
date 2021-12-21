import { deleteDownloadsFolder } from "../../../support/utils";

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

const dummyYear = "2021";
const dummyMonth = "June";
const dummyDay = "15";

const fullyVaccinatedPhn = "9735361219";
const fullyVaccinatedDobYear = "1994";
const fullyVaccinatedDobMonth = "June";
const fullyVaccinatedDobDay = "9";
const fullyVaccinatedDovYear = "2021";
const fullyVaccinatedDovMonth = "January";
const fullyVaccinatedDovDay = "20";

function enterVaccineCardPHN(phn) {
    cy.get("[data-testid=phnInput]")
        .should("be.visible", "be.enabled")
        .clear()
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

describe("Public Vaccine Card Form", () => {
    beforeEach(() => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);
    });

    it("Cancel Button Should Go to Landing Page", () => {
        cy.get("[data-testid=btnCancel]").should("be.visible").click();

        cy.get("[data-testid=btnVaccineCard]").should("exist");
    });

    it("Log In Button Should Go to Login Page", () => {
        cy.get("[data-testid=btnLogin]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=vaccineCardFormTitle]").should("not.exist");
    });

    it("Validate PHN, DOB, and DOV Required", () => {
        clickVaccineCardEnterButton();

        cy.get(feedbackPhnIsRequiredSelector).should("be.visible");
        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Validate PHN Format", () => {
        cy.log("Validate valid PHN passes validation.");
        enterVaccineCardPHN(Cypress.env("phn"));

        select(dobYearSelector, "Year");

        cy.get("[data-testid=phnInput]")
            .should("be.visible", "be.enabled")
            .and("have.class", "form-control is-valid");

        cy.log("Validate invalid PHN fails validation.");
        enterVaccineCardPHN(Cypress.env("phn").replace(/.$/, ""));

        clickVaccineCardEnterButton();

        cy.get(feedbackPhnMustBeValidSelector).should("be.visible");
    });

    it("Validate Date Range for DOB and DOV", () => {
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

    it("Validate DOB Year Required", () => {
        select(dobMonthSelector, dummyMonth);
        select(dobDaySelector, dummyDay);

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
    });

    it("Validate DOB Month Required", () => {
        select(dobYearSelector, dummyYear);
        select(dobDaySelector, dummyDay);

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
    });

    it("Validate DOB Day Required", () => {
        select(dobYearSelector, dummyYear);
        select(dobMonthSelector, dummyMonth);

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
    });

    it("Validate DOV Year Required", () => {
        select(dovMonthSelector, dummyMonth);
        select(dovDaySelector, dummyDay);

        clickVaccineCardEnterButton();

        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Validate DOV Month Required", () => {
        select(dovYearSelector, dummyYear);
        select(dovDaySelector, dummyDay);

        clickVaccineCardEnterButton();

        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Validate DOV Day Required", () => {
        select(dovYearSelector, dummyYear);
        select(dovMonthSelector, dummyMonth);

        clickVaccineCardEnterButton();

        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });
});

describe("Public Vaccine Card Downloads", () => {
    beforeEach(() => {
        deleteDownloadsFolder();
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
            "PublicVaccineDownloadPdf",
        ]);
        cy.intercept("GET", "**/v1/api/PublicVaccineStatus", {
            fixture: "ImmunizationService/publicVaccineStatusLoaded.json",
        });
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(fullyVaccinatedPhn);
        select(dobYearSelector, fullyVaccinatedDobYear);
        select(dobMonthSelector, fullyVaccinatedDobMonth);
        select(dobDaySelector, fullyVaccinatedDobDay);
        select(dovYearSelector, fullyVaccinatedDovYear);
        select(dovMonthSelector, fullyVaccinatedDovMonth);
        select(dovDaySelector, fullyVaccinatedDovDay);

        clickVaccineCardEnterButton();
    });

    it("Error Displayed When PDF Unavailable", () => {
        cy.intercept("GET", "**/v1/api/PublicVaccineStatus/pdf", (req) => {
            req.reply({
                fixture: "ImmunizationService/vaccineProofLoadedNoPdf.json",
            });
        });
        cy.get("[data-testid=save-dropdown-btn]")
            .should("be.visible", "be.enabled")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=errorTextDescription]").should("be.visible");
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");
    });

    it("Save PDF with Retry", () => {
        let isLoading = false;
        cy.intercept("GET", "**/v1/api/PublicVaccineStatus/pdf", (req) => {
            if (!isLoading) {
                req.reply({
                    fixture: "ImmunizationService/vaccineProofNotLoaded.json",
                });
            } else {
                req.reply({
                    fixture: "ImmunizationService/vaccineProofLoaded.json",
                });
            }
            isLoading = !isLoading;
        });

        cy.get("[data-testid=save-dropdown-btn]")
            .should("be.visible", "be.enabled")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.wait(1000);
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");

        cy.verifyDownload("VaccineProof.pdf");
    });

    it("Save Image", () => {
        cy.get("[data-testid=save-dropdown-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=save-as-image-dropdown-item]")
            .should("be.visible")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("ProvincialVaccineProof.png");
    });
});

describe("Public Vaccine Card Downloads When PublicVaccineDownloadPdf Disabled", () => {
    it("Save Image When PublicVaccineDownloadPdf Disabled", () => {
        deleteDownloadsFolder();
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
        ]);
        cy.intercept("GET", "**/v1/api/PublicVaccineStatus", {
            fixture: "ImmunizationService/publicVaccineStatusLoaded.json",
        });
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(fullyVaccinatedPhn);
        select(dobYearSelector, fullyVaccinatedDobYear);
        select(dobMonthSelector, fullyVaccinatedDobMonth);
        select(dobDaySelector, fullyVaccinatedDobDay);
        select(dovYearSelector, fullyVaccinatedDovYear);
        select(dovMonthSelector, fullyVaccinatedDovMonth);
        select(dovDaySelector, fullyVaccinatedDovDay);

        clickVaccineCardEnterButton();

        cy.get("[data-testid=save-a-copy-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("ProvincialVaccineProof.png");
    });
});
