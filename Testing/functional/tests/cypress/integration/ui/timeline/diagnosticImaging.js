import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

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
