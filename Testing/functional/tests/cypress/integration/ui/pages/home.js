import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const homeUrl = "/home";
const timelineUrl = "/timeline";
const otherRecordSourcesUrl = "/otherRecordSources";
const profileUrl = "/profile";
const reportsUrl = "/reports";

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

describe("Authenticated User - Home Page", () => {
    it("Home Page exists", () => {
        cy.configureSettings({});

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=health-records-card]").should("be.visible");
    });

    it("Home - Immunization Record Card button enabled", () => {
        cy.configureSettings({
            homepage: {
                showImmunizationRecordLink: true,
            },
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=immunization-record-card-button]").should(
            "be.visible"
        );
    });

    it("Home - Immunization Record Card links to the immunization report", () => {
        cy.configureSettings({
            homepage: {
                showImmunizationRecordLink: true,
            },
            datasets: [{ name: "immunization", enabled: true }],
        });
        setupImmunizationFixture();
        setupStandardFixtures();
        loginToHome();

        cy.get("[data-testid=immunization-record-card-button]")
            .should("be.visible")
            .click();

        cy.location("pathname").should("eq", reportsUrl);
        cy.get("[data-testid=immunization-history-report-table]").should(
            "be.visible"
        );
        cy.get("[data-testid=immunizationDateItem]")
            .last()
            .contains(/\d{4}-[A-Z][a-z]{2}-\d{2}/);
    });

    it("Home - Other Record Sources Card button enabled", () => {
        cy.configureSettings({
            homepage: {
                otherRecordSources: {
                    enabled: true,
                },
            },
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=other-record-sources-card]")
            .should("be.visible", "be.enabled")
            .click();

        cy.url().should("include", otherRecordSourcesUrl);
    });

    it("Home - Link to timeline page", () => {
        cy.configureSettings({});

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=health-records-card]")
            .should("be.visible")
            .click();

        cy.url().should("include", timelineUrl);
    });

    it("Home - Immunization Record Card button disabled", () => {
        cy.configureSettings({
            homepage: {
                showImmunizationRecordLink: false,
            },
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=immunization-record-card-button]").should(
            "not.exist"
        );
    });

    it("Home - Other Record Sources Card button disabled", () => {
        cy.configureSettings({
            homepage: {
                otherRecordSources: {
                    enabled: false,
                },
            },
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=other-record-sources-card]").should("not.exist");
    });

    it("Home - Notes Card link to Timeline", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "note",
                    enabled: true,
                },
            ],
        });
        setupStandardFixtures({
            userProfileFixture: "UserProfileService/userProfileQuickLinks.json",
        });

        cy.intercept("GET", "**/Note/*", {
            fixture: "NoteService/notes-no-records.json",
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.contains("[data-testid=card-button-title]", "My Notes")
            .parents("[data-testid=quick-link-card]")
            .should("be.visible", "be.enabled")
            .click();

        cy.url().should("include", timelineUrl);
        // Notes has 0 records and will return quickly so content placeholders will not have enough time to display.
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
        cy.get("[data-testid=timeline-record-count]").should("not.exist");
    });

    it("Home - BC Cancer notifications banner shown if last login predates notifications implementation", () => {
        cy.configureSettings({});

        cy.fixture("UserProfileService/userProfile.json").then((data) => {
            data.lastLoginDateTimes = [
                new Date().toISOString(),
                "2026-05-19T15:59:00Z", // previous login was before May 19, 2026 9:00AM Pacific Time
            ];
            setupStandardFixtures({ userProfileBody: data });
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=bc-cancer-notifications-banner]").should(
            "be.visible"
        );
    });

    it("Home - BC Cancer notifications banner hidden if last login is more recent", () => {
        cy.configureSettings({});

        cy.fixture("UserProfileService/userProfile.json").then((data) => {
            data.lastLoginDateTimes = [
                new Date().toISOString(),
                "2026-05-19T16:01:00Z", // previous login was after May 19, 2026 9:00AM Pacific Time
            ];
            setupStandardFixtures({ userProfileBody: data });
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.contains("#subject", "Home").should("exist");
        cy.get("[data-testid=bc-cancer-notifications-banner]").should(
            "not.exist"
        );
    });

    it("Home - BC Cancer notifications banner hidden for new users", () => {
        cy.configureSettings({});

        cy.fixture("UserProfileService/userProfile.json").then((data) => {
            data.lastLoginDateTimes = [new Date().toISOString()];
            setupStandardFixtures({ userProfileBody: data });
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.contains("#subject", "Home").should("exist");
        cy.get("[data-testid=bc-cancer-notifications-banner]").should(
            "not.exist"
        );
    });
});

describe("Home page - notification settings alert", () => {
    it("shows the SMS removed alert once and saves it as dismissed", () => {
        cy.configureSettings({});

        cy.fixture("UserProfileService/userProfile.json").then((profile) => {
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

        cy.get("[data-testid=profile-preferences-link]").click();
        cy.location("pathname").should("eq", profileUrl);
        cy.get("[data-testid=menu-btn-home-link]").click();
        cy.location("pathname").should("eq", homeUrl);
        cy.get("[data-testid=sms-removed-message]").should("not.exist");
    });
});

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

    it("removes and restores the Recommendations quick link", () => {
        cy.fixture("UserProfileService/userProfileQuickLinks.json").then(
            (profile) => {
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

    it("opens the Recommendations dialog with fixture content", () => {
        setupRecommendations();
        loginToHome();
        verifyRecommendationsDialog();
    });

    it("opens the Recommendations dialog on mobile", () => {
        setupRecommendations();
        cy.viewport("iphone-6");
        loginToHome();
        verifyRecommendationsDialog();
    });
});
