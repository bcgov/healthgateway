describe("FAQ Page", () => {
    beforeEach(() => {
        cy.logout();
        cy.visit("/faq");
    });

    it("Page exists", () => {
        cy.contains("h1", "Frequently Asked Questions");

        cy.get("[data-testid=questionBtn]").its("length").should("be.gte", 1);
    });

    it("Expand question", () => {
        cy.get("[data-testid=answerTxt]").should("not.be.visible");

        cy.get("[data-testid=questionBtn]")
            .should("be.visible")
            .first()
            .click();

        cy.get("[data-testid=answerTxt]").should("be.visible");

        cy.get("[data-testid=questionBtn]")
            .should("be.visible")
            .first()
            .click();

        cy.get("[data-testid=answerTxt]").should("not.be.visible");
    });
});
