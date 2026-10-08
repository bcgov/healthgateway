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
        cy.env(["keycloak.password"]).then(
            ({ "keycloak.password": password }) => {
                cy.login(
                    Cypress.expose("keycloak.username"),
                    password,
                    AuthMethod.KeyCloak,
                    profilePath
                );
            }
        );
        cy.url().should("include", profilePath);
    });

    it("Redirect to home", () => {
        cy.env(["keycloak.password"]).then(
            ({ "keycloak.password": password }) => {
                cy.login(
                    Cypress.expose("keycloak.username"),
                    password,
                    AuthMethod.KeyCloak,
                    homePath
                );
            }
        );
        cy.url().should("include", homePath);
    });
});
