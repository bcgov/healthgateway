import { AuthMethod } from "../../../support/constants";
const defaultTimeout = 60000;

describe("Communication", () => {
    beforeEach(() => {
        cy.configureSettings({});
    });

    it("Landing Banner", () => {
        cy.logout();
        cy.intercept("GET", `**/Communication/*`).as("getCommunication");
        cy.visit("/");
        cy.wait("@getCommunication", { timeout: defaultTimeout });
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("Test Banner");

        cy.intercept("GET", "**/UserProfile/termsofservice?api-version=2.0").as(
            "getTermsOfService"
        );
        cy.visit("/termsOfService");
        cy.wait("@getCommunication", { timeout: defaultTimeout });
        cy.wait("@getTermsOfService", { timeout: defaultTimeout });
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("Test Banner");

        cy.visit("/404");
        cy.wait("@getCommunication", { timeout: defaultTimeout });
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("Test Banner");
    });

    it("InApp Banner", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");

        cy.visit("/dependents");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");

        cy.visit("/reports");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");

        cy.visit("/profile");
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");
    });
});
