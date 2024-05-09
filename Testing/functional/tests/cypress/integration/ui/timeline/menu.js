import { AuthMethod } from "../../../support/constants";
import {
    CommunicationFixture,
    CommunicationType,
    setupCommunicationIntercept,
    setupPatientIntercept,
    setupUserProfileIntercept,
} from "../../../support/functions/intercept";

function login(isMobile) {
    cy.configureSettings({
        datasets: [
            {
                name: "note",
                enabled: true,
            },
        ],
    });
    if (isMobile) {
        cy.viewport("iphone-6"); // Set viewport to 375px x 667px
    }

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
        AuthMethod.KeyCloak
    );
    cy.checkTimelineHasLoaded();
}

describe("Menu System", () => {
    it("Validate Toggle Sidebar", { scrollBehavior: "top" }, () => {
        login(false);
        cy.get("[data-testid=menu-btn-timeline-link]").should("be.visible");

        cy.get("[data-testid=navbar-toggle-button]").click();
        cy.get("[data-testid=sidenavbar]").should(
            "have.class",
            "v-navigation-drawer--rail"
        );
        cy.get("[data-testid=navbar-toggle-button]").click();
        cy.get("[data-testid=sidenavbar]").should(
            "not.have.class",
            "v-navigation-drawer--rail"
        );
    });

    it("Validate Profile Button for Desktop", () => {
        login(false);
        cy.get("[data-testid=profileButtonInitials]")
            .should("be.visible")
            .should("include.text", "DG");
        cy.get("[data-testid=profileBtn]").should("not.exist");
        cy.get("[data-testid=logoutBtn]").should("not.exist");
        cy.get("[data-testid=headerDropdownBtn]")
            .should("be.visible", "be.enabled")
            .click();
        cy.get("[data-testid=profileUserName]").should("be.visible");
        cy.get("[data-testid=profileBtn]").should("be.visible");
        cy.get("[data-testid=profileBtn]").should(
            "have.attr",
            "href",
            "/profile"
        );
        cy.get("[data-testid=logoutBtn]")
            .should("be.visible")
            .should("include.text", "Log Out");
    });

    it("Validate Profile Button for Mobile", () => {
        login(true);
        cy.get("[data-testid=profileButtonInitials]")
            .should("be.visible")
            .should("include.text", "DG");
        cy.get("[data-testid=profileBtn]").should("not.exist");
        cy.get("[data-testid=logoutBtn]").should("not.exist");
        cy.get("[data-testid=headerDropdownBtn]")
            .should("be.visible", "be.enabled")
            .click();
        cy.get("[data-testid=profileUserName]").should("be.visible");
        cy.get("[data-testid=profileBtn]").should("be.visible", "be.visible");
        cy.get("[data-testid=profileBtn]").should(
            "have.attr",
            "href",
            "/profile"
        );
        cy.get("[data-testid=logoutBtn]")
            .should("be.visible")
            .should("include.text", "Log Out");
    });

    it("Side bar contains nav links", () => {
        login(false);
        cy.get("[data-testid=menu-btn-home-link]").should(
            "have.attr",
            "href",
            "/home"
        );
        cy.get("[data-testid=menu-btn-timeline-link]").should(
            "have.attr",
            "href",
            "/timeline"
        );
        cy.get("[data-testid=menu-btn-reports-link]").should(
            "have.attr",
            "href",
            "/reports"
        );
        cy.get("[data-testid=menu-btn-dependents-link]").should(
            "have.attr",
            "href",
            "/dependents"
        );
        cy.get("[data-testid=navbar-toggle-button]").should("be.visible");
        cy.get("[data-testid=menu-btn-feedback-link]").should("be.visible");
    });

    it("Side bar expands on login for desktop", () => {
        login(true);
        cy.get("[data-testid=sidenavbar]").should("not.be.visible");
    });
});
