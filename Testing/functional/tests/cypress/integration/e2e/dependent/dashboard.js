import { AuthMethod } from "../../../support/constants";
import { waitForImmunizations } from "../../../support/functions/timeline";

const defaultTimeout = 120000;

const validDependent = {
    hdid: "162346565465464564565463257",
    timelinePath: "/dependents/162346565465464564565463257/timeline",
    healthRecordsButtonSelector:
        "[data-testid=dependent-health-records-button-162346565465464564565463257]",
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

    it("Validate download of vaccine recommendations", () => {
        cy.intercept("GET", `**/Immunization?hdid=${validDependent.hdid}`).as(
            "getDependentImmunizations"
        );

        cy.get(validDependent.recommendationsCardSelector)
            .should("be.visible")
            .click();

        waitForImmunizations("@getDependentImmunizations", defaultTimeout);

        cy.get(recommendationsTableSelector).should("exist");
        cy.get(recommendationsDownloadButtonSelector)
            .should("be.visible")
            .and("be.enabled")
            .click();
        cy.get(recommendationsDownloadPdfButtonSelector).first().click();

        cy.intercept("POST", "**/Report").as("postReport");
        cy.get(confirmationModalButton).click();
        cy.wait("@postReport", { timeout: defaultTimeout }).then(() => {
            cy.verifyDownload(
                "HealthGatewayDependentImmunizationRecommendationReport.pdf",
                {
                    timeout: 60000,
                    interval: 5000,
                }
            );
        });
    });
});
