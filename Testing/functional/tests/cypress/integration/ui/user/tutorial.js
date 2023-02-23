const { AuthMethod } = require("../../../support/constants");

describe("Tutorial", () => {
    it("Validate Add Note Popover", () => {
        cy.configureSettings({}, "Note");
        cy.intercept("GET", "**/UserProfile/*", (req) => {
            req.reply((res) => {
                res.body.resourcePayload.preferences.tutorialMenuNote.value =
                    "true";
            });
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
        cy.get("[data-testid=notesPopover]").should("be.visible");
    });

    it("Validate Export Records Popover", () => {
        cy.configureSettings({});
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

    it("Validate Add Dependent Popover", () => {
        cy.configureSettings(
            {
                dependents: {
                    enabled: true,
                },
            },
            ["Dependent"]
        );
        cy.intercept("GET", "**/UserProfile/*", (req) => {
            req.reply((res) => {
                res.body.resourcePayload.preferences.tutorialAddDependent = {
                    preference: "tutorialAddDependent",
                    value: "true",
                };
            });
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
        cy.get("[data-testid=add-dependent-tutorial-popover]").should(
            "be.visible"
        );
    });

    it("Validate Add Quick Link Popover", () => {
        cy.intercept("GET", "**/UserProfile/*", (req) => {
            req.reply((res) => {
                res.body.resourcePayload.preferences.tutorialAddQuickLink = {
                    preference: "tutorialAddQuickLink",
                    value: "true",
                };
            });
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.get("[data-testid=add-quick-link-tutorial-popover]").should(
            "be.visible"
        );
    });

    it("Validate Timeline Filter Popover", () => {
        cy.configureSettings({});
        cy.intercept("GET", "**/UserProfile/*", (req) => {
            req.reply((res) => {
                res.body.resourcePayload.preferences.tutorialTimelineFilter = {
                    preference: "tutorialTimelineFilter",
                    value: "true",
                };
            });
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.get("[data-testid=filter-tutorial-popover]").should("be.visible");
    });

    it("Validate Comment Popover", () => {
        cy.configureSettings(
            {
                timeline: {
                    comment: true,
                },
            },
            "Medication"
        );
        cy.intercept("GET", "**/UserProfile/*", (req) => {
            req.reply((res) => {
                res.body.resourcePayload.preferences.tutorialComment = {
                    preference: "tutorialComment",
                    value: "true",
                };
            });
        });
        cy.intercept("GET", "**/MedicationStatement/*", {
            fixture: "MedicationService/medicationStatement.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.log(
            "Comment tutorial should be visible when expanding first timeline card"
        );
        cy.get("[data-testid=comment-tutorial-popover]").should("not.exist");
        cy.contains("[data-testid=entryCardDate]", "2021-Jan-22")
            .first()
            .should("be.visible")
            .parents("[data-testid=timelineCard]")
            .click();
        cy.get("[data-testid=comment-tutorial-popover]").should("exist");

        cy.log(
            "Comment tutorial should not be visible when opening additional timeline cards"
        );
        cy.contains("[data-testid=entryCardDate]", "2021-Jan-22")
            .first()
            .should("be.visible")
            .parents("[data-testid=timelineCard] .entryHeading")
            .find("[data-testid=entryCardDetailsTitle]")
            .click();
        cy.get("[data-testid=comment-tutorial-popover]").should("not.exist");
        cy.contains("[data-testid=entryCardDate]", "2019-Dec-25")
            .first()
            .should("be.visible")
            .parents("[data-testid=timelineCard]")
            .click();
        cy.get("[data-testid=comment-tutorial-popover]").should("not.exist");
    });
});
