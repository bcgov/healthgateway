import { AuthMethod } from "../../../support/constants";
import {
    setupCommunicationFixture,
    setupStandardFixtures,
} from "../../../support/functions/intercept";

describe("Communication banners", () => {
    beforeEach(() => {
        cy.configureSettings({});
    });

    it("Displays the landing banner on public pages", () => {
        setupCommunicationFixture();
        cy.intercept("GET", "**/UserProfile/termsofservice*", {
            fixture: "UserProfileService/termsOfService.json",
        });

        ["/", "/termsOfService", "/404"].forEach((path) => {
            cy.visit(path);
            cy.contains(
                "[data-testid=communicationBanner]",
                "Test Banner"
            ).should("be.visible");
        });
    });

    it("Displays the in-app banner on authenticated pages", () => {
        setupStandardFixtures();
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );

        ["/home", "/dependents", "/reports", "/profile"].forEach((path) => {
            cy.visit(path);
            cy.contains(
                "[data-testid=communicationBanner]",
                "In-App Banner"
            ).should("be.visible");
        });
    });
});
