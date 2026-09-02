describe("Login Page", () => {
    beforeEach(() => {
        // These tests only validate the local login page. Clearing browser state
        // avoids an unnecessary request to the external Keycloak logout endpoint.
        cy.clearCookies();
        cy.clearLocalStorage();
        cy.clearAllSessionStorage();
        cy.visit("/login");
    });

    it("Validate URL", () => {
        cy.url().should("include", "login");
    });

    it("Greeting", () => {
        cy.contains("h2", "Log In");
    });
});
