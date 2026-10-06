const defaultTimeout = 60000;

export function confirmAndVerifyReportDownload() {
    cy.intercept("POST", "**/Report").as("postReport");
    cy.get("[data-testid=generic-message-modal]").should("be.visible");
    cy.get("[data-testid=generic-message-submit-btn]").click();

    cy.wait("@postReport", { timeout: defaultTimeout }).then(({ response }) => {
        expect(response.statusCode).to.eq(200);
        const fileName = response.body.resourcePayload.fileName;
        expect(fileName).to.be.a("string").and.not.be.empty;
        cy.verifyDownload(fileName, {
            timeout: defaultTimeout,
            interval: 5000,
        });
    });
}
