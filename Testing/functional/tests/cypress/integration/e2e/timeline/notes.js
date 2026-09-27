const { AuthMethod } = require("../../../support/constants");

describe("Notes", () => {
    // Keep ownership across retries without touching another run's notes.
    const notePrefix = `Cypress Notes CRUD ${Date.now()}-${Cypress._.random(1000000)}`;
    const noteTitle = `${notePrefix} Add`;
    const editedNoteTitle = `${notePrefix} Edit`;

    function getNoteCard(title) {
        return cy
            .contains("[data-testid=noteTitle]", title)
            .closest("[data-testid=timelineCard]");
    }

    function removeTestNotesIfPresent() {
        cy.get("body").then(($body) => {
            const title = [noteTitle, editedNoteTitle].find((candidate) =>
                [...$body.find("[data-testid=entryCardDetailsTitle]")].some(
                    (element) => element.textContent.trim() === candidate
                )
            );
            if (!title) {
                return;
            }

            getNoteCard(title).find("[data-testid=noteMenuBtn]").click();
            cy.get("[data-testid=deleteNoteMenuBtn]:visible").click();
            cy.wait("@deleteNote");
            cy.contains("[data-testid=noteTitle]", title).should("not.exist");
            removeTestNotesIfPresent();
        });
    }

    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "note",
                    enabled: true,
                },
            ],
        });
        cy.intercept("GET", "**/Note/*").as("getNotes");
        cy.intercept("DELETE", "**/Note/*").as("deleteNote");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/timeline"
        );
        cy.on("window:confirm", (message) => {
            expect(message).to.eq("Are you sure you want to delete this note?");
            return true;
        });

        cy.wait("@getNotes", { timeout: 60000 }).then(({ response }) => {
            cy.checkTimelineHasLoaded();
            const hasTestNotes = response.body.resourcePayload.some((note) =>
                [noteTitle, editedNoteTitle].includes(note.title)
            );
            if (hasTestNotes) {
                // The filter is absent on an empty timeline. When cleanup is
                // needed, filtering also finds notes beyond the first page.
                cy.get("[data-testid=filterDropdown]").click();
                cy.get("[data-testid=filterTextInput]").type(notePrefix);
                cy.get("[data-testid=btnFilterApply]").click();
                cy.get("[data-testid=btnFilterApply]").should("not.exist");
                removeTestNotesIfPresent();
            }
        });
    });

    it("Validate Add - Edit - Delete", () => {
        // Add Note
        cy.intercept("POST", "**/Note/*").as("createNote");
        cy.log("Adding Note.");
        cy.get("[data-testid=addNoteBtn]").click();
        cy.get("[data-testid=noteTitleInput]").type(noteTitle);
        cy.get("[data-testid=noteDateInput] input")
            .focus()
            .clear()
            .type("1950-Jan-01");
        cy.get("[data-testid=noteTextInput]").type("Test");
        cy.get("[data-testid=saveNoteBtn]").click();

        // Select this run's note rather than relying on timeline ordering.
        cy.wait("@createNote");
        getNoteCard(noteTitle)
            .find("[data-testid=entryCardDate]")
            .should("have.text", "1950-Jan-01");
        getNoteCard(noteTitle)
            .find("[data-testid=entryCardDetailsTitle]")
            .should("have.text", noteTitle);

        // Edit Note
        cy.intercept("PUT", "**/Note/*").as("updateNote");
        cy.log("Editing Note.");
        getNoteCard(noteTitle).find("[data-testid=noteMenuBtn]").click();
        cy.get("[data-testid=editNoteMenuBtn]:visible").click();
        cy.get("[data-testid=noteTitleInput] input")
            .clear()
            .type(editedNoteTitle);
        cy.get("[data-testid=saveNoteBtn]").click();

        // Confirm edited note
        cy.wait("@updateNote");
        getNoteCard(editedNoteTitle)
            .find("[data-testid=entryCardDate]")
            .should("have.text", "1950-Jan-01");
        getNoteCard(editedNoteTitle)
            .find("[data-testid=entryCardDetailsTitle]")
            .should("have.text", editedNoteTitle);

        // Delete Note
        cy.log("Deleting Note.");
        getNoteCard(editedNoteTitle).find("[data-testid=noteMenuBtn]").click();
        cy.get("[data-testid=deleteNoteMenuBtn]:visible").click();

        // Confirm deleted note
        cy.wait("@deleteNote");
        cy.contains("[data-testid=noteTitle]", editedNoteTitle).should(
            "not.exist"
        );
    });
});
