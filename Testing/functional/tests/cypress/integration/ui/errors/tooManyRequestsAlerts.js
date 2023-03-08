const { AuthMethod } = require("../../../support/constants");

const vaccineCardUrl = "/vaccinecard";
const covidTestUrl = "/covidtest";
const dependentHdid = "645645767756756767";
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

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

describe("Authenticated Vaccine Card Downloads", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus?hdid=*", {
            statusCode: 429,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });

    it("Save As PDF Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus/pdf?hdid=*", {
            statusCode: 429,
        });
        cy.configureSettings({
            covid19: {
                proofOfVaccination: {
                    exportPdf: true,
                },
            },
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle")
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Public COVID-19 Test Results", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/PublicLaboratory/CovidTests", {
            statusCode: 429,
        });
        cy.configureSettings({
            covid19: {
                publicCovid19: {
                    enableTestResults: true,
                },
            },
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.logout();
        cy.visit(covidTestUrl);

        enterVaccineCardPHN(fullyVaccinatedPhn);
        cy.get(dobYearSelector).select(fullyVaccinatedDobYear);
        cy.get(dobMonthSelector).select(fullyVaccinatedDobMonth);
        cy.get(dobDaySelector).select(fullyVaccinatedDobDay);
        cy.get(collectionDateYearSelector).select(fullyVaccinatedDovYear);
        cy.get(collectionDateMonthSelector).select(fullyVaccinatedDovMonth);
        cy.get(collectionDateDaySelector).select(fullyVaccinatedDovDay);
        clickVaccineCardEnterButton();

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Public Vaccine Card Form", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/PublicVaccineStatus", {
            statusCode: 429,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.logout();
        cy.visit(vaccineCardUrl);

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
        cy.intercept("GET", "**/PublicVaccineStatus", {
            fixture: "ImmunizationService/publicVaccineStatusLoaded.json",
        });
        cy.intercept("GET", "**/PublicVaccineStatus/pdf", {
            statusCode: 429,
        });
        cy.configureSettings({
            covid19: {
                publicCovid19: {
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
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(fullyVaccinatedPhn);
        cy.get(dobYearSelector).select(fullyVaccinatedDobYear);
        cy.get(dobMonthSelector).select(fullyVaccinatedDobMonth);
        cy.get(dobDaySelector).select(fullyVaccinatedDobDay);
        cy.get(dovYearSelector).select(fullyVaccinatedDovYear);
        cy.get(dovMonthSelector).select(fullyVaccinatedDovMonth);
        cy.get(dovDaySelector).select(fullyVaccinatedDovDay);
        clickVaccineCardEnterButton();

        cy.get("[data-testid=save-dropdown-btn]")
            .should("be.visible", "be.enabled")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Landing Page - Too Many Requests", () => {
    it("Too Many Requests Banner Appears on 429 Response", () => {
        cy.configureSettings({});
        cy.intercept("GET", "**/Communication/*", { statusCode: 429 });
        cy.visit("/");

        cy.contains(
            "[data-testid=communicationBanner]",
            "higher than usual site traffic"
        ).should("be.visible");
    });

    it("Too Many Requests Banner Doesn't Appear on 200 Response", () => {
        cy.configureSettings({});
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
        cy.configureSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Encounter", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/Encounter/*", {
            statusCode: 429,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Hospital Visits", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/HospitalVisit/*", {
            statusCode: 429,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "hospitalVisit",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Medication Request", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/MedicationRequest/*", {
            statusCode: 429,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "specialAuthorityRequest",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Mobile - COVID-19 Orders", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
            statusCode: 429,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Mobile - Immunization: Unsuccessful Response", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/Immunization?*", {
            statusCode: 429,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Mobile - Encounter", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/Encounter/*", {
            statusCode: 429,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
            ],
        });
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Mobile - Hospital Visits", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/HospitalVisit/*", {
            statusCode: 429,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "hospitalVisit",
                    enabled: true,
                },
            ],
        });
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Mobile - Laboratory Orders", () => {
    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            statusCode: 429,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "labResult",
                    enabled: true,
                },
            ],
        });
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );

        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });
});

