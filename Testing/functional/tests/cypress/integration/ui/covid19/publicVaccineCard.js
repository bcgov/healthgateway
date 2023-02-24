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

function clickVaccineCardEnterButton() {
    cy.get("[data-testid=btnEnter]").should("be.enabled", "be.visible").click();
}

describe("Public Vaccine Card Form", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [{ name: "immunization", enabled: true }],
        });
        cy.logout();
        cy.visit(vaccineCardUrl);
    });

    it("Cancel Button Should Go to Landing Page", () => {
        cy.get("[data-testid=btnCancel]").should("be.visible").click();
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

        cy.get(dobYearSelector).select("Year");

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
        cy.get(dobYearSelector).shouldNotContainValue(nextYear.toString());
        cy.get(dovYearSelector).shouldNotContainValue(nextYear.toString());

        // Test Current Year
        cy.get(dobYearSelector).select(year.toString());
        cy.get(dovYearSelector).select(year.toString());
        cy.log("Testing Current Year.");

        if (nextMonth > 1) {
            cy.log("Next Month is: " + nextMonth);
            cy.log(
                "Current year has been set in dropdown, so if next month is 1 - January, it means current month is December. Test Future Month does not exist. Month can only be current or past month for current year."
            );
            cy.get(dobMonthSelector).shouldNotContainValue(
                nextMonth.toString()
            );
            cy.get(dovMonthSelector).shouldNotContainValue(
                nextMonth.toString()
            );
        }

        cy.log("Test and set Current Month");
        cy.get(dobMonthSelector).select(monthNames[monthNumber]);
        cy.get(dovMonthSelector).select(monthNames[monthNumber]);

        if (nextDay > 1) {
            cy.log("Next Day: " + nextDay);
            cy.log(
                "Current Year and Month have been set. If next day is 1, it means previous day was last day of current month. Next Day is associated with the current month. Test Future Day in current month does not exist."
            );
            cy.get(dobDaySelector).shouldNotContainValue(nextDay.toString());
            cy.get(dovDaySelector).shouldNotContainValue(nextDay.toString());
        }
        //Test Current Day exists
        cy.log("Test Current Day exists.");
        cy.get(dobDaySelector).shouldContainValue(day.toString());
        cy.get(dovDaySelector).shouldContainValue(day.toString());
    });

    it("Validate DOB Year Required", () => {
        cy.get(dobMonthSelector).select(dummyMonth);
        cy.get(dobDaySelector).select(dummyDay);

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
    });

    it("Validate DOB Month Required", () => {
        cy.get(dobYearSelector).select(dummyYear);
        cy.get(dobDaySelector).select(dummyDay);

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
    });

    it("Validate DOB Day Required", () => {
        cy.get(dobYearSelector).select(dummyYear);
        cy.get(dobMonthSelector).select(dummyMonth);

        clickVaccineCardEnterButton();

        cy.get(feedbackDobIsRequiredSelector).should("be.visible");
    });

    it("Validate DOV Year Required", () => {
        cy.get(dovMonthSelector).select(dummyMonth);
        cy.get(dovDaySelector).select(dummyDay);

        clickVaccineCardEnterButton();

        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Validate DOV Month Required", () => {
        cy.get(dovYearSelector).select(dummyYear);
        cy.get(dovDaySelector).select(dummyDay);

        clickVaccineCardEnterButton();

        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });

    it("Validate DOV Day Required", () => {
        cy.get(dovYearSelector).select(dummyYear);
        cy.get(dovMonthSelector).select(dummyMonth);

        clickVaccineCardEnterButton();

        cy.get(feedbackDovIsRequiredSelector).should("be.visible");
    });
});

describe("Public Vaccine Card Downloads", () => {
    beforeEach(() => {
        cy.configureSettings(
            {
                covid19: {
                    publicCovid19: {
                        showFederalProofOfVaccination: true,
                    },
                },
            },
            ["Immunization"]
        );
        cy.logout();
        cy.intercept("GET", "**/PublicVaccineStatus", {
            fixture: "ImmunizationService/publicVaccineStatusLoaded.json",
        });
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(fullyVaccinatedPhn);
        cy.get(dobYearSelector).select(fullyVaccinatedDobYear);
        cy.get(dobMonthSelector).select(fullyVaccinatedDobMonth);
        cy.get(dobDaySelector).select(fullyVaccinatedDobDay);
        cy.get(dovYearSelector).select(fullyVaccinatedDovYear);
        cy.get(dovMonthSelector).select(fullyVaccinatedDovMonth);
        cy.get(dovDaySelector).select(fullyVaccinatedDovDay);

        clickVaccineCardEnterButton();
    });

    it("Error Displayed When PDF Unavailable", () => {
        cy.intercept("GET", "**/PublicVaccineStatus/pdf", {
            fixture: "ImmunizationService/vaccineProofLoadedNoPdf.json",
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
        cy.intercept("GET", "**/PublicVaccineStatus/pdf", (req) => {
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
        cy.configureSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.logout();
        cy.intercept("GET", "**/PublicVaccineStatus", {
            fixture: "ImmunizationService/publicVaccineStatusLoaded.json",
        });
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(fullyVaccinatedPhn);
        cy.get(dobYearSelector).select(fullyVaccinatedDobYear);
        cy.get(dobMonthSelector).select(fullyVaccinatedDobMonth);
        cy.get(dobDaySelector).select(fullyVaccinatedDobDay);
        cy.get(dovYearSelector).select(fullyVaccinatedDovYear);
        cy.get(dovMonthSelector).select(fullyVaccinatedDovMonth);
        cy.get(dovDaySelector).select(fullyVaccinatedDovDay);

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
