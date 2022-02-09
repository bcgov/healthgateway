const { AuthMethod } = require("../../../support/constants");

describe("Notes", () => {
    beforeEach(() => {
        cy.enableModules("Note");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Add - Edit - Delete", () => {
        // Add Note
        cy.log("Adding Note.");
        cy.get("[data-testid=addNoteBtn]").click();
        cy.get("[data-testid=noteTitleInput]").type("Note Title!");
        cy.get("[data-testid=noteDateInput] input")
            .focus()
            .clear()
            .type("1950-Jan-01");
        cy.get("[data-testid=noteTextInput]").type("Test");
        cy.get("[data-testid=saveNoteBtn]").click();

        // Edit Note
        cy.log("Editing Note.");
        cy.get("[data-testid=noteMenuBtn]").first().click();
        cy.get("[data-testid=editNoteMenuBtn]").first().click();
        cy.get("[data-testid=noteTitleInput]").clear().type("Test Edit");
        cy.get("[data-testid=saveNoteBtn]").click();

        // Confirm added note - notes are sorted by date in descending order
        cy.get("[data-testid=entryCardDate]")
            .last()
            .should("have.text", "1950-Jan-01");
        cy.get("[data-testid=noteTitle]")
            .last()
            .should("have.text", "Note Title!");

        // Delete Note
        cy.log("Deleting Note.");
        cy.get("[data-testid=noteMenuBtn]").last().click();
        cy.on("window:confirm", (str) => {
            expect(str).to.eq("Are you sure you want to delete this note?");
        });
        cy.get("[data-testid=deleteNoteMenuBtn]").last().click();

        // Confirm edited note
        cy.get("[data-testid=noteTitle]")
            .first()
            .should("have.text", "Test Edit");

        // Confirm deleted note
        cy.get("[data-testid=entryCardDetailsTitle]")
            .last()
            .contains("Note Title!")
            .should("not.exist");
    });
});
