const defaultTimeout = 60000;
const rowSelector = "[data-testid=feedback-table] tbody tr.mud-table-row";

function setupPatientDetailsAliases() {
    cy.intercept("GET", "**/Support/Users*").as("getUsers");
    cy.intercept("GET", "**/Support/PatientSupportDetails*").as(
        "getPatientSupportDetails"
    );
}

function waitForPatientDetailsDataLoad() {
    cy.wait("@getUsers", { timeout: defaultTimeout });
    cy.wait("@getPatientSupportDetails", { timeout: defaultTimeout });
}

describe("Feedback Review", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/feedback"
        );
    });

    it("Navigating to Support Page", () => {
        cy.log("Looking up feedback author.");
        setupPatientDetailsAliases();

        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-person-search-button]")
                    .should("be.visible")
                    .click();
            });

        waitForPatientDetailsDataLoad();
        cy.location("pathname").should("eq", "/patient-details");
    });
});
