import {
    configureFakeSettings,
    fakeLogin,
    useFakeEnvironment,
} from "../../../support/functions/fakeEnvironment";
import {
    setupCommunicationFixture,
    setupStandardFixtures,
} from "../../../support/functions/intercept";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();

describe("Communication banners", () => {
    beforeEach(() => {
        configureFakeSettings({});
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
        fakeLogin("/home");

        ["/home", "/dependents", "/reports", "/profile"].forEach((path) => {
            cy.visit(path);
            cy.contains(
                "[data-testid=communicationBanner]",
                "In-App Banner"
            ).should("be.visible");
        });
    });
});
