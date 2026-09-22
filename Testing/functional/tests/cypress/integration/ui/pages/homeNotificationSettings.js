import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const homeUrl = "/home";
const profileUrl = "/profile";

function loginToHome() {
    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak,
        homeUrl
    );
}

describe("Home page - notification settings alert", () => {
    it("Home - Shows the SMS removed alert once and saves it as dismissed", () => {
        cy.configureSettings({});

        cy.fixture("UserProfileService/userProfile.json").then((profile) => {
            profile = Cypress._.cloneDeep(profile);
            profile.preferences.showSmsRemoved = {
                hdId: profile.hdId,
                preference: "showSmsRemoved",
                value: "true",
                version: 1,
            };
            setupStandardFixtures({ userProfileBody: profile });
        });

        cy.intercept(
            "PUT",
            "**/UserProfile/*/preference?api-version=2.0",
            (request) => {
                expect(request.body).to.include({
                    preference: "showSmsRemoved",
                    value: "false",
                });
                request.reply({ statusCode: 200, body: request.body });
            }
        ).as("dismissSmsRemovedAlert");

        // The modified profile fixture must not share the default cross-spec
        // session with tests that use the same Keycloak account.
        loginToHome();

        cy.get("[data-testid=incomplete-profile-banner]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=sms-removed-message]").should(
                    "be.visible"
                );
                cy.get("[data-testid=unverified-email-sms-message]").should(
                    "not.exist"
                );
            });
        cy.wait("@dismissSmsRemovedAlert");

        cy.get("[data-testid=profile-preferences-link]")
            .should("have.attr", "href", profileUrl)
            .click();
        cy.location("pathname").should("eq", profileUrl);
        cy.get("[data-testid=menu-btn-home-link]").click();
        cy.location("pathname").should("eq", homeUrl);
        cy.get("[data-testid=sms-removed-message]").should("not.exist");
    });
});
