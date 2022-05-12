const { AuthMethod } = require("../../../support/constants");

describe("Tutorial", () => {
    it("Validate Add Note Popover", () => {
        cy.intercept("GET", "**/UserProfile/*", (req) => {
            req.reply((res) => {
                res.body.resourcePayload.preferences.tutorialMenuNote.value =
                    "true";
            });
        });
        cy.enableModules("Note");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
        cy.get("[data-testid=notesPopover]").should("be.visible");
    });

    it("Validate Export Records Popover", () => {
        cy.intercept("GET", "**/UserProfile/*", (req) => {
            req.reply((res) => {
                res.body.resourcePayload.preferences.tutorialMenuExport.value =
                    "true";
            });
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.get("[data-testid=exportRecordsPopover]").should("be.visible");
    });
});
