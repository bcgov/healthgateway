const { AuthMethod } = require("../../../support/constants");

function toggleServices(enabled = true) {
    cy.configureSettings({
        services: {
            enabled: enabled,
        },
    });
}

function login(isMobile) {
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

describe("Menu System when services is enabled", () => {
    beforeEach(() => {
        toggleServices(true);
    });
    it("Validate Toggle Sidebar", () => {
        login(false);
        cy.get("[data-testid=servicesLabel]")
            .should("be.visible")
            .should("have.text", "Services");
        cy.get("[data-testid=sidebarToggle]").click();
        cy.get("[data-testid=servicesLabel]").should("not.be.visible");
        cy.get("[data-testid=sidebarToggle]").click();
        cy.get("[data-testid=servicesLabel]").should("be.visible");
    });

    it("Side bar contains services nav link", () => {
        login(false);
        cy.get("[data-testid=menu-btn-Services-link]").should(
            "have.attr",
            "href",
            "/services"
        );
        cy.get("[data-testid=sidebarToggle]").should("be.visible");
    });

    it("Side bar expands on login for desktop", () => {
        login(true);
        cy.get("[data-testid=servicesLabel]").should("not.be.visible");
    });
});

describe("Menu system when services is disabled", () => {
    beforeEach(() => {
        toggleServices(false);
    });
    it("Side bar does not contain services nav link when services is disabled", () => {
        cy.configureSettings({
            services: {
                enabled: false,
            },
        });
        login(false);
        cy.get("[data-testid=menu-btn-Services-link]").should("not.be.visible");
        cy.get("[data-testid=sidebarToggle]").should("be.visible");
    });
});
