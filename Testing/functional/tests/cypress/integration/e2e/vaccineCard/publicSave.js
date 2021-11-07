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

function clickVaccineCardEnterButton() {
    cy.get("[data-testid=btnEnter]").should("be.enabled", "be.visible").click();
}

function select(selector, value) {
    cy.get(selector).should("be.visible", "be.enabled").select(value);
}

describe("Public User - Vaccine Card Page - Save PDF but Vaccine Record not found", () => {
    beforeEach(() => {
        deleteDownloadsFolder();
        cy.intercept("GET", "**/v1/api/PublicVaccineStatus/pdf", (req) => {
            req.reply({
                fixture: "ImmunizationService/vaccineProofLoadedNoPdf.json",
            });
        });
    });
    it("Save Dropdown List - Save PDF - Download confirmed With Retry", () => {
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
        cy.get("[data-testid=errorTextDescription]").should("be.visible");
    });
});

describe("Public User - Vaccine Card Page - Save PDF", () => {
    beforeEach(() => {
        deleteDownloadsFolder();
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
    });
    it("Save Dropdown List - Save PDF - Download confirmed With Retry", () => {
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
        cy.wait(10000);
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");
        cy.verifyDownload("VaccineProof.pdf");
    });
});

describe("Public User - Vaccine Card Page - Save Image", () => {
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
        cy.verifyDownload("ProvincialVaccineProof.png");
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
        cy.verifyDownload("ProvincialVaccineProof.png");
    });
});
