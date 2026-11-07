import {
    configureFakeSettings,
    fakeLogin,
    useFakeEnvironment,
} from "../../../support/functions/fakeEnvironment";
import { FakeUser } from "../../../support/functions/fakeKeycloak";
import { setupStandardFixtures } from "../../../support/functions/intercept";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();

const HDID = "K6HL4VX67CZ2PGSZ2ZOIR4C3PGMFFBW5CIOXM74D6EQ7RYYL7P4A";

describe("Need to accept terms of service", () => {
    beforeEach(() => {
        configureFakeSettings({});
        setupStandardFixtures({
            patientHdid: HDID,
            notificationHdid: HDID,
            userProfileHdid: HDID,
            userProfileFixture: "UserProfileService/userProfileAcceptTos.json",
        });

        cy.intercept("GET", "**/UserProfile/termsofservice?api-version=2.0", {
            fixture: "UserProfileService/termsOfService.json",
        });

        fakeLogin("/home", { user: FakeUser.AcceptTermsOfService });
    });

    it("Validate accept terms of service", () => {
        cy.url().should("include", "/acceptTermsOfService");

        cy.get("[data-testid=tos-page-title]").should("be.visible");
        cy.get("[data-testid=tos-text-area-component]").should("be.visible");

        cy.get("[data-testid=continue-btn]").should("be.disabled");

        cy.get("[data-testid=accept-tos-checkbox] input")
            .should("be.enabled")
            .check({ force: true });
        cy.get("[data-testid=continue-btn]").should("be.enabled");
    });
});

describe("Does not need to accept terms of service", () => {
    beforeEach(() => {
        setupStandardFixtures();
        fakeLogin("/home");
    });

    it("Validate accept terms of service", () => {
        cy.url().should("include", "/home");
    });
});
