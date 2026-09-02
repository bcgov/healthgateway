import { AuthMethod } from "../../../support/constants";
import {
    fillTemplate,
    ReminderBody,
    ResultBody,
} from "../../../support/functions/bcCancerScreening";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("Clinical Document cards", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/ClinicalDocument/*", {
            fixture: "ClinicalDocumentService/clinicalDocument.json",
        });
        cy.configureSettings({
            datasets: [{ name: "clinicalDocument", enabled: true }],
        });
        setupStandardFixtures();
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Displays clinical document details", () => {
        cy.get("[data-testid=clinicaldocumentTitle]")
            .first()
            .should("be.visible")
            .click({ force: true });
        cy.get("[data-testid=clinical-document-discipline]").should(
            "be.visible"
        );
        cy.get("[data-testid=clinical-document-facility]").should("be.visible");
    });
});

describe("Diagnostic Imaging cards", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/PatientData/*?patientDataTypes=*", {
            fixture: "PatientData/allTimelineData.json",
        });
        cy.configureSettings({
            datasets: [{ name: "diagnosticImaging", enabled: true }],
        });
        setupStandardFixtures();
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    [
        { hasFile: true, downloadVisible: true },
        { hasFile: false, downloadVisible: false },
    ].forEach(({ hasFile, downloadVisible }) => {
        it(`Displays details ${hasFile ? "with" : "without"} a file`, () => {
            cy.get("[data-testid=timelineCard]").then(($cards) => {
                const matchingCards = hasFile
                    ? $cards.filter(":has([data-testid=attachment-button])")
                    : $cards.not(":has([data-testid=attachment-button])");

                cy.wrap(matchingCards.first()).within(() => {
                    cy.get("[data-testid=diagnosticimagingTitle]").click({
                        force: true,
                    });
                    cy.get(
                        "[data-testid=diagnostic-imaging-procedure-description]"
                    ).should("be.visible");
                    cy.get(
                        "[data-testid=diagnostic-imaging-health-authority]"
                    ).should("be.visible");
                    cy.get(
                        "[data-testid=diagnostic-imaging-download-button]"
                    ).should(downloadVisible ? "be.visible" : "not.exist");
                });
            });
        });
    });
});

describe("Medication cards", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/MedicationStatement/*", {
            fixture: "MedicationService/medicationStatement.json",
        });
        cy.configureSettings({
            datasets: [{ name: "medication", enabled: true }],
        });
        setupStandardFixtures();
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Displays prescription and pharmacist assessment details", () => {
        cy.get("[data-testid=medicationTitle]")
            .not(":contains('Pharmacist Assessment')")
            .first()
            .click({ force: true });
        cy.get("[data-testid=medication-practitioner]").should("be.visible");
        cy.get("[data-testid=medication-directions]").should("be.visible");
        cy.get("[data-testid=pharmacist-outcome]").should("not.exist");

        cy.contains("[data-testid=medicationTitle]", "Pharmacist Assessment")
            .first()
            .click({ force: true });
        cy.get("[data-testid=pharmacist-outcome]").should("be.visible");
    });
});

describe("Medication Request", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "specialAuthorityRequest",
                    enabled: true,
                },
            ],
        });
        cy.intercept("GET", "**/MedicationRequest/*", {
            fixture: "MedicationService/medicationRequest.json",
        });
        setupStandardFixtures();
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card Details", () => {
        cy.get("[data-testid=specialauthorityrequestTitle]").should(
            "be.visible"
        );
        cy.get("[data-testid=medicationPractitioner]").should("not.exist");
        cy.get("[data-testid=entryCardDetailsTitle]")
            .first()
            .click({ force: true });
        cy.get("[data-testid=medicationPractitioner]").should("be.visible");
    });
});

describe("BC Cancer Screening cards", () => {
    let fixtureToUse;

    function validateReminderCard(
        cardTitle,
        cardSubTitle,
        cardButtonText,
        cardBody
    ) {
        cy.log(
            `Validate reminder card for card title: ${cardTitle} | cardSubTitle: ${cardSubTitle} | cardButtonText: ${cardButtonText} | cardBody: ${cardBody}`
        );
        cy.get("[data-testid=timelineCard]")
            .filter(`:contains("${cardTitle}")`)
            .first()
            .within(() => {
                cy.get("[data-testid=bccancerscreeningTitle]").click({
                    force: true,
                });
                cy.get("[data-testid=bc-cancer-screening-download-button]")
                    .contains(cardButtonText, { matchCase: false })
                    .should("exist");
                cy.get("[data-testid=entryCardDetailsTitle]")
                    .should("be.visible")
                    .contains(cardTitle);
                cy.get("[data-testid=entryCardDetailsSubtitle]")
                    .should("be.visible")
                    .contains(cardSubTitle);
                cy.get("[data-testid=bc-cancer-screening-body]")
                    .should("be.visible")
                    .contains(cardBody);
            });
    }

    function validateResultCard(
        cardTitle,
        cardSubTitle,
        cardButtonText,
        cardBody
    ) {
        cy.log(
            `Validate result card for card title: ${cardTitle} | cardSubTitle: ${cardSubTitle} | cardButtonText: ${cardButtonText} | cardBody: ${cardBody}`
        );
        cy.get("[data-testid=timelineCard]")
            .filter(`:contains("${cardTitle}")`)
            .first()
            .within(() => {
                cy.get("[data-testid=bccancerscreeningTitle]").click({
                    force: true,
                });
                cy.get("[data-testid=bc-cancer-screening-download-button]")
                    .contains(cardButtonText, { matchCase: false })
                    .should("exist");
                cy.get("[data-testid=entryCardDetailsTitle]")
                    .should("be.visible")
                    .contains(cardTitle);
                cy.get("[data-testid=entryCardDetailsSubtitle]")
                    .should("be.visible")
                    .contains(cardSubTitle);
                cy.get("[data-testid=bc-cancer-result-body]")
                    .should("be.visible")
                    .contains(cardBody);
            });
    }

    function sharedSetup() {
        cy.intercept("GET", "**/PatientData/*?patientDataTypes=*", {
            fixture: fixtureToUse,
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "bcCancerScreening",
                    enabled: true,
                },
            ],
        });
        setupStandardFixtures();
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    }

    it("Displays the supported cancer program cards", () => {
        fixtureToUse = "PatientData/bcCancerPrograms.json";
        sharedSetup();

        [
            ["Breast", "breast cancer"],
            ["Cervix", "cervix cancer"],
            ["Colon", "colon cancer"],
            ["Lung", "lung cancer"],
        ].forEach(([displayName, bodyProgram]) => {
            validateReminderCard(
                `${displayName} Screening Reminder Letter`,
                `${displayName} Screening`,
                "View Letter",
                fillTemplate(ReminderBody, { program: bodyProgram })
            );
            validateResultCard(
                `${displayName} Screening Result Letter`,
                `${displayName} Screening`,
                "View Letter",
                fillTemplate(ResultBody, { program: bodyProgram })
            );
        });
    });

    it("Should display different cards for different types - program name is Unknown Cancer", () => {
        fixtureToUse = "PatientData/bcCancerTypesUnknownCancer.json";
        sharedSetup();
        validateReminderCard(
            "Unknown Cancer Screening Reminder Letter",
            "Unknown Cancer Screening",
            "View Letter",
            fillTemplate(ReminderBody, { program: "unknown cancer" })
        );
        validateResultCard(
            "Unknown Cancer Screening Result Letter",
            "Unknown Cancer Screening",
            "View Letter",
            fillTemplate(ResultBody, { program: "unknown cancer" })
        );
    });

    it("Should display different cards for different types - program name is null", () => {
        fixtureToUse = "PatientData/bcCancerTypesNull.json";
        sharedSetup();
        validateReminderCard(
            "Unknown Screening Reminder Letter",
            "Unknown Screening",
            "View Letter",
            fillTemplate(ReminderBody, { program: "unknown" })
        );
        validateResultCard(
            "Unknown Screening Result Letter",
            "Unknown Screening",
            "View Letter",
            fillTemplate(ResultBody, { program: "unknown" })
        );
    });
});
