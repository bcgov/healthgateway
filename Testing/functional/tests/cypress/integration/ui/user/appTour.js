const { AuthMethod } = require("../../../support/constants");

const hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

describe("App Tour Authenticated", () => {
    beforeEach(() => {
        cy.intercept("GET", `**/UserProfile/${hdid}`, {
            fixture: "UserProfileService/userProfile.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
    });

    it("Header tour button should be visible and have badge when tour changes present", () => {
        cy.get("[data-testid=app-tour-button]")
            .should("be.visible")
            .find("span")
            .should("have.class", "b-avatar-badge badge-danger");

        cy.get("[data-testid=app-tour-button").click();

        cy.get("[data-testid=app-tour-modal]").should("be.visible");

        // Badge should be removed once tour is opened
        cy.get("[data-testid=app-tour-button")
            .find("span.b-avatar-badge")
            .should("not.exist");
    });

    function keepClickNext() {
        const nextButtonId = "[data-testid=app-tour-next]";
        cy.get("[data-testid=app-tour-modal]").then((modal) => {
            if (modal.find(nextButtonId).length) {
                cy.get(nextButtonId).should("be.visible").click();
                keepClickNext();
            }
        });
    }

    it("Modal controls should be visible and work (no back)", () => {
        cy.get("[data-testid=app-tour-button]").click();
        cy.get("[data-testid=app-tour-modal]").should("be.visible");

        cy.get("[data-testid=app-tour-back]")
            .should("be.visible")
            .should("be.disabled");

        cy.get("[data-testid=app-tour-skip]").should("be.visible").click();
        cy.get("[data-testid=app-tour-modal]").should("not.exist");

        cy.get("[data-testid=app-tour-button]").click();

        keepClickNext();

        // last slide app-tour-done should be visible
        cy.get("[data-testid=app-tour-done]").should("be.visible").click();

        // modal should be closed
        cy.get("[data-testid=app-tour-modal]").should("not.exist");
    });

    // This greatly depends on the tour content, and isn't a great test but will test if the content is there
    it("Modal controls should be visible and work forwards and backwards", () => {
        cy.get("[data-testid=app-tour-button]").click();
        cy.get("[data-testid=app-tour-modal]").should("be.visible");

        // first slide back should be disabled
        cy.get("[data-testid=app-tour-back]")
            .should("be.visible")
            .should("be.disabled");

        cy.get("[data-testid=app-tour-next]").should("be.visible").click();

        cy.get("[data-testid=app-tour-modal]").then((modal) => {
            if (modal.find("[data-testid=app-tour-back]").length) {
                cy.get("[data-testid=app-tour-back]")
                    .should("be.visible")
                    .click();
            }
            cy.log("No back button found");
        });
    });
});
