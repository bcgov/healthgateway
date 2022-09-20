describe("Landing Page", () => {
    beforeEach(() => {
        cy.logout();
        cy.visit("/");
    });

    it("Title", () => {
        cy.title().should("eq", "Health Gateway");
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
            .should("have.attr", "href", "/login");
    });

    it("Device Previews", () => {
        cy.log("Laptop preview should be displayed by default");
        cy.get("[data-testid=preview-device-button-laptop]").should(
            "be.disabled"
        );
        cy.get("[data-testid=preview-device-button-tablet]").should(
            "not.be.disabled"
        );
        cy.get("[data-testid=preview-device-button-smartphone]").should(
            "not.be.disabled"
        );
        cy.get("[data-testid=preview-image-laptop]").should("be.visible");
        cy.get("[data-testid=preview-image-tablet]").should("not.be.visible");
        cy.get("[data-testid=preview-image-smartphone]").should(
            "not.be.visible"
        );

        cy.log(
            "Tablet preview button should switch the displayed image and enabled buttons"
        );
        cy.get("[data-testid=preview-device-button-tablet]").click();
        cy.get("[data-testid=preview-device-button-laptop]").should(
            "not.be.disabled"
        );
        cy.get("[data-testid=preview-device-button-tablet]").should(
            "be.disabled"
        );
        cy.get("[data-testid=preview-device-button-smartphone]").should(
            "not.be.disabled"
        );
        cy.get("[data-testid=preview-image-laptop]").should("not.be.visible");
        cy.get("[data-testid=preview-image-tablet]").should("be.visible");
        cy.get("[data-testid=preview-image-smartphone]").should(
            "not.be.visible"
        );

        cy.log(
            "Smartphone preview button should switch the displayed image and enabled buttons"
        );
        cy.get("[data-testid=preview-device-button-smartphone]").click();
        cy.get("[data-testid=preview-device-button-laptop]").should(
            "not.be.disabled"
        );
        cy.get("[data-testid=preview-device-button-tablet]").should(
            "not.be.disabled"
        );
        cy.get("[data-testid=preview-device-button-smartphone]").should(
            "be.disabled"
        );
        cy.get("[data-testid=preview-image-laptop]").should("not.be.visible");
        cy.get("[data-testid=preview-image-tablet]").should("not.be.visible");
        cy.get("[data-testid=preview-image-smartphone]").should("be.visible");

        cy.log(
            "Laptop preview button should switch the displayed image and enabled buttons"
        );
        cy.get("[data-testid=preview-device-button-laptop]").click();
        cy.get("[data-testid=preview-device-button-laptop]").should(
            "be.disabled"
        );
        cy.get("[data-testid=preview-device-button-tablet]").should(
            "not.be.disabled"
        );
        cy.get("[data-testid=preview-device-button-smartphone]").should(
            "not.be.disabled"
        );
        cy.get("[data-testid=preview-image-laptop]").should("be.visible");
        cy.get("[data-testid=preview-image-tablet]").should("not.be.visible");
        cy.get("[data-testid=preview-image-smartphone]").should(
            "not.be.visible"
        );
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
                cy.intercept("GET", "**/configuration/", config);
            });
        cy.visit("/");
        cy.get("[data-testid=offlineMessage]").contains(
            "customized offline message"
        );
        cy.get("#btnLogin").should("not.exist");
        cy.get("#menuBtnLogin").should("not.exist");
        cy.get("footer > .navbar").should("not.visible");
    });

    it("Validate active tiles", () => {
        cy.get("[data-testid=active-tile-ClinicalDocument]").should(
            "be.visible"
        );
    });
});
