const { AuthMethod } = require("../../../support/constants");

describe("Validate Star Rating", () => {
    beforeEach(() => {
        cy.configureSettings({});
        cy.env(["keycloak.password"]).then(
            ({ "keycloak.password": password }) => {
                cy.login(
                    Cypress.expose("keycloak.username"),
                    password,
                    AuthMethod.KeyCloak,
                    "/home"
                );
            }
        );
    });

    afterEach(() => {
        Cypress.session.clearAllSavedSessions();
    });

    it("Clicking the 5 star button should log out", () => {
        cy.get("[data-testid=headerDropdownBtn]").click();
        cy.get("[data-testid=logoutBtn]").click();
        cy.get(
            "[data-testid=formRating] > .v-rating__wrapper:last button"
        ).click();
        cy.url().should("include", "/logout");
    });

    it("Clicking Skip button should log out", () => {
        cy.get("[data-testid=headerDropdownBtn]").click();
        cy.get("[data-testid=logoutBtn]").click();
        cy.get("[data-testid=ratingModalSkipBtn]").click();
        cy.url().should("include", "/logout");
    });
});
