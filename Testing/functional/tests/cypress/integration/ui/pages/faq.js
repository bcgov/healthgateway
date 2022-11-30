describe("FAQ Page", () => {
    beforeEach(() => {
        cy.logout();
    });

    it("Page exists", () => {
        cy.visit("/faq");
        cy.contains("h1", "Frequently Asked Questions");

        cy.get("[data-testid=questionBtn]").its("length").should("be.gte", 1);
    });

    it("Expand question", () => {
        cy.visit("/faq");
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
