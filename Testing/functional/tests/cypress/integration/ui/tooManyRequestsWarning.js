const { AuthMethod } = require("../../support/constants");

const vaccineCardUrl = "/vaccinecard";
const covidTestUrl = "/covidtest";

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
const collectionDateYearSelector =
    "[data-testid=dateOfCollectionInput] [data-testid=formSelectYear]";
const collectionDateMonthSelector =
    "[data-testid=dateOfCollectionInput] [data-testid=formSelectMonth]";
const collectionDateDaySelector =
    "[data-testid=dateOfCollectionInput] [data-testid=formSelectDay]";

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

function enterCovidTestPHN(phn) {
    cy.get("[data-testid=phnInput]")
        .should("be.visible", "be.enabled")
        .clear()
        .type(phn);
}

function clickCovidTestEnterButton() {
    cy.get("[data-testid=btnEnter]").should("be.enabled", "be.visible").click();
}

describe("Authenticated Vaccine Card Downloads", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus?hdid=*", {
            statusCode: 429,
        });
        cy.enableModules(["Immunization", "VaccinationStatus"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");
        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });

    it("Save As PDF Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus/pdf?hdid=*", {
            statusCode: 429,
        });
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
            "VaccinationExportPdf",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle")
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Public COVID-19 Test Results", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        const phn = "9735361219 ";
        const dobYear = "1994";
        const dobMonth = "June";
        const dobDay = "9";
        const collectionDateYear = "2021";
        const collectionDateMonth = "January";
        const collectionDateDay = "20";

        cy.enableModules(["Laboratory", "PublicLaboratoryResult"]);
        cy.logout();
        cy.visit(covidTestUrl);

        cy.intercept("GET", "**/PublicLaboratory/CovidTests", {
            statusCode: 429,
        });

        enterCovidTestPHN(phn);
        cy.get(dobYearSelector).select(dobYear);
        cy.get(dobMonthSelector).select(dobMonth);
        cy.get(dobDaySelector).select(dobDay);
        cy.get(collectionDateYearSelector).select(collectionDateYear);
        cy.get(collectionDateMonthSelector).select(collectionDateMonth);
        cy.get(collectionDateDaySelector).select(collectionDateDay);
        clickCovidTestEnterButton();
        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Public Vaccine Card Form", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.enableModules(["Immunization", "VaccinationStatus"]);
        cy.logout();
        cy.visit(vaccineCardUrl);

        cy.intercept("GET", "**/PublicVaccineStatus", {
            statusCode: 429,
        });

        enterVaccineCardPHN(fullyVaccinatedPhn);
        cy.get(dobYearSelector).select(fullyVaccinatedDobYear);
        cy.get(dobMonthSelector).select(fullyVaccinatedDobMonth);
        cy.get(dobDaySelector).select(fullyVaccinatedDobDay);
        cy.get(dovYearSelector).select(fullyVaccinatedDovYear);
        cy.get(dovMonthSelector).select(fullyVaccinatedDovMonth);
        cy.get(dovDaySelector).select(fullyVaccinatedDovDay);
        clickVaccineCardEnterButton();

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Public Vaccine Card Downloads", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
            "PublicVaccineDownloadPdf",
        ]);
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

        cy.intercept("GET", "**/PublicVaccineStatus/pdf", {
            statusCode: 429,
        });
        cy.get("[data-testid=save-dropdown-btn]")
            .should("be.visible", "be.enabled")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");
    });
});

describe("Landing Page - Too Many Requests", () => {
    beforeEach(() => {
        cy.logout();
    });

    it("Too Many Requests Banner Appears on 429 Response", () => {
        cy.intercept("GET", "**/Communication/*", { statusCode: 429 });
        cy.visit("/");

        cy.contains(
            "[data-testid=communicationBanner]",
            "higher than usual site traffic"
        ).should("be.visible");
    });

    it("Too Many Requests Banner Doesn't Appear on 200 Response", () => {
        cy.intercept("GET", "**/Communication/*", { statusCode: 200 }).as(
            "getCommunication"
        );
        cy.visit("/");

        // wait for both Communication calls to complete
        cy.wait("@getCommunication");
        cy.wait("@getCommunication");

        cy.contains(
            "[data-testid=communicationBanner]",
            "higher than usual site traffic"
        ).should("not.exist");
    });
});

describe("Immunization", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/Immunization?*", {
            statusCode: 429,
        });
        cy.enableModules("Immunization");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Medication Request", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.enableModules("MedicationRequest");
        cy.intercept("GET", "**/MedicationRequest/*", {
            fixture: "MedicationService/medicationRequest.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.intercept("GET", "**/MedicationRequest/*", {
            statusCode: 429,
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Mobile - COVID-19 Orders", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
            fixture: "LaboratoryService/covid19Orders.json",
        });
        cy.enableModules("Laboratory");
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
            statusCode: 429,
        });
        cy.enableModules("Laboratory");
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Mobile - Immunization: Unsuccessful Response", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/Immunization?*", {
            statusCode: 429,
        });
        cy.enableModules("Immunization");
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Mobile - Laboratory Orders", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrders.json",
        });
        cy.enableModules("AllLaboratory");
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            statusCode: 429,
        });
        cy.enableModules("AllLaboratory");
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});
