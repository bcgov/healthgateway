import { AuthMethod } from "../../../support/constants";
import {
    testAddCommentError,
    testAddQuickLinkError,
    testEditEmailError,
    testEditSmsError,
    testGetConfigurationError,
    testGetProfileErrorOnLoad,
    testRegisterError,
    testRemoveQuickLinkError,
    testValidateEmailError,
    testVerifySmsError,
} from "../../../support/functions/errorAlertActions";
import { setupStandardFixtures } from "../../../support/functions/intercept";

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

describe("429 Alerts", () => {
    it("429 Error Retrieving Configuration", () => {
        testGetConfigurationError(429);
    });

    it("429 Error Retrieving Profile on Load", () => {
        testGetProfileErrorOnLoad(429);
    });

    it("429 Error Registering", () => {
        testRegisterError(429);
    });

    it("429 Error Validating Email", () => {
        testValidateEmailError(429);
    });
    it("429 Error Adding Quick Link", () => {
        testAddQuickLinkError(429);
    });

    it("429 Error Adding Comment", () => {
        testAddCommentError(429);
    });

    it("429 Error Removing Quick Link", () => {
        testRemoveQuickLinkError(429);
    });

    it("429 Error Editing SMS Number", () => {
        testEditSmsError(429);
    });

    it("429 Error On SMS Verification", () => {
        testVerifySmsError(429);
    });

    it("429 Error Editing Email", () => {
        testEditEmailError(429);
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
});

describe("Comments", () => {
    it("Add Comment: Too Many Requests Error", () => {
        setupStandardFixtures();

        cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
            fixture: "LaboratoryService/covid19Orders.json",
        }).as("getCovid19Orders");

        cy.intercept("GET", "**/UserProfile/*/Comment", {
            fixture: "UserProfileService/commentNoResult.json",
        }).as("getComments");

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
        cy.wait(["@getCovid19Orders", "@getComments"]);
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
        }).as("getImmunizations");
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
        cy.wait("@getImmunizations");

        cy.get("[data-testid=export-record-btn]").click();
        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.get("[data-testid=generic-message-modal]").should("not.exist");
        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });
});