describe("Mobile - Laboratory Orders Report Download", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrders.json",
        });

        cy.configureSettings({
            datasets: [
                {
                    name: "labResult",
                    enabled: true,
                },
            ],
        });
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/Laboratory/*/Report*", {
            statusCode: 429,
        });

        cy.log(
            "Verifying Laboratory Report Download returns Too Many Requests Error"
        );
        cy.get("[data-testid=timelineCard]").last().scrollIntoView().click();

        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=laboratory-report-download-btn]")
                    .should("be.visible")
                    .click({ force: true });
            });

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]")
            .should("be.visible")
            .click({ force: true });

        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
        cy.get("[data-testid=backBtn]").click({ force: true });
    });

    it("Unsuccessful Response: Internal Server Error", () => {
        cy.intercept("GET", "**/Laboratory/*/Report*", {
            statusCode: 500,
        });

        cy.log(
            "Verifying Laboratory Report Download returns Internal Server Error"
        );
        cy.get("[data-testid=timelineCard]").last().scrollIntoView().click();

        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=laboratory-report-download-btn]")
                    .should("be.visible")
                    .click({ force: true });
            });

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]")
            .should("be.visible")
            .click({ force: true });

        cy.get("[data-testid=singleErrorHeader]").contains(
            "Unable to download laboratory report"
        );
        cy.get("[data-testid=backBtn]").click({ force: true });
    });
});

describe("Mobile - Covid19 Orders Report Download", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
            fixture: "LaboratoryService/covid19Orders.json",
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/Laboratory/*/Report*", {
            statusCode: 429,
        });

        cy.log(
            "Verifying Covid19 Orders Report Download returns Too Many Requests Error"
        );
        cy.get("[data-testid=timelineCard]").last().scrollIntoView().click();

        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=covid-result-download-btn]")
                    .should("be.visible")
                    .click({ force: true });
            });

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]")
            .should("be.visible")
            .click({ force: true });

        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
        cy.get("[data-testid=backBtn]").click({ force: true });
    });

    it("Unsuccessful Response: Internal Server Error", () => {
        cy.intercept("GET", "**/Laboratory/*/Report*", {
            statusCode: 500,
        });

        cy.log(
            "Verifying Covid19 Orders Report Download returns Internal Server Error"
        );
        cy.get("[data-testid=timelineCard]").last().scrollIntoView().click();

        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=covid-result-download-btn]")
                    .should("be.visible")
                    .click({ force: true });
            });

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]")
            .should("be.visible")
            .click({ force: true });

        cy.get("[data-testid=singleErrorHeader]").contains(
            "Unable to download COVID‑19 laboratory report"
        );
        cy.get("[data-testid=backBtn]").click({ force: true });
    });
});

