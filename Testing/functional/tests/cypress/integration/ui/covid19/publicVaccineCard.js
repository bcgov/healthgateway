const { monthNames } = require("../../../support/constants");

const dobField = "[data-testid=dateOfBirthInput]";
const dobInput = `${dobField} input`;
const dovField = "[data-testid=dateOfVaccineInput]";
const dovInput = `${dovField} input`;

const phnField = "[data-testid=phnInput]";
const phnInput = `${phnField} input`;

const vaccineCardUrl = "/vaccinecard";

const dummyYear = "2021";
const dummyMonth = "June";
const dummyDay = "15";

const fullyVaccinatedPhn = "9735361219";
const fullyVaccinatedDob = "1994-JUN-09";
const fullyVaccinatedDov = "2021-JAN-20";

function enterVaccineCardPHN(phn) {
    cy.get(phnInput).should("be.visible", "be.enabled").clear().type(phn);
}

function clickVaccineCardEnterButton() {
    cy.get("[data-testid=btnEnter]")
        .should("be.enabled", "be.visible")
        .click({ force: true });
}

function populateDatePicker(selector, dateValue) {
    const date = new Date(dateValue);
    const year = date.getFullYear();
    const month = monthNames[date.getMonth()].substring(0, 3).toUpperCase();
    const day = date.getDate() < 10 ? `0${date.getDate()}` : date.getDate();

    cy.get(selector).clear().type(`${year}-${month}-${day}`).blur();
}

describe("Public Vaccine Card Form", () => {
    beforeEach(() => {
        cy.configureSettings({
            covid19: { publicCovid19: { vaccineCardEnabled: true } },
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

        cy.get(phnField).should("have.class", "v-input--error");
        cy.get(dobField).should("have.class", "v-input--error");
        cy.get(dovField).should("have.class", "v-input--error");
    });

    it("Validate PHN Format", () => {
        cy.log("Validate valid PHN passes validation.");
        enterVaccineCardPHN(Cypress.env("phn"));

        cy.get(phnInput).blur();

        cy.get(phnField)
            .should("be.visible", "be.enabled")
            .and("not.have.class", "v-input--error");

        cy.log("Validate invalid PHN fails validation.");
        enterVaccineCardPHN(Cypress.env("phn").replace(/.$/, ""));

        clickVaccineCardEnterButton();

        cy.get(phnField).and("have.class", "v-input--error");
    });

    it("Validate Date Range for DOB and DOV", () => {
        const d = new Date();
        const futureD = new Date().setDate(d.getDate() + 1);

        cy.log("Testing Future dates fail.");
        populateDatePicker(dobInput, futureD);
        populateDatePicker(dovInput, futureD);
        cy.get(dobField).should("have.class", "v-input--error");
        cy.get(dovField).should("have.class", "v-input--error");

        // Test Current date
        populateDatePicker(dobInput, d);
        populateDatePicker(dovInput, d);
        cy.get(dobField).should("not.have.class", "v-input--error");
        cy.get(dovField).should("not.have.class", "v-input--error");
    });
});

describe("Public Vaccine Card Downloads", () => {
    beforeEach(() => {
        cy.configureSettings({
            covid19: {
                publicCovid19: {
                    vaccineCardEnabled: true,
                    showFederalProofOfVaccination: true,
                },
            },
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
        cy.get(dobInput).clear().type(fullyVaccinatedDob);
        cy.get(dovInput).clear().type(fullyVaccinatedDov);

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
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.get("[data-testid=errorTextDescription]").should("be.visible");
        cy.get("[data-testid=loadingSpinner]").should("not.exist");
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

        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=loadingSpinner]").should("not.exist");

        cy.verifyDownload("VaccineProof.pdf");
    });

    it("Save Image", () => {
        cy.get("[data-testid=save-dropdown-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=save-as-image-dropdown-item]")
            .should("be.visible")
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.get("[data-testid=loadingSpinner]").should("not.exist");
        cy.verifyDownload("ProvincialVaccineProof.png");
    });
});

describe("Public Vaccine Card Downloads When showFederalProofOfVaccination Disabled", () => {
    it("Save Image When showFederalProofOfVaccination Disabled", () => {
        cy.configureSettings({
            covid19: {
                publicCovid19: {
                    vaccineCardEnabled: true,
                },
            },
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
        populateDatePicker(dobInput, fullyVaccinatedDob);
        populateDatePicker(dovInput, fullyVaccinatedDov);

        clickVaccineCardEnterButton();

        cy.get("[data-testid=save-a-copy-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.get("[data-testid=loadingSpinner]").should("not.exist");
        cy.verifyDownload("ProvincialVaccineProof.png");
    });
});
