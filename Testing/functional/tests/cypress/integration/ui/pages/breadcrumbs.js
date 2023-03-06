const pageUrls = ["/release-notes", "/termsOfService"];

describe("Breadcrumbs", () => {
    it("Breadcrumbs hidden when logged out", () => {
        cy.logout();
        for (const url of pageUrls) {
            cy.visit(url);
            cy.get("[data-testid=breadcrumbs]", { timeout: 2500 }).should(
                "not.exist"
            );
        }
    });
});