describe("User Profile", () => {
    beforeEach(() => {
        cy.configureSettings({});
        cy.intercept("GET", `**/UserProfile/${HDID}`, {
            fixture: "UserProfileService/userProfile.json",
        });
        cy.intercept("PUT", `**/UserProfile/${HDID}/sms`, {
            statusCode: 200,
            body: true,
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );
    });

    it("Edit email address: Too Many Requests Error", () => {
        cy.intercept("PUT", `**/UserProfile/${HDID}/email`, {
            statusCode: 429,
        });

        cy.log("Edit email address");
        cy.get("[data-testid=editEmailBtn]").click();
        cy.get("[data-testid=emailInput]").type(Cypress.env("emailAddress"));
        cy.get("[data-testid=editEmailSaveBtn]").click();

        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });

    it("Verify SMS number: Too Many Requests Error", () => {
        cy.intercept("GET", `**/UserProfile/${HDID}/sms/validate/*`, {
            statusCode: 429,
        });
        cy.intercept("GET", `**/UserProfile/${HDID}`, {
            fixture: "UserProfileService/userProfile.json",
        });
        cy.get("[data-testid=verifySMSBtn]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get("[data-testid=verifySMSModalCodeInput]")
            .should("be.visible")
            .should("have.focus")
            .type("123456");

        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });
});

describe("Dependents", () => {
    const validDependent = {
        firstName: "Sam ", // Add end space to ensure field is trimmed
        lastName: "Testfive ", // Add end space to ensure field is trimmed
        wrongLastName: "Testfive2",
        invalidDoB: "2007-Aug-05",
        doB: "2014-Mar-15",
        testDate: "2020-Mar-21",
        phn: "9874307168",
        hdid: "645645767756756767",
    };

    beforeEach(() => {
        cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
            fixture: "LaboratoryService/covid19Orders.json",
        });
        cy.intercept("GET", "**/UserProfile/*/Dependent", {
            fixture: "UserProfileService/dependent.json",
        });
        cy.configureSettings({
            dependents: {
                enabled: true,
            },
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
    });

    it("Delete Dependent: Too Many Requests Error", () => {
        cy.intercept("DELETE", "**/UserProfile/*/Dependent/*", {
            statusCode: 429,
        });
        cy.get(`[data-testid=dependent-card-${validDependent.phn}]`).within(
            () => {
                cy.get("[data-testid=dependentMenuBtn]").click();
                cy.get("[data-testid=deleteDependentMenuBtn]").click();
            }
        );
        cy.get("[data-testid=confirmDeleteBtn]").click();

        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });

    it("Add Dependent: Too Many Requests Error", () => {
        cy.intercept("POST", "**/UserProfile/*/Dependent", {
            statusCode: 429,
        });
        cy.get("[data-testid=addNewDependentBtn]").click();

        cy.get("[data-testid=firstNameInput]")
            .clear()
            .type(validDependent.firstName);
        cy.get("[data-testid=lastNameInput]")
            .clear()
            .type(validDependent.lastName);
        cy.get("[data-testid=dateOfBirthInput] input")
            .clear()
            .type(validDependent.doB);
        cy.get("[data-testid=phnInput]").clear().type(validDependent.phn);
        cy.get("[data-testid=termsCheckbox]").check({ force: true });

        cy.get("[data-testid=registerDependentBtn]").click();

        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });
});

describe("Dependent - Immunizaation History Tab - report download error handling", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/UserProfile/*/Dependent", {
            fixture: "UserProfileService/dependent.json",
        });
        cy.intercept("GET", "**/Immunization?hdid=*", {
            fixture: "ImmunizationService/dependentImmunization.json",
        });
        cy.configureSettings({
            dependents: {
                enabled: true,
            },
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
    });

    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("POST", "**/Report", {
            statusCode: 429,
        });

        cy.get(`[data-testid=immunization-tab-title-${dependentHdid}]`)
            .parent()
            .click();

        // History tab
        cy.get(`[data-testid=immunization-tab-div-${dependentHdid}]`).within(
            () => {
                cy.contains("a", "History").click();
            }
        );
        cy.get(
            `[data-testid=immunization-history-table-${dependentHdid}]`
        ).should("be.visible");

        // Click download dropdown under History tab
        cy.log("Validating download history report button.");
        cy.get(
            `[data-testid=download-immunization-history-report-btn-${dependentHdid}]`
        ).click();

        // Click PDF
        cy.log("Selecting PDF as download report type.");
        cy.get(
            `[data-testid=download-immunization-history-report-pdf-btn-${dependentHdid}]`
        ).click();

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });

    it("Unsuccessful Response: Internal Server Error", () => {
        cy.intercept("POST", "**/Report", {
            statusCode: 500,
        });

        cy.get(`[data-testid=immunization-tab-title-${dependentHdid}]`)
            .parent()
            .click();

        // History tab
        cy.get(`[data-testid=immunization-tab-div-${dependentHdid}]`).within(
            () => {
                cy.contains("a", "History").click();
            }
        );
        cy.get(
            `[data-testid=immunization-history-table-${dependentHdid}]`
        ).should("be.visible");

        // Click download dropdown under History tab
        cy.log("Validating download history report button.");
        cy.get(
            `[data-testid=download-immunization-history-report-btn-${dependentHdid}]`
        ).click();

        // Click PDF
        cy.log("Selecting PDF as download report type.");
        cy.get(
            `[data-testid=download-immunization-history-report-pdf-btn-${dependentHdid}]`
        ).click();

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=singleErrorHeader]").contains(
            "Unable to download Dependent Immunization Report"
        );
    });
});

