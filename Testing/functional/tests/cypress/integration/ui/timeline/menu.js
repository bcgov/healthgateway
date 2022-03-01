const { AuthMethod } = require("../../../support/constants");

function login(isMobile) {
    cy.enableModules("Note");
    if (isMobile) {
        cy.viewport("iphone-6"); // Set viewport to 375px x 667px
    }
    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak
    );
    cy.checkTimelineHasLoaded();
}

describe("Menu System", () => {
    beforeEach(() => {});

    it("Validate Toggle Sidebar", () => {
        login(false);
        cy.get("[data-testid=timelineLabel]")
            .should("be.visible")
            .should("have.text", "Timeline");
        cy.get("[data-testid=sidebarToggle]").click();
        cy.get("[data-testid=timelineLabel]").should("not.be.visible");
        cy.get("[data-testid=sidebarToggle]").click();
        cy.get("[data-testid=timelineLabel]").should("be.visible");
    });

    it("Validate Profile Button for Desktop", () => {
        login(false);
        cy.get("[data-testid=sidebarUserName]").should("not.exist");
        cy.get("[data-testid=profileButtonUserName]")
            .should("be.visible")
            .should("include.text", "Dr Gateway");
        cy.get("[data-testid=profileBtn]").should("not.be.visible");
        cy.get("[data-testid=profileDropDownIcon]").should("not.be.visible");
        cy.get("[data-testid=logoutBtn]").should("not.be.visible");
        cy.get("[data-testid=logoutDropDownIcon]").should("not.be.visible");
        cy.get("[data-testid=headerDropdownBtn]")
            .should("be.visible", "be.enabled")
            .click();
        cy.get("[data-testid=profileUserNameMobileOnly]").should("not.exist");
        cy.get("[data-testid=profileBtn]").should("be.visible");
        cy.get("[data-testid=profileDropDownIcon]").should("be.visible");
        cy.get("[data-testid=profileBtn]").should(
            "have.attr",
            "href",
            "/profile"
        );
        cy.get("[data-testid=logoutBtn]")
            .should("be.visible")
            .should("include.text", "Log Out");
        cy.get("[data-testid=logoutDropDownIcon]").should("be.visible");
    });

    it("Validate Profile Button for Mobile", () => {
        login(true);
        cy.get("[data-testid=profileButtonUserName]").should("not.exist");
        cy.get("[data-testid=profileBtn]").should("not.be.visible");
        cy.get("[data-testid=profileDropDownIcon]").should("not.be.visible");
        cy.get("[data-testid=logoutBtn]").should("not.be.visible");
        cy.get("[data-testid=logoutDropDownIcon]").should("not.be.visible");
        cy.get("[data-testid=headerDropdownBtn]")
            .should("be.visible", "be.enabled")
            .click();
        cy.get("[data-testid=profileUserNameMobileOnly]").should("exist");
        cy.get("[data-testid=profileBtn]").should("be.visible", "be.visible");
        cy.get("[data-testid=profileDropDownIcon]").should("be.visible");
        cy.get("[data-testid=profileBtn]").should(
            "have.attr",
            "href",
            "/profile"
        );
        cy.get("[data-testid=logoutBtn]")
            .should("be.visible")
            .should("include.text", "Log Out");
        cy.get("[data-testid=logoutDropDownIcon]").should("be.visible");
    });

    it("Side bar contains nav links", () => {
        login(false);
        cy.get("[data-testid=menu-btn-home-link]").should(
            "have.attr",
            "href",
            "/home"
        );
        cy.get("[data-testid=menu-btn-time-line-link]").should(
            "have.attr",
            "href",
            "/timeline"
        );
        cy.get("[data-testid=menu-btn-health-insights-link]").should(
            "have.attr",
            "href",
            "/healthInsights"
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
        cy.get("[data-testid=sidebarToggle]").should("be.visible");
        cy.get("[data-testid=feedbackContainer]").should("be.visible");
    });

    it("Side bar expands on login for desktop", () => {
        login(true);
        cy.get("[data-testid=timelineLabel]").should("not.be.visible");
    });
});
