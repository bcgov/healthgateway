import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const profilePath = "/profile";
const homePath = "/home";

describe("Bookmark", () => {
    beforeEach(() => {
        cy.configureSettings({
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
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            profilePath
        );
        cy.url().should("include", profilePath);
    });

    it("Redirect to home", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );
        cy.url().should("include", homePath);
    });
});
