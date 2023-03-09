const { AuthMethod } = require("../../../support/constants");

const validDependentHdid = "162346565465464564565463257";
const validDependentTimelinePath = `/dependents/${validDependentHdid}/timeline`;
const validDependentFederalProofOfVaccinationButtonId = `[data-testid=proof-vaccination-card-btn-${validDependentHdid}]`;

function validateDatasetCard(dataset) {
    const selector = `[data-testid=dependent-entry-type-${dataset}-${validDependentHdid}]`;
    cy.get(selector).should("be.enabled", "be.visible").click();
    cy.location("pathname").should("eq", validDependentTimelinePath);
    cy.checkTimelineHasLoaded();

    cy.get("[data-testid=filterContainer]").should("not.exist");
    cy.get("[data-testid=filterDropdown]").click();
    cy.get(`[data-testid=${dataset}-filter]`).should("to.be.checked");
    cy.get("[data-testid=btnFilterCancel]").click();
}

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

    it("Validate dashboard immunizations tab click to timeline", () => {
        validateDatasetCard("Immunization");
    });

    it("Validate dashboard lab results tab click to timeline", () => {
        validateDatasetCard("LabResult");
    });

    it("Validate dashboard covid19 test results tab click to timeline", () => {
        validateDatasetCard("Covid19TestResult");
    });

    it("Validate dashboard clinical documents tab click to timeline", () => {
        validateDatasetCard("ClinicalDocument");
    });

    it("Validate dashboard special authority requests tab click to timeline", () => {
        validateDatasetCard("SpecialAuthorityRequest");
    });

    it("Validate download of federal proof of vaccination", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus/pdf?hdid=*");

        cy.get(validDependentFederalProofOfVaccinationButtonId)
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("VaccineProof.pdf", {
            timeout: 60000,
            interval: 5000,
        });
    });
});
