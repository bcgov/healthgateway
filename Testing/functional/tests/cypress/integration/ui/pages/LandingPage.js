describe("Landing Page", () => {
    beforeEach(() => {
        cy.enableModules(["VaccinationStatus"]);
        cy.visit("/");
    });

    it("Title", () => {
        cy.title().should("eq", "Health Gateway");
    });

    it("Greeting", () => {
        cy.contains("h2", "Proof of Vaccination");
    });

    it("Sign Up Button", () => {
        cy.get("#btnStart")
            .should("be.visible")
            .should("have.attr", "href", "/registration")
            .contains("Register");
    });

    it("Login Button", () => {
        cy.get("[data-testid=btnLogin]")
            .should("be.visible")
            .parent()
            .should("have.attr", "href", "/login")
            .contains("Log In");
    });

    it("Offline", () => {
        cy.get("[data-testid=offlineMessage]").should("not.exist");
        cy.readConfig()
            .as("config")
            .then((config) => {
                config.webClient["offlineMode"] = {
                    startDateTime: "2021-01-17T12:00:00",
                    endDateTime: "2121-01-21T12:00:00",
                    message: "customized offline message",
                    whitelist: [],
                };
                cy.intercept("GET", "/v1/api/configuration/", config);
            });
        cy.visit("/");
        cy.get("[data-testid=offlineMessage]").contains(
            "customized offline message"
        );
        cy.get("#btnLogin").should("not.exist");
        cy.get("#menuBtnLogin").should("not.exist");
        cy.get("footer > .navbar").should("not.visible");
    });
});
