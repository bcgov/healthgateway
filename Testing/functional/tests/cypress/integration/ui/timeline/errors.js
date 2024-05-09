import { AuthMethod } from "../../../support/constants";
import {
    CommunicationFixture,
    CommunicationType,
    setupCommunicationIntercept,
    setupPatientIntercept,
    setupUserProfileIntercept,
} from "../../../support/functions/intercept";

describe("Banner Error", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Note/*", {
            fixture: "WebClientService/dbError.json",
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "note",
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

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Verify banner error", () => {
        cy.get("[data-testid=errorBanner]").should("be.visible");
        cy.get("[data-testid=errorBanner]").contains(
            "Unable to retrieve notes"
        );
        cy.get("[data-testid=errorDetailsBtn]").should("be.visible");

        cy.get("[data-testid=errorDetailsBtn]").click();

        cy.get("[data-testid=error-details-span-1]").should("be.visible");

        cy.get("[data-testid=copyToClipBoardBtn]").should("be.visible");
    });
});
