describe("Login Page", () => {
    beforeEach(() => {
        cy.logout();
        cy.visit("/login");
    });

    it("Validate URL", () => {
        cy.url().should("include", "login");
    });

    it("Greeting", () => {
        cy.contains("h2", "Log In");
    });
});
