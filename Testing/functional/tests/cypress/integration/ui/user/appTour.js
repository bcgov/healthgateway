import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("App Tour Authenticated", () => {
    beforeEach(() => {
        cy.configureSettings({});
        setupStandardFixtures();
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
    });

    // AB#16927 Disable notifications while aligning Classic with Salesforce version
    it.skip("Header tour button should be visible and have badge when tour changes present", () => {
        cy.get("[data-testid=app-tour-button]")
            .should("be.visible")
            .find("span")
            .should("have.class", "v-badge__badge");

        cy.get("[data-testid=app-tour-button").click();
        cy.get("[data-testid=app-tour-modal]").should("be.visible");

        // Badge should be removed once tour is opened
        cy.get("[data-testid=app-tour-button")
            .find("span.v-badge__badge")
            .should("not.be.visible");
    });

    function keepClickingNext(resolve) {
        const nextButtonId = "[data-testid=app-tour-next]";

        cy.get("[data-testid=app-tour-modal]")
            .wait(1000) // Ensure all animations are complete
            .then((modal) => {
                if (modal.find(nextButtonId).length) {
                    cy.get(nextButtonId).as("nextBtn").should("be.visible");
                    cy.get("@nextBtn").click();
                    keepClickingNext(resolve);
                } else {
                    resolve();
                }
            });
    }

    // AB#16927 Disable notifications while aligning Classic with Salesforce version
    it.skip("User should be able to move through all slides and use done to close modal", () => {
        cy.get("[data-testid=app-tour-button]").should("be.visible").click();
        cy.get("[data-testid=app-tour-modal]").should("be.visible");

        cy.get("[data-testid=app-tour-skip]").should("be.visible").click();
        cy.get("[data-testid=app-tour-modal]").should("not.exist");

        cy.get("[data-testid=app-tour-button]").click();
        cy.get("[data-testid=app-tour-start]")
            .should("be.visible", "be.enabled")
            .click();

        new Cypress.Promise((resolve) => {
            keepClickingNext(resolve);
        }).then(() => {
            // last slide app-tour-done should be visible
            cy.get("[data-testid=app-tour-done]").should("be.visible").click();

            // modal should be closed
            cy.get("[data-testid=app-tour-modal]").should("not.exist");
        });
    });

    // This greatly depends on the tour content, and isn't a great test but will test if the content is there
    // AB#16927 Disable notifications while aligning Classic with Salesforce version
    it.skip("User should be able to navigate forwards and backwards if greater than 2 slides", () => {
        cy.get("[data-testid=app-tour-button]").should("be.visible").click();
        cy.get("[data-testid=app-tour-modal]").should("be.visible");

        // first slide: back should be disabled
        cy.get("[data-testid=app-tour-back]").should("not.exist");

        cy.get("[data-testid=app-tour-start]")
            .should("be.visible", "be.enabled")
            .click();

        // second slide: either back should be enabled or done should be visible
        cy.get("[data-testid=app-tour-modal]")
            .wait(1000) // Wait for animations to complete
            .then((modal) => {
                if (modal.find("[data-testid=app-tour-back]").length) {
                    cy.get("[data-testid=app-tour-back]")
                        .should("be.visible", "be.enabled")
                        .click()
                        .wait(1000); // Wait for animations to complete
                    cy.get("[data-testid=app-tour-start]").should(
                        "be.visible",
                        "be.enabled"
                    );
                } else {
                    cy.get("[data-testid=app-tour-done]").should("be.visible");
                }
            });
    });
});
