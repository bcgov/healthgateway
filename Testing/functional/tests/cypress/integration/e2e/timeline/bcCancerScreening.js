const { AuthMethod } = require("../../../support/constants");
import {
    fillTemplate,
    ReminderBody,
    ResultBody,
} from "../../../support/functions/bcCancerScreening";
const {
    validateAttachmentDownload,
    validateFileDownload,
} = require("../../../support/functions/timeline");

describe("BC Cancer", () => {
    function sharedSetup() {
        cy.configureSettings({
            datasets: [
                {
                    name: "bcCancerScreening",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    }

    function validateReminderCard(cardTitle, cardSubTitle, cardBody) {
        cy.log(
            `Validate reminder card for card title: ${cardTitle} | cardSubTitle: ${cardSubTitle} | cardBody: ${cardBody}`
        );
        cy.contains("[data-testid=timelineCard]", cardTitle)
            .scrollIntoView()
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=bccancerscreeningTitle]").click({
                    force: true,
                });
                cy.get("[data-testid=entryCardDetailsTitle]")
                    .should("be.visible")
                    .contains(cardTitle);
                cy.get("[data-testid=entryCardDetailsSubtitle]")
                    .should("be.visible")
                    .contains(cardSubTitle);
                cy.get("[data-testid=bc-cancer-screening-body]")
                    .should("be.visible")
                    .contains(cardBody);

                validateFileDownload(
                    "[data-testid=bc-cancer-screening-download-button]",
                    false
                );

                validateAttachmentDownload();
            });

        // Close the card to prevent overflow
        cy.contains("[data-testid=timelineCard]", cardTitle)
            .find("[data-testid=entryCardDetailsTitle]")
            .click({ force: true });
    }

    function validateResultCard(cardTitle, cardSubTitle, cardBody) {
        cy.log(
            `Validate result card for card title: ${cardTitle} | cardSubTitle: ${cardSubTitle} | cardBody: ${cardBody}`
        );
        cy.contains("[data-testid=timelineCard]", cardTitle)
            .scrollIntoView()
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=bccancerscreeningTitle]").click({
                    force: true,
                });
                cy.get("[data-testid=entryCardDetailsTitle]")
                    .should("be.visible")
                    .contains(cardTitle);
                cy.get("[data-testid=entryCardDetailsSubtitle]")
                    .should("be.visible")
                    .contains(cardSubTitle);
                cy.get("[data-testid=bc-cancer-result-body]")
                    .should("be.visible")
                    .contains(cardBody);

                validateFileDownload(
                    "[data-testid=bc-cancer-screening-download-button]",
                    false
                );

                validateAttachmentDownload();
            });

        // Close the card to prevent overflow
        cy.contains("[data-testid=timelineCard]", cardTitle)
            .find("[data-testid=entryCardDetailsTitle]")
            .click({ force: true });
    }

    it("Validate BC Cancer Cards and download", () => {
        sharedSetup();

        cy.log("Validate BC Breast Cancer Screening Card");
        validateReminderCard(
            "BC Breast Cancer Screening Reminder Letter",
            "Breast Screening",
            fillTemplate(ReminderBody, { program: "breast cancer" })
        );
        validateResultCard(
            "BC Breast Cancer Screening Result Letter",
            "Breast Screening",
            fillTemplate(ResultBody, { program: "breast cancer" })
        );

        cy.log("Validate BC Cervix Cancer Screening Card");
        validateReminderCard(
            "BC Cervix Cancer Screening Reminder Letter",
            "Cervix Screening",
            fillTemplate(ReminderBody, { program: "cervix cancer" })
        );
        validateResultCard(
            "BC Cervix Cancer Screening Result Letter",
            "Cervix Screening",
            fillTemplate(ResultBody, { program: "cervix cancer" })
        );

        cy.log("Validate BC Colon Cancer Screening Card");
        validateReminderCard(
            "BC Colon Cancer Screening Reminder Letter",
            "Colon Screening",
            fillTemplate(ReminderBody, { program: "colon cancer" })
        );
        validateResultCard(
            "BC Colon Cancer Screening Result Letter",
            "Colon Screening",
            fillTemplate(ResultBody, { program: "colon cancer" })
        );

        cy.log("Validate BC Lung Cancer Screening Card");
        validateReminderCard(
            "BC Lung Cancer Screening Reminder Letter",
            "Lung Screening",
            fillTemplate(ReminderBody, { program: "lung cancer" })
        );
        validateResultCard(
            "BC Lung Cancer Screening Result Letter",
            "Lung Screening",
            fillTemplate(ResultBody, { program: "lung cancer" })
        );
    });
});
