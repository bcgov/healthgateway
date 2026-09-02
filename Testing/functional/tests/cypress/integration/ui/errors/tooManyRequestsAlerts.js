import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const dependentHdid = "645645767756756767";
const dependentPhn = "9874307168";
const tooManyRequestsStatusCode = 429;

// Each entry exercises the store handling for a distinct timeline endpoint.
// The warning is a shared responsive component, so repeating these scenarios
// at a mobile viewport would repeat the same behavior and require extra logins.
const timelineTooManyRequestsScenarios = [
    ["Immunization", "**/Immunization?hdid*", "immunization"],
    ["MSP Visits", "**/Encounter/*", "healthVisit"],
    ["Hospital Visits", "**/HospitalVisit/*", "hospitalVisit"],
    [
        "Special Authority Requests",
        "**/MedicationRequest/*",
        "specialAuthorityRequest",
    ],
    ["COVID-19 Orders", "**/Laboratory/Covid19Orders*", "covid19TestResult"],
    ["Laboratory Orders", "**/Laboratory/LaboratoryOrders*", "labResult"],
];

function loginWithDatasetError(endpoint, dataset) {
    setupStandardFixtures();
    cy.intercept("GET", endpoint, { statusCode: tooManyRequestsStatusCode });
    cy.configureSettings({
        datasets: [{ name: dataset, enabled: true }],
    });
    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak
    );
}

function setupDependentsPage(datasets, setupDatasetFixtures) {
    setupStandardFixtures();
    cy.intercept("GET", "**/UserProfile/*/Dependent", {
        fixture: "UserProfileService/dependent.json",
    });
    setupDatasetFixtures?.();
    cy.configureSettings({
        dependents: {
            enabled: true,
        },
        datasets,
    });
    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak,
        "/dependents"
    );

    // UI specs use fixtures and skip endpoint waits. The rendered dependent
    // card is the authoritative signal that the page is ready for interaction.
    cy.get(`[data-testid=dependent-card-${dependentPhn}]`).should("be.visible");
}

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
        cy.intercept("GET", "**/Communication/*", { statusCode: 200 });
        cy.visit("/");

        cy.contains(
            "[data-testid=communicationBanner]",
            "higher than usual site traffic"
        ).should("not.exist");
    });
});

describe("Timeline - Too Many Requests", () => {
    timelineTooManyRequestsScenarios.forEach(([name, endpoint, dataset]) => {
        it(`Displays the warning when ${name} returns 429`, () => {
            loginWithDatasetError(endpoint, dataset);

            cy.get("[data-testid=too-many-requests-warning]").should(
                "be.visible"
            );
        });
    });
});

// Profile email-update and SMS-validation 429 responses are intentionally
// covered by the parameterized helpers in errorAlerts.js. Those helpers verify
// both the general server-error path and the 429-specific alert for each action.

describe("Mobile - Laboratory Orders Report Download", () => {
    beforeEach(() => {
        setupStandardFixtures();

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
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]")
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
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]")
            .should("be.visible")
            .click({ force: true });

        cy.get("[data-testid=errorBanner]").contains(
            "Unable to download laboratory report"
        );
        cy.get("[data-testid=backBtn]").click({ force: true });
    });
});

describe("Mobile - Covid19 Orders Report Download", () => {
    beforeEach(() => {
        setupStandardFixtures();

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
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]")
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
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]")
            .should("be.visible")
            .click({ force: true });

        cy.get("[data-testid=errorBanner]").contains(
            "Unable to download COVID‑19 laboratory report"
        );
        cy.get("[data-testid=backBtn]").click({ force: true });
    });
});

