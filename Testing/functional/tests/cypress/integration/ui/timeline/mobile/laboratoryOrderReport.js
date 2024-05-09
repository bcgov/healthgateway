import { AuthMethod } from "../../../../support/constants";
import {
    CommunicationFixture,
    CommunicationType,
    setupCommunicationIntercept,
    setupPatientIntercept,
    setupUserProfileIntercept,
} from "../../../../support/functions/intercept";

describe("Laboratory Orders - Report", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrders.json",
        });
        cy.intercept(
            "GET",
            "**/Laboratory/*/Report?hdid=P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A&isCovid19=false",
            {
                fixture: "LaboratoryService/laboratoryReportPdf.json",
            }
        );
        cy.configureSettings({
            datasets: [
                {
                    name: "labResult",
                    enabled: true,
                },
            ],
        });

        setupPatientIntercept();
        setupUserProfileIntercept();
        setupCommunicationIntercept();
        setupCommunicationIntercept({
            communicationType: CommunicationType.InApp,
            communicationFixture: CommunicationFixture.InApp,
        });

        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Download", () => {
        cy.log("Verifying Laboratory Report PDF download");
        cy.get("[data-testid=timelineCard]").last().scrollIntoView().click();

        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=laboratory-report-download-btn]")
                    .should("be.visible")
                    .click({ force: true });
            });

        cy.get("[data-testid=generic-message-submit-btn]")
            .should("be.visible")
            .click({ force: true });
        cy.verifyDownload("Laboratory_Report_2021_04_04-08_43.pdf");
        cy.get("[data-testid=backBtn]").click({ force: true });
    });
});
