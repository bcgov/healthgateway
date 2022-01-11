const { AuthMethod } = require("../../../support/constants");

describe("Tutorial", () => {
    before(() => {
        cy.intercept("GET", "**/v1/api/UserProfile/*", (req) => {
            req.reply((res) => {
                res.body.resourcePayload.preferences.tutorialMenuNote.value =
                    "true";
                res.body.resourcePayload.preferences.tutorialMenuExport.value =
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
    });

    it("Validate Add Note Popover", () => {
        cy.get("[data-testid=sidebarToggle]").should("be.visible").click();

        cy.get("[data-testid=notesPopover]").should("be.visible");
    });

    it("Validate Export Records Popover", () => {
        cy.get("[data-testid=exportRecordsPopover]").should("be.visible");
    });
});
