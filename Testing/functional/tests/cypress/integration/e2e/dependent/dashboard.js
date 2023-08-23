const { AuthMethod } = require("../../../support/constants");

const validDependentHdid = "162346565465464564565463257";
const validDependentTimelinePath = `/dependents/${validDependentHdid}/timeline`;
const validDependentFederalProofOfVaccinationButtonId = `[data-testid=proof-vaccination-card-btn-${validDependentHdid}]`;

describe("dependents - dashboard", () => {
    beforeEach(() => {
        cy.configureSettings({
            homepage: {
                showFederalProofOfVaccination: true,
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
        const selector = `[data-testid=dependent-health-records-button-${validDependentHdid}]`;
        cy.get(selector).should("be.visible").click();
        cy.location("pathname").should("eq", validDependentTimelinePath);
    });

    it("Validate download of federal proof of vaccination", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus/pdf?hdid=*");

        cy.get(validDependentFederalProofOfVaccinationButtonId)
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("VaccineProof.pdf", {
            timeout: 60000,
            interval: 5000,
        });
    });
});
