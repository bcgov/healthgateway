import { getTableRows, selectTab } from "../../utilities/sharedUtilities";
import { performSearch as search } from "../../utilities/supportUtilities";

const hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const phnWithValidDoses = "9042146714";
const phnWithInvalidDoses = "9735352535";
const phnWithBlockedImmunizations = "9873659643";
const switchName = "Immunization";
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

describe("Patient details page as admin user", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
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

    it("Verify covid immunization section (not blocked) and contains invalid dose", () => {
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
