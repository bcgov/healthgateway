export function getEntryCardDateString() {
    return cy
        .get("[data-testid=entryCardDate]")
        .first()
        .invoke("text")
        .then((text) => {
            return new Date(text).toISOString().slice(0, 10).replace(/-/g, "_");
        });
}

const deferredLoadTimeout = 60000;
const maxDeferredLoadAttempts = 3;

export function waitForTimelineCard(timeout = deferredLoadTimeout) {
    return cy
        .get("[data-testid=timelineCard]", { timeout })
        .first()
        .should("be.visible");
}

function waitForLoadedOrders(
    alias,
    errorMessage,
    validateStatusCode,
    timeout = deferredLoadTimeout,
    attemptsRemaining = maxDeferredLoadAttempts
) {
    return cy.wait(alias, { timeout }).then((interception) => {
        if (validateStatusCode) {
            expect(interception.response.statusCode).to.eq(200);
        }

        const payload = interception.response.body.resourcePayload;
        if (!payload.loaded && payload.retryin > 0) {
            if (attemptsRemaining <= 1) {
                throw new Error(errorMessage);
            }
            return waitForLoadedOrders(
                alias,
                errorMessage,
                validateStatusCode,
                timeout,
                attemptsRemaining - 1
            );
        }

        expect(payload.loaded).to.be.true;
    });
}

export function waitForCovid19Orders(alias, timeout = deferredLoadTimeout) {
    return waitForLoadedOrders(
        alias,
        "COVID-19 orders did not finish loading",
        true,
        timeout
    );
}

export function waitForLaboratoryOrders(alias, timeout = deferredLoadTimeout) {
    return waitForLoadedOrders(
        alias,
        "Laboratory orders did not finish loading",
        false,
        timeout
    );
}

function waitForImmunizationResponse(alias, timeout = deferredLoadTimeout) {
    return cy.wait(alias, { timeout }).then((interception) => {
        expect(interception.response.statusCode).to.eq(200);
    });
}

// The ClientApp may reuse cached immunization data instead of making a request.
// Prepare the intercept before the user action, then invoke the returned callback
// afterward to wait only when a request was actually started.
export function prepareImmunizationWait(hdid, timeout = deferredLoadTimeout) {
    const aliasName = "getImmunizationsIfRequested";
    let requestStarted = false;

    cy.intercept("GET", `**/Immunization?hdid=${hdid}`, () => {
        requestStarted = true;
    }).as(aliasName);

    return () =>
        cy.then(() => {
            if (requestStarted) {
                return waitForImmunizationResponse(`@${aliasName}`, timeout);
            }

            cy.log("Using previously loaded immunization data.");
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
