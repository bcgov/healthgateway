import { AuthMethod, Dataset } from "../../../../support/constants";

const defaultTimeout = 60000;

// Jennifer T is seeded with access to the dependent timeline and real PHSA data.
const dependent = {
    hdid: "162346565465464564565463257",
    formattedName: "JENNIFER T",
    healthRecordsButtonSelector:
        "[data-testid=dependent-health-records-button-162346565465464564565463257]",
};

describe("Dependent Timeline Integration", () => {
    it("Loads an authorized dependent and a timeline dataset", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: Dataset.ClinicalDocument,
                    enabled: true,
                },
            ],
            dependents: {
                enabled: true,
                timelineEnabled: true,
            },
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents",
            { waitForPatient: false }
        );

        cy.intercept("GET", "**/ClinicalDocument/*").as(
            "getDependentClinicalDocuments"
        );
        cy.get(dependent.healthRecordsButtonSelector)
            .should("be.visible")
            .click();

        cy.wait("@getDependentClinicalDocuments", {
            timeout: defaultTimeout,
        });
        cy.location("pathname").should(
            "eq",
            `/dependents/${dependent.hdid}/timeline`
        );
        cy.checkTimelineHasLoaded();
        cy.get("[data-testid=page-title]").should(
            "contain",
            dependent.formattedName
        );
        cy.get("[data-testid=clinicaldocumentTitle]").should("be.visible");
    });
});
