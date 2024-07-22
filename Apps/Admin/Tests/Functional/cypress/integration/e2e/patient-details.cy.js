import { performSearch as search } from "../../utilities/supportUtilities";
import { getTableRows, selectTab } from "../../utilities/sharedUtilities";

const hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const phnWithValidDoses = "9042146714";
const phnWithInvalidDoses = "9735352535";
const phnWithBlockedImmunizations = "9873659643";
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

function validateTabDoesNotExist(tabText) {
    cy.get("[data-testid=patient-details-tabs]")
        .contains(".mud-tab", tabText)
        .should("not.exist");
}

function checkAgentAuditHistory() {
    cy.get("[data-testid=agent-audit-history-title]")
        .should("be.visible")
        .click();

    cy.get("[data-testid=agent-audit-history-table]").should("be.visible");

    return cy.get("[data-testid=agent-audit-history-count]").invoke("text");
}

function validateCovid19InputContainsError(inputId) {
    cy.get(inputId)
        .parent()
        .parent()
        .parent()
        .within(() => {
            cy.get("div").contains("Required").should("be.visible");
        });
}

function validateMailAddressFormCancel() {
    cy.get("[data-testid=mail-button]").click();
    cy.get("[data-testid=address-confirmation-form]").should("be.visible");
    cy.get("[data-testid=address-cancel-button]").click();
    cy.get("[data-testid=address-confirmation-form]").should("not.exist");
}

function validateMailAddressFormRequiredInputs() {
    cy.get("[data-testid=mail-button]").click();
    cy.get("[data-testid=address-confirmation-form]").should("exist");
    cy.get("div").contains("Required").should("not.exist");

    cy.get("[data-testid=address-lines-input]").clear();
    cy.get("[data-testid=city-input]").clear();
    cy.get('[data-testid="country-input"]')
        .parent()
        .within(() => {
            cy.get('button[aria-label="Clear"]').click();
        });
    cy.get("body").click(0, 0);

    cy.get("[data-testid=address-confirmation-button]").click();

    validateCovid19InputContainsError("[data-testid=address-lines-input]");
    validateCovid19InputContainsError("[data-testid=city-input]");
    validateCovid19InputContainsError("[data-testid=province-input]");
    validateCovid19InputContainsError("[data-testid=postal-code-input]");

    cy.get("[data-testid=address-cancel-button]").click();
    cy.get("[data-testid=address-confirmation-form]").should("not.exist");
}

function validateMailAddressFormSubmission() {
    cy.get("[data-testid=mail-button]").click();
    cy.get("[data-testid=address-confirmation-form]").should("be.visible");
    cy.get("[data-testid=address-lines-input]")
        .clear()
        .type("9105 ROTTERDAM PLACE");
    cy.get("[data-testid=city-input]").clear().type("CLITHEROE");
    cy.get("[data-testid=country-input]").clear();
    cy.get(".mud-list .mud-list-item-text").contains("Canada").click();
    cy.get("[data-testid=province-input]").click();
    cy.get("[data-testid=province]").contains("British Columbia").click();
    cy.get("[data-testid=postal-code-input]").clear().type("V3X 4J5");

    cy.intercept("POST", "**/Patient/Document").as("postDocument");
    cy.get("[data-testid=address-confirmation-button]").click();
    cy.wait("@postDocument", { timeout: defaultTimeout });

    cy.get("[data-testid=address-confirmation-form]").should("not.exist");
}

