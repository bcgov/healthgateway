const { AuthMethod } = require("../../../support/constants");

describe("Notes", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "note",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Add - Edit - Delete", () => {
        // Add Note
        cy.intercept("POST", "**/Note/*").as("createNote");
        cy.log("Adding Note.");
        cy.get("[data-testid=addNoteBtn]").click();
        cy.get("[data-testid=noteTitleInput]").type("Note Title!");
        cy.get("[data-testid=noteDateInput] input")
            .focus()
            .clear()
            .type("1950-Jan-01");
        cy.get("[data-testid=noteTextInput]").type("Test");
        cy.get("[data-testid=saveNoteBtn]").click();

        // Confirm added note - notes are sorted by date in descending order
        cy.wait("@createNote");
        cy.get("[data-testid=entryCardDate]")
            .last()
            .should("have.text", "1950-Jan-01");
        cy.get("[data-testid=noteTitle]")
            .last()
            .should("have.text", "Note Title!");

        // Edit Note
        cy.intercept("PUT", "**/Note/*").as("updateNote");
        cy.log("Editing Note.");
        cy.get("[data-testid=noteMenuBtn]").first().click();
        cy.get("[data-testid=editNoteMenuBtn]").first().click();
        cy.get("[data-testid=noteTitleInput]").clear().type("Test Edit");
        cy.get("[data-testid=saveNoteBtn]").click();

        // Confirm edited note
        cy.wait("@updateNote");
        cy.get("[data-testid=noteTitle]")
            .first()
            .should("have.text", "Test Edit");

        // Delete Note
        cy.intercept("DELETE", "**/Note/*").as("deleteNote");
        cy.log("Deleting Note.");
        cy.get("[data-testid=noteMenuBtn]").last().click();
        cy.on("window:confirm", (str) => {
            expect(str).to.eq("Are you sure you want to delete this note?");
        });
        cy.get("[data-testid=deleteNoteMenuBtn]").last().click();

        // Confirm deleted note
        cy.wait("@deleteNote");
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
    });
});
