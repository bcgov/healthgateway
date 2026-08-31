export function getEntryCardDateString() {
    return cy
        .get("[data-testid=entryCardDate]")
        .first()
        .invoke("text")
        .then((text) => {
            return new Date(text).toISOString().slice(0, 10).replace(/-/g, "_");
        });
}

const maxDeferredLoadAttempts = 10;

export function waitForCovid19Orders(
    alias,
    timeout = 120000,
    attemptsRemaining = maxDeferredLoadAttempts
) {
    return cy.wait(alias, { timeout }).then((interception) => {
        expect(interception.response.statusCode).to.eq(200);

        const payload = interception.response.body.resourcePayload;
        if (!payload.loaded && payload.retryin > 0) {
            if (attemptsRemaining <= 1) {
                throw new Error("COVID-19 orders did not finish loading");
            }
            return waitForCovid19Orders(alias, timeout, attemptsRemaining - 1);
        }

        expect(payload.loaded).to.be.true;
    });
}

export function waitForLaboratoryOrders(
    alias,
    timeout = 120000,
    attemptsRemaining = maxDeferredLoadAttempts
) {
    return cy.wait(alias, { timeout }).then((interception) => {
        const payload = interception.response.body.resourcePayload;
        if (!payload.loaded && payload.retryin > 0) {
            if (attemptsRemaining <= 1) {
                throw new Error("Laboratory orders did not finish loading");
            }
            return waitForLaboratoryOrders(
                alias,
                timeout,
                attemptsRemaining - 1
            );
        }

        expect(payload.loaded).to.be.true;
    });
}

export function waitForImmunizations(
    alias,
    timeout = 120000,
    attemptsRemaining = maxDeferredLoadAttempts
) {
    return cy.wait(alias, { timeout }).then((interception) => {
        expect(interception.response.statusCode).to.eq(200);

        const loadState = interception.response.body.resourcePayload.loadState;
        if (loadState.refreshInProgress) {
            if (attemptsRemaining <= 1) {
                throw new Error("Immunizations did not finish loading");
            }
            return waitForImmunizations(alias, timeout, attemptsRemaining - 1);
        }
    });
}

export function validateAttachmentDownload() {
    getEntryCardDateString().then((dateString) => {
        cy.get("[data-testid=attachment-button]").should("be.visible").click();
        validateSensitiveDocumentDownload(dateString);
    });
}

export function validateFileDownload(buttonSelector, clickEntryCard = true) {
    getEntryCardDateString().then((dateString) => {
        if (clickEntryCard) {
            cy.get("[data-testid=entryCardDetailsTitle]")
                .should("be.visible")
                .click({ force: true });
        }
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
        timeout: 60000,
    });
}
