import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

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
        AuthMethod.KeyCloak,
        "/home"
    );
}

describe("Menu System when services are enabled", () => {
    beforeEach(() => {
        toggleServices(true);

        setupStandardFixtures();
    });

    it("Side bar contains services nav link and toggle validated", () => {
        login(false);

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

        cy.get("[data-testid=menu-btn-services-link]")
            .should("be.visible")
            .click();
        cy.location("pathname", { timeout: 10000 }).should("eq", "/services");
    });

    it("Side navigation bar does not expand on mobile", () => {
        login(true);
        cy.get("[data-testid=sidenavbar]").should("not.be.visible");
    });
});

describe("Menu system when services are disabled", () => {
    beforeEach(() => {
        toggleServices(false);

        setupStandardFixtures();
    });
    it("Side bar does not contain services nav link when services is disabled", () => {
        cy.configureSettings({
            services: {
                enabled: false,
            },
        });
        login(false);
        cy.get("[data-testid=menu-btn-services-link]").should("not.be.visible");
    });
});
