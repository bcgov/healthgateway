import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const homeUrl = "/home";

function setupImmunizationFixture() {
    cy.intercept("GET", "**/Immunization?hdid=*", {
        fixture: "ImmunizationService/immunization.json",
    });
}

function loginToHome() {
    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak,
        homeUrl
    );
}

describe("Home page - Recommendations", () => {
    function setupRecommendations(standardFixtureOptions = {}) {
        cy.configureSettings({
            homepage: {
                showRecommendationsLink: true,
            },
            datasets: [{ name: "immunization", enabled: true }],
        });
        setupImmunizationFixture();
        setupStandardFixtures(standardFixtureOptions);
    }

    function verifyRecommendationsDialog() {
        cy.get("[data-testid=recommendations-card-button]").click();
        cy.get("[data-testid=recommendations-dialog]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=recommendation-history-report-table]")
                    .should("be.visible")
                    .find("tbody tr")
                    .should("have.length.at.least", 1);
                cy.get(
                    "[data-testid=close-recommendations-dialog-button]"
                ).click();
            });
        cy.get("[data-testid=recommendations-dialog]").should("not.exist");
    }

    it("Home - Removes and restores the Recommendations quick link", () => {
        cy.fixture("UserProfileService/userProfileQuickLinks.json").then(
            (profile) => {
                profile = Cypress._.cloneDeep(profile);
                profile.preferences.hideRecommendationsQuickLink = {
                    hdId: profile.hdId,
                    preference: "hideRecommendationsQuickLink",
                    value: "false",
                    version: 1,
                };
                setupRecommendations({ userProfileBody: profile });
            }
        );

        cy.intercept(
            "PUT",
            "**/UserProfile/*/preference?api-version=2.0",
            (request) => {
                request.alias =
                    request.body.preference === "hideRecommendationsQuickLink"
                        ? "saveRecommendationPreference"
                        : "saveQuickLinks";
                request.reply({
                    statusCode: 200,
                    body: request.body,
                });
            }
        );

        loginToHome();

        cy.get("[data-testid=recommendations-card-button]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=card-menu-button]").click();
            });
        cy.get("[data-testid=remove-quick-link-button]").click();
        cy.wait("@saveRecommendationPreference");
        cy.get("[data-testid=recommendations-card-button]").should("not.exist");

        cy.get("[data-testid=add-quick-link-button]").click();
        cy.get("[data-testid=recommendations-dialog-filter]").click();
        cy.get("[data-testid=add-quick-link-btn]").click();
        cy.wait("@saveQuickLinks");
        cy.wait("@saveRecommendationPreference");
        cy.get("[data-testid=recommendations-card-button]").should(
            "be.visible"
        );
    });

    it("Home - Opens the Recommendations dialog with fixture content", () => {
        setupRecommendations();
        loginToHome();
        verifyRecommendationsDialog();
    });

    it("Home - Opens the Recommendations dialog on mobile", () => {
        setupRecommendations();
        cy.viewport("iphone-6");
        loginToHome();
        verifyRecommendationsDialog();
    });
});
