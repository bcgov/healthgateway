import { getTableRows, selectTab } from "../../utilities/sharedUtilities";
import { performSearch as search } from "../../utilities/supportUtilities";

const hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const switchName = "Immunization";
const auditBlockReason = "Test block reason";
const auditUnblockReason = "Test unblock reason";
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

function performSearch(queryType, queryString) {
    setupPatientDetailsAliases();
    search(queryType, queryString);
    waitForPatientDetailsDataLoad();
}

function selectPatientTab(tabText) {
    selectTab("[data-testid=patient-details-tabs]", tabText);
}

function checkAgentAuditHistory() {
    cy.get("[data-testid=agent-audit-history-title]")
        .should("be.visible")
        .click();

    cy.get("[data-testid=agent-audit-history-table]").should("be.visible");

    return cy.get("[data-testid=agent-audit-history-count]").invoke("text");
}

function validateDatasetAccess() {
    cy.log("Verify initial dataset access.");
    cy.get("[data-testid=block-access-loader]").should("not.be.visible");
    cy.get("[data-testid=block-access-switches]").should("be.visible");
    cy.get(`[data-testid*="block-access-switch"]`).should(
        "not.have.attr",
        "readonly"
    );
    cy.get("[data-testid=block-access-cancel]").should("not.exist");
    cy.get("[data-testid=block-access-save]").should("not.exist");

    cy.log("Verify blocked access change can be cancelled.");
    cy.get(`[data-testid=block-access-switch-${switchName}]`)
        .should("exist")
        .click();

    cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
        "be.checked"
    );

    cy.get("[data-testid=block-access-save]").should("be.visible");
    cy.get("[data-testid=block-access-cancel]").should("be.visible").click();

    cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
        "not.be.checked"
    );

    cy.log("Verify blocked access can be blocked with audit reason.");
    selectPatientTab("Notes");

    checkAgentAuditHistory().then((presaveCount) => {
        selectPatientTab("Manage");

        cy.get(`[data-testid=block-access-switch-${switchName}]`)
            .should("exist")
            .click();

        cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
            "be.checked"
        );

        cy.get("[data-testid=block-access-cancel]").should(
            "exist",
            "be.visible"
        );
        cy.get("[data-testid=block-access-save]")
            .should("exist", "be.visible")
            .click();

        cy.get("[data-testid=audit-reason-input")
            .should("be.visible")
            .type(auditBlockReason);

        cy.get("[data-testid=audit-confirm-button]")
            .should("be.visible")
            .click({ force: true });

        cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
            "be.checked"
        );

        selectPatientTab("Notes");

        checkAgentAuditHistory().then((postsaveCount) => {
            expect(Number(postsaveCount)).to.equal(Number(presaveCount) + 1);
        });
    });

    cy.log("Verify blocked access can be unblocked with audit reason.");
    selectPatientTab("Manage");

    cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
        "be.checked"
    );

    selectPatientTab("Notes");

    checkAgentAuditHistory().then((presaveCount) => {
        selectPatientTab("Manage");

        cy.get(`[data-testid=block-access-switch-${switchName}]`)
            .should("exist")
            .click();
        cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
            "not.be.checked"
        );

        cy.get("[data-testid=block-access-cancel]").should(
            "exist",
            "be.visible"
        );
        cy.get("[data-testid=block-access-save]")
            .should("exist", "be.visible")
            .click();

        cy.get("[data-testid=audit-reason-input")
            .should("be.visible")
            .type(auditUnblockReason);

        cy.get("[data-testid=audit-confirm-button]")
            .should("be.visible")
            .click({ force: true });

        cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
            "not.be.checked"
        );

        selectPatientTab("Notes");

        checkAgentAuditHistory().then((postsaveCount) => {
            expect(Number(postsaveCount)).to.equal(Number(presaveCount) + 1);
        });
    });
}

describe("Patient details page as admin user", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify profile, account, dataset access and manage tab details", () => {
        performSearch("HDID", hdid);

        selectPatientTab("Profile");

        cy.log("Verify patient details.");
        cy.get("[data-testid=patient-name]").should("be.visible");
        cy.get("[data-testid=patient-dob]").should("be.visible");
        cy.get("[data-testid=patient-phn]").should("be.visible");
        cy.get("[data-testid=patient-mailing-address]").should("be.visible");
        cy.get("[data-testid=patient-physical-address]").should("be.visible");

        selectPatientTab("Account");

        cy.log("Verify account details.");
        cy.get("[data-testid=patient-hdid]")
            .should("be.visible")
            .contains(hdid);
        cy.get("[data-testid=profile-created-datetime]").should("be.visible");
        cy.get("[data-testid=profile-last-login-datetime]").should(
            "be.visible"
        );

        cy.log("Verify API Registration");
        cy.get("[data-testid=api-registration]").should("be.visible");

        cy.log("Verify Refresh Disagnoistic Image Cache ");
        cy.get("[data-testid=last-imaging-refresh-date]").should("be.visible");

        cy.intercept("POST", "**/Support/Patient/RefreshHealthData*").as(
            "refreshHealthData"
        );
        cy.get("[data-testid=imaging-cache-refresh-button]").click();

        cy.wait("@refreshHealthData", { timeout: defaultTimeout });
        // Intercept already defined in initial search
        cy.wait("@getPatientSupportDetails", { timeout: defaultTimeout });
        cy.get("[data-testid=last-imaging-refresh-date]").should("be.visible");

        cy.log("Verify Refresh Laboratory Cache ");
        cy.get("[data-testid=last-labs-refresh-date]").should("be.visible");
        cy.get("[data-testid=labs-cache-refresh-button]").click();

        cy.wait("@refreshHealthData", { timeout: defaultTimeout });
        // Intercept already defined in initial search
        cy.wait("@getPatientSupportDetails", { timeout: defaultTimeout });
        cy.get("[data-testid=last-labs-refresh-date]").should("be.visible");

        cy.log("Verify verification.");
        getTableRows("[data-testid=messaging-verification-table]").should(
            "have.length",
            3
        );

        selectPatientTab("Manage");

        cy.log("Verify dependents.");
        getTableRows("[data-testid=dependent-table]")
            .filter(':contains("John Tester")')
            .first()
            .within(() => {
                cy.get("[data-testid=dependent-name]").contains("John Tester");
                cy.get("[data-testid=dependent-dob]").contains("2005-01-01");
                cy.get("[data-testid=dependent-phn]").contains("9874307215");
                cy.get("[data-testid=dependent-address]").should(
                    "not.have.text",
                    ""
                );
                cy.get("[data-testid=dependent-expiry-date]")
                    .invoke("text")
                    .should("match", /^\d{4}-\d{2}-\d{2}$/);
                cy.get("[data-testid=dependent-protected-icon]").should(
                    "not.exist"
                );
            });

        validateDatasetAccess();
    });
});
