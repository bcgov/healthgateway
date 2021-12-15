const pageUrls = ["/faq", "/release-notes", "/termsOfService", "/contact-us"];

describe("Breadcrumbs", () => {
    it("Breadcrumbs hidden when logged out", () => {
        for (const url of pageUrls) {
            cy.visit(url);
            cy.get("[data-testid=breadcrumbs]", { timeout: 2500 }).should(
                "not.exist"
            );
        }
    });
});
