import { AuthMethod } from "../../../support/constants";
import {
    CommunicationFixture,
    CommunicationType,
    setupCommunicationIntercept,
    setupPatientIntercept,
    setupUserProfileIntercept,
} from "../../../support/functions/intercept";

const homeUrl = "/home";
const covid19Url = "/covid19";
const timelineUrl = "/timeline";

describe("Authenticated User - Home Page", () => {
    it("Home Page exists", () => {
        cy.configureSettings({});

        setupPatientIntercept();
        setupUserProfileIntercept();
        setupCommunicationIntercept();
        setupCommunicationIntercept({
            communicationType: CommunicationType.InApp,
            communicationFixture: CommunicationFixture.InApp,
        });

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

        setupPatientIntercept();
        setupUserProfileIntercept();
        setupCommunicationIntercept();
        setupCommunicationIntercept({
            communicationType: CommunicationType.InApp,
            communicationFixture: CommunicationFixture.InApp,
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

        setupPatientIntercept();
        setupUserProfileIntercept();
        setupCommunicationIntercept();
        setupCommunicationIntercept({
            communicationType: CommunicationType.InApp,
            communicationFixture: CommunicationFixture.InApp,
        });

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

        setupPatientIntercept();
        setupUserProfileIntercept();
        setupCommunicationIntercept();
        setupCommunicationIntercept({
            communicationType: CommunicationType.InApp,
            communicationFixture: CommunicationFixture.InApp,
        });

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

        setupPatientIntercept();
        setupUserProfileIntercept();
        setupCommunicationIntercept();
        setupCommunicationIntercept({
            communicationType: CommunicationType.InApp,
            communicationFixture: CommunicationFixture.InApp,
        });

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
                    name: "note",
                    enabled: true,
                },
            ],
        });

        setupPatientIntercept();
        setupUserProfileIntercept({
            userProfileFixture: "UserProfileService/userProfileQuickLinks.json",
        });
        setupCommunicationIntercept();
        setupCommunicationIntercept({
            communicationType: CommunicationType.InApp,
            communicationFixture: CommunicationFixture.InApp,
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
});
