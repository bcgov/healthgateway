import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("BC Cancer Screening cards", () => {
    function testCard(cardTitle, cardButtonText) {
        cy.get("[data-testid=timelineCard")
            .filter(`:contains("${cardTitle}")`)
            .first()
            .within(() => {
                cy.get("[data-testid=bccancerscreeningTitle]").click({
                    force: true,
                });
                cy.get("[data-testid=bc-cancer-screening-download-button]")
                    .contains(cardButtonText, { matchCase: false })
                    .should("exist");
            });
    }

    beforeEach(() => {
        cy.intercept("GET", "**/PatientData/*?patientDataTypes=*", {
            fixture: "PatientData/bcCancerTypes.json",
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
    });

    it("Should display different cards for different types", () => {
        testCard("BC Cancer Screening Reminder", "View Letter");
        testCard("BC Cancer Screening Result", "View Letter");
    });
});
