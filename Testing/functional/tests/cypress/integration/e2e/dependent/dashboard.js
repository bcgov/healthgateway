const { AuthMethod } = require("../../../support/constants");

const validDependent = {
    hdid: "162346565465464564565463257",
    timelinePath: "/dependents/162346565465464564565463257/timeline",
    healthRecordsButtonSelector:
        "[data-testid=dependent-health-records-button-162346565465464564565463257]",
    federalProofOfVaccinationButtonSelector:
        "[data-testid=proof-vaccination-card-btn-162346565465464564565463257]",
    recommendationsCardSelector:
        "[data-testid=recommendations-card-162346565465464564565463257]",
};
const recommendationsTableSelector =
    "[data-testid=recommendation-history-report-table]";
const recommendationsDownloadButtonSelector =
    "[data-testid=export-recommendations-record-button]";
const recommendationsDownloadPdfButtonSelector =
    "[data-testid=export-record-menu] .v-list-item";
const confirmationModalButton = "[data-testid=generic-message-submit-btn]";

describe("dependents - dashboard", () => {
    beforeEach(() => {
        cy.configureSettings({
            homepage: {
                showFederalProofOfVaccination: true,
                showRecommendationsLink: true,
            },
            dependents: {
                enabled: true,
                timelineEnabled: true,
            },
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
                {
                    name: "labResult",
                    enabled: true,
                },
                {
                    name: "healthVisit",
                    enabled: true,
                },
                {
                    name: "hospitalVisit",
                    enabled: true,
                },
                {
                    name: "medication",
                    enabled: true,
                },
                {
                    name: "specialAuthorityRequest",
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

    it("Validate clicking health records button loads timeline", () => {
        cy.get(validDependent.healthRecordsButtonSelector)
            .should("be.visible")
            .click();
        cy.location("pathname").should("eq", validDependent.timelinePath);
    });

    it("Validate download of federal proof of vaccination", () => {
        cy.get(validDependent.federalProofOfVaccinationButtonSelector)
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get(confirmationModalButton).click();

        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("VaccineProof.pdf", {
            timeout: 60000,
            interval: 5000,
        });
    });

    it("Validate download of vaccine recommendations", () => {
        cy.get(validDependent.recommendationsCardSelector)
            .should("be.visible", "be.enabled")
            .click();

        cy.get(recommendationsTableSelector).should("exist");
        cy.get(recommendationsDownloadButtonSelector)
            .should("be.visible", "be.enabled")
            .click();
        cy.get(recommendationsDownloadPdfButtonSelector).first().click();
        cy.get(confirmationModalButton).click();

        cy.verifyDownload(
            "HealthGatewayDependentImmunizationRecommendationReport.pdf",
            {
                timeout: 60000,
                interval: 5000,
            }
        );
    });
});