describe("Comments", () => {
    it("Add Comment: Too Many Requests Error", () => {
        cy.intercept("POST", "**/UserProfile/*/Comment", {
            statusCode: 429,
        });
        cy.configureSettings({
            timeline: {
                comment: true,
            },
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        var testComment = "Test Add Comment";

        cy.get("[data-testid=entryCardDetailsTitle]").first().click();

        // Add comment
        cy.get("[data-testid=addCommentTextArea]").first().type(testComment);
        cy.get("[data-testid=postCommentBtn]").first().click();

        // Verify
        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });
});

describe("Notes", () => {
    it("Add Note: Too Many Requests Error", () => {
        cy.intercept("POST", "**/Note/*", {
            statusCode: 429,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "note",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );

        cy.get("[data-testid=addNoteBtn]").click();
        cy.get("[data-testid=noteTitleInput]").type("Note Title!");
        cy.get("[data-testid=noteDateInput] input")
            .focus()
            .clear()
            .type("1950-Jan-01");
        cy.get("[data-testid=noteTextInput]").type("Test");
        cy.get("[data-testid=saveNoteBtn]").click();

        // Verify
        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });

    it("Edit Note: Too Many Requests Error", () => {
        cy.intercept("GET", "**/Note/*", {
            fixture: "NoteService/notes-test-note.json",
        });
        cy.intercept("PUT", "**/Note/*", {
            statusCode: 429,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "note",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );

        cy.log("Editing Note.");
        cy.get("[data-testid=noteMenuBtn]").first().click();
        cy.get("[data-testid=editNoteMenuBtn]").first().click();
        cy.get("[data-testid=noteTitleInput]").clear().type("Test Edit");
        cy.get("[data-testid=saveNoteBtn]").click();

        // Verify
        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });

    it("Delete Note: Too Many Requests Error", () => {
        cy.intercept("GET", "**/Note/*", {
            fixture: "NoteService/notes-test-note.json",
        });
        cy.intercept("DELETE", "**/Note/*", {
            statusCode: 429,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "note",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );

        cy.get("[data-testid=noteMenuBtn]").last().click();
        cy.get("[data-testid=deleteNoteMenuBtn]").last().click();

        // Verify
        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });

    describe("Export Records - Immunizaation - report download error handling", () => {
        beforeEach(() => {
            cy.intercept("GET", "**/Immunization?hdid=*", {
                fixture: "ImmunizationService/immunization.json",
            });
            cy.configureSettings({
                datasets: [
                    {
                        name: "immunization",
                        enabled: true,
                    },
                ],
            });
            cy.login(
                Cypress.env("keycloak.username"),
                Cypress.env("keycloak.password"),
                AuthMethod.KeyCloak,
                "/reports"
            );
        });

        it("Unsuccessful Response: Too Many Requests", () => {
            cy.intercept("POST", "**/Report", {
                statusCode: 429,
            });

            cy.get("[data-testid=reportType]").select("Immunizations");

            // Click download button
            cy.get("[data-testid=exportRecordBtn] button")
                .should("be.enabled", "be.visible")
                .click();

            // Select and click first option
            cy.get("[data-testid=exportRecordBtn] a").first().click();

            // Confirmation modal
            cy.get("[data-testid=genericMessageModal]").should("be.visible");
            cy.get("[data-testid=genericMessageSubmitBtn]").click();

            cy.get("[data-testid=too-many-requests-error]").should(
                "be.visible"
            );
        });

        it("Unsuccessful Response: Internal Server Error", () => {
            cy.intercept("POST", "**/Report", {
                statusCode: 500,
            });

            cy.get("[data-testid=reportType]").select("Immunizations");

            // Click download button
            cy.get("[data-testid=exportRecordBtn] button")
                .should("be.enabled", "be.visible")
                .click();

            // Select and click first option
            cy.get("[data-testid=exportRecordBtn] a").first().click();

            // Confirmation modal
            cy.get("[data-testid=genericMessageModal]").should("be.visible");
            cy.get("[data-testid=genericMessageSubmitBtn]").click();

            cy.get("[data-testid=singleErrorHeader]").contains(
                "Unable to download Export Records"
            );
        });
    });
});
