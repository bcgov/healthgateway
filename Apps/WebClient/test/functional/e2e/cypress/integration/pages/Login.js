describe("Login Page", () => {
    beforeEach(() => {
        cy.visit("/login");
    });

    it("Validate URL", () => {
        cy.url().should(
            "contains",
            "https://dev.healthgateway.gov.bc.ca/login"
        );
    });

    it("Greeting", () => {
        cy.contains("h3", "Log In");
    });

    it("Menu Login", () => {
        cy.get('[data-testid="loginBtn"]')
            .should("be.visible")
            .should("have.attr", "href", "/login")
            .should("have.text", "Login");
    });
});
