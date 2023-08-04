const { AuthMethod } = require("../../../../support/constants");
const homeUrl = "/home";
const covid19Url = "/covid19";
const timelineUrl = "/timeline";

describe("Authenticated User - Home Page", () => {
    it("Home Page exists", () => {
        cy.configureSettings({});
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=bc-vaccine-card-card]").should("be.visible");
        cy.get("[data-testid=health-records-card]").should("be.visible");
    });

    it("Home - Federal Card button enabled", () => {
        cy.configureSettings({
            homepage: {
                showFederalProofOfVaccination: true,
            },
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=proof-vaccination-card-btn]").should("be.visible");
    });

    it("Home - Link to COVID-19 page", () => {
        cy.configureSettings({});
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=bc-vaccine-card-card]")
            .should("be.visible")
            .click();

        cy.url().should("include", covid19Url);
    });

    it("Home - Link to timeline page", () => {
        cy.configureSettings({});
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

    it("Home - Federal Card button disabled", () => {
        cy.configureSettings({});

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=proof-vaccination-card-btn]").should("not.exist");
    });

    it("Home - Notes Card link to Timeline", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
                {
                    name: "labResult",
                    enabled: true,
                },
                {
                    name: "healthVisit",
                    enabled: true,
                },
                {
                    name: "hospitalVisit",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
                {
                    name: "medication",
                    enabled: true,
                },
                {
                    name: "specialAuthorityRequest",
                    enabled: true,
                },
                {
                    name: "note",
                    enabled: true,
                },
            ],
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

        cy.get("[data-testid=loading-toast]").should("exist");
        cy.url().should("include", timelineUrl);
        // Notes has 0 records and will return quickly so content placeholders will not have enough time to display.
        cy.get("[data-testid=content-placeholders]").should("not.exist");
        cy.get("[data-testid=loading-toast]").should("exist");
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
        cy.get("[data-testid=timeline-record-count]").should("not.exist");
        cy.get("[data-testid=loading-toast]").should("exist");
    });
});
