import {
    configureFakeSettings,
    fakeLogin,
    useFakeEnvironment,
} from "../../../support/functions/fakeEnvironment";
import { setupStandardFixtures } from "../../../support/functions/intercept";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();

function toggleServices(enabled = true) {
    configureFakeSettings({
        services: {
            enabled: enabled,
        },
    });
}

function login(isMobile) {
    if (isMobile) {
        cy.viewport("iphone-6"); // Set viewport to 375px x 667px
    }
    fakeLogin("/home");
}

describe("Menu System when services are enabled", () => {
    beforeEach(() => {
        toggleServices(true);
        setupStandardFixtures();
        login(false);
    });

    it("Side bar contains services nav link and toggle validated", () => {
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
});

describe("Menu System when services are enabled on mobile", () => {
    beforeEach(() => {
        toggleServices(true);
        setupStandardFixtures();
        login(true);
    });

    it("Side navigation bar does not expand on mobile", () => {
        cy.get("[data-testid=sidenavbar]").should(($drawer) => {
            expect($drawer[0].getBoundingClientRect().right).to.be.lessThan(0);
        });
    });
});

describe("Menu system when services are disabled", () => {
    beforeEach(() => {
        toggleServices(false);
        setupStandardFixtures();
        login(false);
    });
    it("Side bar does not contain services nav link when services is disabled", () => {
        cy.get("[data-testid=menu-btn-services-link]").should("not.be.visible");
    });
});