describe("Dependents", () => {
    const validDependent = {
        firstName: "Sam ", // Add end space to ensure field is trimmed
        lastName: "Testfive ", // Add end space to ensure field is trimmed
        wrongLastName: "Testfive2",
        invalidDoB: "2007-Aug-05",
        doB: "2025-Mar-15",
        testDate: "2020-Mar-21",
        phn: "9874307168",
        hdid: "645645767756756767",
    };

    // This should not be the same in order to test Add dependent too many requests error, the new duplicate dependent check prevents using validDependent
    const alternativeDependent = {
        firstName: "Sammy",
        lastName: "Testfivey",
        doB: "2025-Mar-15",
        testDate: "2020-Mar-21",
        phn: "9735361219",
        hdid: "645645767756756767",
    };

    beforeEach(() => {
        setupDependentsPage(
            [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
            () => {
                cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
                    fixture: "LaboratoryService/covid19Orders.json",
                });
            }
        );
    });

    it("Delete Dependent: Too Many Requests Error", () => {
        cy.intercept("DELETE", "**/UserProfile/*/Dependent/*", {
            statusCode: 429,
        });
        cy.get(`[data-testid=dependent-card-${validDependent.phn}]`).within(
            () => {
                cy.get("[data-testid=dependentMenuBtn]").click();
                cy.document()
                    .find("[data-testid=deleteDependentMenuBtn]")
                    .click();
            }
        );
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });

    it("Add Dependent: Too Many Requests Error", () => {
        cy.intercept("POST", "**/UserProfile/*/Dependent", {
            statusCode: 429,
        });
        cy.get("[data-testid=add-dependent-button]").click();

        cy.get("[data-testid=dependent-first-name-input] input")
            .clear()
            .type(alternativeDependent.firstName);
        cy.get("[data-testid=dependent-last-name-input] input")
            .clear()
            .type(alternativeDependent.lastName);
        cy.get("[data-testid=dependent-date-of-birth-input] input")
            .clear()
            .type(alternativeDependent.doB);
        cy.get("[data-testid=dependent-phn-input]  input")
            .clear()
            .type(alternativeDependent.phn);
        cy.get("[data-testid=dependent-terms-checkbox]  input").check({
            force: true,
        });

        cy.get("[data-testid=register-dependent-btn]").click();

        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });
});

describe("Dependent - Immunizaation History Tab - report download error handling", () => {
    beforeEach(() => {
        setupDependentsPage(
            [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
            () => {
                cy.intercept("GET", "**/Immunization?hdid=*", {
                    fixture: "ImmunizationService/dependentImmunization.json",
                });
            }
        );
    });

    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("POST", "**/Report", {
            statusCode: 429,
        });

        cy.get(`[data-testid=immunization-tab-title-${dependentHdid}]`)
            .should("be.visible")
            .click();

        // History tab
        cy.get(`[data-testid=immunization-tab-div-${dependentHdid}]`).within(
            () => {
                cy.contains("button", "History").click();
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
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });

    it("Unsuccessful Response: Internal Server Error", () => {
        cy.intercept("POST", "**/Report", {
            statusCode: 500,
        });

        cy.get(`[data-testid=immunization-tab-title-${dependentHdid}]`)
            .should("be.visible")
            .click();

        // History tab
        cy.get(`[data-testid=immunization-tab-div-${dependentHdid}]`).within(
            () => {
                cy.contains("button", "History").click();
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
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=errorBanner]").should("not.be.empty");
    });
});

describe("Comments", () => {
    it("Add Comment: Too Many Requests Error", () => {
        setupStandardFixtures();

        cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
            fixture: "LaboratoryService/covid19Orders.json",
        });

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

        cy.get("[data-testid=entryCardDetailsTitle]")
            .first()
            .click({ force: true });

        // Add comment
        cy.get("[data-testid=add-comment-text-area] textarea")
            .first()
            .type(testComment);
        cy.get("[data-testid=post-comment-btn]").first().click();

        // Verify
        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });
});

describe("Notes", () => {
    it("Add Note: Too Many Requests Error", () => {
        setupStandardFixtures();

        cy.intercept("GET", "**/Note/*", {
            fixture: "NoteService/notes-no-records.json",
        });

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
        setupStandardFixtures();

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
        cy.get("[data-testid=noteTitleInput] input").clear().type("Test Edit");
        cy.get("[data-testid=saveNoteBtn]").click();

        // Verify
        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });

    it("Delete Note: Too Many Requests Error", () => {
        setupStandardFixtures();

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
});

describe("Export Records - Immunizaation - report download error handling", () => {
    beforeEach(() => {
        setupStandardFixtures();
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

        cy.vSelect("[data-testid=report-type]", "Immunizations");

        cy.get("[data-testid=export-record-btn]").click();
        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.get("[data-testid=generic-message-modal]").should("not.exist");
        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });

    it("Unsuccessful Response: Internal Server Error", () => {
        cy.intercept("POST", "**/Report", {
            statusCode: 500,
        });

        cy.vSelect("[data-testid=report-type]", "Immunizations");

        cy.get("[data-testid=export-record-btn]").click();
        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.get("[data-testid=generic-message-modal]").should("not.exist");
        cy.get("[data-testid=errorBanner]").should("not.be.empty");
    });
});
