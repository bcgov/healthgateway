import { AuthMethod } from "../../../support/constants";
const defaultTimeout = 60000;

describe("Communication", () => {
    beforeEach(() => {
        cy.configureSettings({});
    });

    it("Landing Banner", () => {
        cy.logout();
        cy.intercept("GET", "**/Communication/0").as("getBannerCommunication");
        cy.visit("/");
        cy.wait("@getBannerCommunication", { timeout: defaultTimeout });
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("Test Banner");

        cy.intercept("GET", "**/UserProfile/termsofservice*").as(
            "getTermsOfService"
        );
        cy.visit("/termsOfService");
        cy.wait("@getBannerCommunication", { timeout: defaultTimeout });
        cy.wait("@getTermsOfService", { timeout: defaultTimeout });
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("Test Banner");

        cy.visit("/404");
        cy.wait("@getBannerCommunication", { timeout: defaultTimeout });
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("Test Banner");
    });

    it("InApp Banner", () => {
        cy.intercept("GET", "**/Communication/2").as("getInAppCommunication");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.wait("@getInAppCommunication", { timeout: defaultTimeout });
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");

        cy.visit("/dependents");
        cy.wait("@getInAppCommunication", { timeout: defaultTimeout });
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");

        cy.visit("/reports");
        cy.wait("@getInAppCommunication", { timeout: defaultTimeout });
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");

        cy.visit("/profile");
        cy.wait("@getInAppCommunication", { timeout: defaultTimeout });
        cy.get("[data-testid=communicationBanner]")
            .should("exist")
            .contains("In-App Banner");
    });
});