function validatePrintVaccineCardSubmission() {
    cy.intercept("GET", `**/Document?phn=${phnWithInvalidDoses}`).as(
        "getVaccineCard"
    );
    cy.scrollTo("bottom", { ensureScrollable: false });
    cy.get("[data-testid=print-button]").click();

    cy.wait("@getVaccineCard").then((interception) => {
        cy.verifyDownload("VaccineProof.pdf", {
            timeout: 60000,
            interval: 5000,
        });
    });
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

    it("Verify profile, account and manage tab details", () => {
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

        cy.log("Verify verification.");
        getTableRows("[data-testid=messaging-verification-table]").should(
            "have.length",
            3
        );

        selectPatientTab("Manage");

        cy.log("Verify dependents.");
        getTableRows("[data-testid=dependent-table]")
            .should("have.length.gt", 1)
            .first()
            .within(() => {
                cy.get("[data-testid=dependent-name]").contains("John Tester");
                cy.get("[data-testid=dependent-dob]").contains("2005-01-01");
                cy.get("[data-testid=dependent-phn]").contains("9874307215");
                cy.get("[data-testid=dependent-address]").should(
                    "not.be.empty"
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

    it("Verify covid immunization section (not blocked), contains invalid dose, mails address submission and prints vaccine card", () => {
        performSearch("PHN", phnWithInvalidDoses);

        selectPatientTab("Profile");

        cy.get("[data-testid=patient-phn]")
            .should("be.visible")
            .contains(phnWithInvalidDoses);
        cy.get("[data-testid=patient-hdid]").should("not.exist");

        cy.scrollTo("bottom", { ensureScrollable: false });
        getTableRows("[data-testid=immunization-table]").should(
            "have.length.greaterThan",
            0
        );
        cy.get("[data-testid=invalid-dose-alert").should("be.visible");
        cy.get("[data-testid=mail-button]").should("be.visible", "be.enabled");
        cy.get("[data-testid=print-button]").should("be.visible", "be.enabled");

        validateMailAddressFormCancel();
        validateMailAddressFormRequiredInputs();
        validateMailAddressFormSubmission();
        validatePrintVaccineCardSubmission();
    });

    it("Verify covid immunization section (not blocked) and contains valid dose", () => {
        performSearch("PHN", phnWithValidDoses);

        selectPatientTab("Profile");

        cy.get("[data-testid=patient-phn]")
            .should("be.visible")
            .contains(phnWithValidDoses);
        cy.get("[data-testid=patient-hdid]").should("not.exist");

        getTableRows("[data-testid=immunization-table]").should(
            "have.length.greaterThan",
            0
        );
        cy.get("[data-testid=invalid-dose-alert").should("not.exist");
    });
});

describe("Patient details page as reviewer", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_reviewer_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    // verify that the reviewer cannot use the block access controls
    it("Verify block access readonly", () => {
        performSearch("HDID", hdid);

        selectPatientTab("Manage");
        validateTabDoesNotExist("Notes");

        cy.get(`[data-testid*="block-access-switch-"]`).each(($el) => {
            // follow the mud tag structure to find the mud-readonly class
            cy.wrap($el).parent().parent().should("have.class", "mud-readonly");
        });

        // Clicke any switch and check if the dirty state has exposed the save and cancel buttons
        cy.get(`[data-testid=block-access-switch-${switchName}]`)
            .should("exist")
            .click();

        cy.get("[data-testid=block-access-cancel]").should("not.exist");
        cy.get("[data-testid=block-access-save]").should("not.exist");
    });
});

describe("Patient Details as Support", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_support_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify covid immunization section (not blocked) and contains invalid dose", () => {
        performSearch("PHN", phnWithInvalidDoses);

        selectPatientTab("Profile");
        validateTabDoesNotExist("Account");
        validateTabDoesNotExist("Manage");
        validateTabDoesNotExist("Notes");

        cy.get("[data-testid=patient-phn]")
            .should("be.visible")
            .contains(phnWithInvalidDoses);
        cy.get("[data-testid=patient-hdid]").should("not.exist");

        cy.scrollTo("bottom", { ensureScrollable: false });
        getTableRows("[data-testid=immunization-table]").should(
            "have.length.greaterThan",
            0
        );
        cy.get("[data-testid=invalid-dose-alert").should("be.visible");
        cy.get("[data-testid=mail-button]").should("be.visible", "be.enabled");
        cy.get("[data-testid=print-button]").should("be.visible", "be.enabled");
    });

    it("Verify covid immunization section (not blocked) and contains valid dose", () => {
        performSearch("PHN", phnWithValidDoses);

        selectPatientTab("Profile");
        validateTabDoesNotExist("Account");
        validateTabDoesNotExist("Manage");
        validateTabDoesNotExist("Notes");

        cy.get("[data-testid=patient-phn]")
            .should("be.visible")
            .contains(phnWithValidDoses);
        cy.get("[data-testid=patient-hdid]").should("not.exist");

        getTableRows("[data-testid=immunization-table]").should(
            "have.length.greaterThan",
            0
        );
        cy.get("[data-testid=invalid-dose-alert").should("not.exist");
    });

    it("Verify covid immunization blocked", () => {
        performSearch("PHN", phnWithBlockedImmunizations);

        selectPatientTab("Profile");
        validateTabDoesNotExist("Account");
        validateTabDoesNotExist("Manage");
        validateTabDoesNotExist("Notes");

        cy.get("[data-testid=patient-phn]")
            .should("be.visible")
            .contains(phnWithBlockedImmunizations);
        cy.get("[data-testid=patient-hdid]").should("not.exist");

        cy.get("[data-testid=immunization-table]").should("not.exist");
        cy.get("[data-testid=blocked-immunization-alert").should("be.visible");
    });

    it("Verify dependent section not shown", () => {
        performSearch("PHN", phnWithValidDoses);

        selectPatientTab("Profile");
        validateTabDoesNotExist("Account");
        validateTabDoesNotExist("Manage");
        validateTabDoesNotExist("Notes");

        cy.get("[data-testid=patient-name]").should("be.visible");
        cy.get("[data-testid=dependent-table]").should("not.exist");
    });
});
