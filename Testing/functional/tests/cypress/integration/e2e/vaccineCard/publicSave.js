import { deleteDownloadsFolder } from "../../../support/utils";

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

const vaccinationStatusModule = "VaccinationStatus";
const vaccineCardUrl = "/vaccinecard";

function enterVaccineCardPHN(phn) {
    cy.get("[data-testid=phnInput]")
        .should("be.visible", "be.enabled")
        .type(phn);
}

function interceptVaccineStatus() {
    cy.intercept("GET", "**/v1/api/VaccineStatus/pdf", {
        fixture: "ImmunizationService/vaccineProof.json",
    });
}

function clickVaccineCardEnterButton() {
    cy.get("[data-testid=btnEnter]").should("be.enabled", "be.visible").click();
}

function select(selector, value) {
    cy.get(selector).should("be.visible", "be.enabled").select(value);
}

describe("Public User - Vaccine Card Page - Save", () => {
    beforeEach(() => {
        deleteDownloadsFolder();
    });

    it("Save Copy - Spinner and download confirmed", () => {
        const phn = "9735361219 ";
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

        cy.get("[data-testid=save-a-copy-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("BCVaccineCard.png");
    });

    it("Save Dropdown List - Save Image - Spinner and Download confirmed", () => {
        const phn = "9735361219 ";
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
            "PublicVaccineDownloadPdf",
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

        cy.get("[data-testid=save-dropdown-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=save-as-image-dropdown-item]")
            .should("be.visible")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("BCVaccineCard.png");
    });

    it("Save Dropdown List - Save PDF - Spinner displayed", () => {
        const phn = "9735361219 ";
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
            "PublicVaccineDownloadPdf",
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

        cy.get("[data-testid=save-dropdown-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
    });

    it("Save Dropdown List - Save PDF - Download confirmed", () => {
        const phn = "9735361219 ";
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
            "PublicVaccineDownloadPdf",
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

        cy.get("[data-testid=save-dropdown-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        interceptVaccineStatus();
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.verifyDownload("VaccineProof.pdf");
    });
});
