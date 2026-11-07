import {
    configureFakeSettings,
    fakeLogin,
    useFakeEnvironment,
} from "../../../support/functions/fakeEnvironment";
import { setupStandardFixtures } from "../../../support/functions/intercept";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();

const profilePath = "/profile";
const homePath = "/home";

describe("Bookmark", () => {
    beforeEach(() => {
        configureFakeSettings({
            timeline: {
                comment: true,
            },
            datasets: [
                {
                    name: "medication",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();
    });

    it("Redirect to UserProfile", () => {
        fakeLogin(profilePath);
        cy.url().should("include", profilePath);
    });

    it("Redirect to home", () => {
        fakeLogin(homePath);
        cy.url().should("include", homePath);
    });
});
