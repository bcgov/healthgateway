export function getEntryCardDateString() {
    return cy
        .get("[data-testid=entryCardDate]")
        .first()
        .invoke("text")
        .then((text) => {
            return new Date(text).toISOString().slice(0, 10).replace(/-/g, "_");
        });
}

export function validateAttachmentDownload() {
    getEntryCardDateString().then((dateString) => {
        cy.get("[data-testid=attachment-button]").should("be.visible").click();
        validateSensitiveDocumentDownload(dateString);
    });
}

export function validateFileDownload(buttonSelector) {
    getEntryCardDateString().then((dateString) => {
        cy.get("[data-testid=entryCardDetailsTitle]")
            .should("be.visible")
            .click({ force: true });

        cy.get(buttonSelector).should("be.visible").click();
        validateSensitiveDocumentDownload(dateString);
    });
}

export function validateSensitiveDocumentDownload(
    filename,
    exactMatch = false
) {
    cy.document()
        .find("[data-testid=generic-message-submit-btn]")
        .should("be.visible")
        .click();

    cy.document()
        .find("[data-testid=generic-message-modal]")
        .should("not.exist");

    cy.verifyDownload(filename, {
        contains: !exactMatch,
        interval: 500,
        timeout: 20000,
    });
}
