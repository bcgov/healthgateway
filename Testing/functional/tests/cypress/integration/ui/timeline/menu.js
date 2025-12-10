import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

function login(isMobile) {
    cy.configureSettings({
        dependents: {
            enabled: true,
        },
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

    setupStandardFixtures();

    cy.intercept("GET", "**/Note/*", {
        fixture: "NoteService/notes-test-note.json",
    });

    cy.intercept("GET", "**/UserProfile/*/Dependent", {
        fixture: "UserProfileService/dependent.json",
    });

    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak
    );
    cy.checkTimelineHasLoaded();
}

function verifyNavClick(testId, expectedPath) {
    login(false);
    cy.get(`[data-testid=${testId}]`).should("be.visible").click();
    cy.location("pathname", { timeout: 10000 }).should("eq", expectedPath);
}

describe("Menu System", () => {
    it("Validate Toggle Sidebar", { scrollBehavior: "top" }, () => {
        login(false);
        cy.get("[data-testid=menu-btn-health-records-link]").should(
            "be.visible"
        );

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
        cy.get("[data-testid=logoutBtn]")
            .should("be.visible")
            .should("include.text", "Log Out");
        cy.get("[data-testid=profileBtn]").should("be.visible").click();
        cy.location("pathname", { timeout: 10000 }).should("eq", "/profile");
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
        cy.get("[data-testid=logoutBtn]")
            .should("be.visible")
            .should("include.text", "Log Out");
        cy.get("[data-testid=profileBtn]").should("be.visible").click();
        cy.location("pathname", { timeout: 10000 }).should("eq", "/profile");
    });

    it("Side bar does not expand on login for mobile", () => {
        login(true);
        cy.get("[data-testid=sidenavbar]").should("not.be.visible");
    });

    it("Side bar contains nav items", () => {
        login(false);
        cy.get("[data-testid=menu-btn-home-link]").should(
            "have.attr",
            "href",
            "/home"
        );
        cy.get("[data-testid=menu-btn-health-records-link]").should(
            "have.text",
            "Health Records"
        );
        cy.get("[data-testid=menu-btn-dependents-link]").should(
            "have.text",
            "Dependents"
        );
        cy.get("[data-testid=menu-btn-reports-link]").should(
            "have.text",
            "Download"
        );
        cy.get("[data-testid=navbar-toggle-button]").should("be.visible");
        cy.get("[data-testid=menu-btn-feedback-link]").should("be.visible");
    });

    it("Validate Side bar Timeline link", () => {
        verifyNavClick("menu-btn-health-records-link", "/timeline");
    });

    it("Validate Side bar Dependents link", () => {
        verifyNavClick("menu-btn-dependents-link", "/dependents");
    });

    it("Validate Side bar Export link", () => {
        verifyNavClick("menu-btn-reports-link", "/reports");
    });
});
