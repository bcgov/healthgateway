import {
    configureFakeSettings,
    fakeLogin,
    useFakeEnvironment,
} from "../../../support/functions/fakeEnvironment";
import { setupStandardFixtures } from "../../../support/functions/intercept";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();

const homeUrl = "/home";
const timelineUrl = "/timeline";
const otherRecordSourcesUrl = "/otherRecordSources";

describe("Authenticated User - Home Page", () => {
    it("Home Page exists", () => {
        configureFakeSettings({});

        setupStandardFixtures();

        fakeLogin(homeUrl);

        cy.get("[data-testid=health-records-card]").should("be.visible");
    });

    it("Home - Immunization Record Card button enabled", () => {
        configureFakeSettings({
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

        fakeLogin(homeUrl);

        cy.get("[data-testid=immunization-record-card-button]").should(
            "be.visible"
        );
    });

    it("Home - Other Record Sources Card button enabled", () => {
        configureFakeSettings({
            homepage: {
                otherRecordSources: {
                    enabled: true,
                },
            },
        });

        setupStandardFixtures();

        fakeLogin(homeUrl);

        cy.get("[data-testid=other-record-sources-card]")
            .should("be.visible", "be.enabled")
            .click();

        cy.url().should("include", otherRecordSourcesUrl);
    });

    it("Home - Link to timeline page", () => {
        configureFakeSettings({});

        setupStandardFixtures();

        fakeLogin(homeUrl);

        cy.get("[data-testid=health-records-card]")
            .should("be.visible")
            .click();

        cy.url().should("include", timelineUrl);
    });

    it("Home - Immunization Record Card button disabled", () => {
        configureFakeSettings({
            homepage: {
                showImmunizationRecordLink: false,
            },
        });

        setupStandardFixtures();

        fakeLogin(homeUrl);

        cy.get("[data-testid=immunization-record-card-button]").should(
            "not.exist"
        );
    });

    it("Home - Other Record Sources Card button disabled", () => {
        configureFakeSettings({
            homepage: {
                otherRecordSources: {
                    enabled: false,
                },
            },
        });

        setupStandardFixtures();

        fakeLogin(homeUrl);

        cy.get("[data-testid=other-record-sources-card]").should("not.exist");
    });

    it("Home - Notes Card link to Timeline", () => {
        configureFakeSettings({
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

        fakeLogin(homeUrl);

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
        configureFakeSettings({});

        cy.fixture("UserProfileService/userProfile.json").then((data) => {
            const profile = Cypress._.cloneDeep(data);
            profile.lastLoginDateTimes = [
                new Date().toISOString(),
                "2026-05-19T15:59:00Z", // previous login was before May 19, 2026 9:00AM Pacific Time
            ];
            setupStandardFixtures({ userProfileBody: profile });
        });

        fakeLogin(homeUrl);

        cy.get("[data-testid=bc-cancer-notifications-banner]").should(
            "be.visible"
        );
    });

    it("Home - BC Cancer notifications banner hidden if last login is more recent", () => {
        configureFakeSettings({});

        cy.fixture("UserProfileService/userProfile.json").then((data) => {
            const profile = Cypress._.cloneDeep(data);
            profile.lastLoginDateTimes = [
                new Date().toISOString(),
                "2026-05-19T16:01:00Z", // previous login was after May 19, 2026 9:00AM Pacific Time
            ];
            setupStandardFixtures({ userProfileBody: profile });
        });

        fakeLogin(homeUrl);

        cy.contains("#subject", "Home").should("exist");
        cy.get("[data-testid=bc-cancer-notifications-banner]").should(
            "not.exist"
        );
    });

    it("Home - BC Cancer notifications banner hidden for new users", () => {
        configureFakeSettings({});

        cy.fixture("UserProfileService/userProfile.json").then((data) => {
            const profile = Cypress._.cloneDeep(data);
            profile.lastLoginDateTimes = [new Date().toISOString()];
            setupStandardFixtures({ userProfileBody: profile });
        });

        fakeLogin(homeUrl);

        cy.contains("#subject", "Home").should("exist");
        cy.get("[data-testid=bc-cancer-notifications-banner]").should(
            "not.exist"
        );
    });
});
