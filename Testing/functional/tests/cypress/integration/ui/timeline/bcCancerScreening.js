import { AuthMethod } from "../../../support/constants";
import {
    fillTemplate,
    ReminderBody,
    ResultBody,
} from "../../../support/functions/bcCancerScreening";
import { setupStandardFixtures } from "../../../support/functions/intercept";

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
