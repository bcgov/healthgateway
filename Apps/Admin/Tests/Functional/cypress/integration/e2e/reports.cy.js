import { getTableRows, selectTab } from "../../utilities/sharedUtilities";

const blockedHdid = "GO4DOSMRJ7MFKPPADDZ3FK2MOJ45SFKONJWR67XNLMZQFNEHDKDA";
const defaultTimeout = 60000;

function setupPatientDetailsAliases() {
    cy.intercept("GET", "**/Support/PatientSupportDetails*").as(
        "getPatientSupportDetails"
    );

    cy.intercept("GET", "**/Support/Users*").as("getUsers");
}

function waitForPatientDetailsDataLoad() {
    cy.wait("@getPatientSupportDetails", { timeout: defaultTimeout });
    cy.wait("@getUsers", { timeout: defaultTimeout });
}

describe("Reports", () => {
    beforeEach(() => {
        cy.intercept("GET", `**/AdminReport/BlockedAccess`).as(
            "getBlockedAccess"
        );

        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/reports"
        );

        cy.wait("@getBlockedAccess", { timeout: defaultTimeout });
    });

    it("Verify blocked access reports", () => {
        cy.log("Checking blocked access table");
        getTableRows("[data-testid=blocked-access-table]").should(
            "have.length.gt",
            0
        );

        cy.log("Checking blocked access row click goes to patient details");
        setupPatientDetailsAliases();
        cy.contains("[data-testid=blocked-access-hdid]", blockedHdid)
            .should("be.visible")
            .click();

        waitForPatientDetailsDataLoad();
        cy.get("[data-testid=patient-details-back-button]").should(
            "be.visible"
        );

        cy.url().should("include", "/patient-details");
        selectTab("[data-testid=patient-details-tabs]", "Account");
        cy.contains("[data-testid=patient-hdid]", blockedHdid).should(
            "be.visible"
        );
    });

    it("Verify protected dependent reports", () => {
        cy.log("Checking protected dependents table");
        getTableRows("[data-testid=protected-dependents-table]").should(
            "have.length.gt",
            0
        );

        cy.log(
            "Checking protected dependent row click goes to delegation page"
        );
        cy.contains("[data-testid=protected-dependent-phn]", /[0-9].*?/)
            .first()
            .should("be.visible")
            .click();

        cy.url().should("include", "/delegation");
        cy.get("[data-testid=dependent-table]").should("be.visible");
    });
});
