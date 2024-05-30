import { AuthMethod } from "../../../support/constants";

const defaultTimeout = 60000;

function testPageBreadcrumb(url, dataTestId) {
    cy.intercept("GET", "**/AuthenticatedVaccineStatus?hdid=*").as(
        "getVaccinationStatus"
    );
    cy.intercept("GET", `**/Communication/*`).as("getCommunication");
    cy.intercept("GET", "**/Patient/*").as("getPatient");
    cy.intercept("GET", "**/UserProfile/*").as("getUserProfile");
    cy.intercept("GET", "**/UserProfile/*/Dependent").as("getDependent");

    cy.visit(url);

    if (url === "/covid19") {
        cy.wait("@getVaccinationStatus", { timeout: defaultTimeout });
    }

    if (url === "/dependents") {
        cy.wait("@getDependent", { timeout: defaultTimeout });
    }

    if (url !== "/dependents" && url !== "/profile") {
        cy.wait("@getPatient", { timeout: defaultTimeout });
        cy.wait("@getUserProfile", { timeout: defaultTimeout });
    }

    cy.wait("@getCommunication", { timeout: defaultTimeout });

    cy.get("[data-testid=breadcrumbs]").should("be.visible");
    cy.get(`[data-testid='${dataTestId}'].v-breadcrumbs-item--active`).should(
        "be.visible"
    );
    cy.get("[data-testid=breadcrumb-home]").should("be.visible").click();
    cy.url().should("include", "/home");
}

describe("Breadcrumbs", () => {
    beforeEach(() => {
        cy.configureSettings({
            dependents: {
                enabled: true,
            },
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
    });
    it("Breadcrumbs present on timeline", () =>
        testPageBreadcrumb("/timeline", "breadcrumb-timeline"));
    it("Breadcrumbs present on covid19", () =>
        testPageBreadcrumb("/covid19", "breadcrumb-covid-19"));
    it("Breadcrumbs present on dependents", () =>
        testPageBreadcrumb("/dependents", "breadcrumb-dependents"));
    it("Breadcrumbs present on reports", () =>
        testPageBreadcrumb("/reports", "breadcrumb-export-records"));
    it("Breadcrumbs present on profile", () =>
        testPageBreadcrumb("/profile", "breadcrumb-profile"));
    it("Breadcrumbs present on termsOfService", () =>
        testPageBreadcrumb("/termsOfService", "breadcrumb-terms-of-service"));
});
