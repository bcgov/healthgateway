describe("Landing Page", () => {
    it("Displays landing page content", () => {
        cy.visit("/");

        cy.title().should("eq", "Health Gateway");
        cy.get("#btnStart").should("be.visible").contains("Register");

        cy.get("[data-testid=access-card-HealthRecords]").should("be.visible");
        cy.get("[data-testid=read-more-button-HealthRecords]")
            .scrollIntoView()
            .should("be.visible");

        cy.get("[data-testid=access-card-DependentRecords]").should(
            "be.visible"
        );
        cy.get("[data-testid=read-more-button-DependentRecords]").should(
            "be.visible"
        );

        cy.get("[data-testid=access-card-RecordsManagement]").should(
            "be.visible"
        );
        cy.get("[data-testid=read-more-button-RecordsManagement]").should(
            "be.visible"
        );

        cy.get("[data-testid=access-card-HealthServices]").should("be.visible");
        cy.get("[data-testid=read-more-button-HealthServices]").should(
            "be.visible"
        );

        cy.get("[data-testid=managed-health-card-HealthLinkBC]")
            .scrollIntoView()
            .should("be.visible");
        cy.get("[data-testid=read-more-button-HealthLinkBC]").should(
            "be.visible"
        );

        cy.get("[data-testid=managed-health-card-Call811]").should(
            "be.visible"
        );
        cy.get("[data-testid=read-more-button-Call811]").should("be.visible");

        cy.get("[data-testid=managed-health-card-FindDoctor]").should(
            "be.visible"
        );
        cy.get("[data-testid=read-more-button-FindDoctor]").should(
            "be.visible"
        );
    });

    it("Login BCSC", () => {
        cy.visit("/");
        cy.get("[data-testid=btnLogin]")
            .should("be.visible")
            .contains("Log in with BC Services Card")
            .click();
        cy.location("pathname", { timeout: 10000 }).should("eq", "/login");
    });

    it("Login Header", () => {
        cy.visit("/");
        cy.get("[data-testid=loginBtn]")
            .should("be.visible")
            .should("have.text", "Log in")
            .click();
        cy.location("pathname", { timeout: 10000 }).should("eq", "/login");
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
                cy.intercept("GET", "**/configuration", config);
            });
        cy.visit("/");
        cy.get("[data-testid=offlineMessage]").contains(
            "customized offline message"
        );
        cy.get("#btnLogin").should("not.exist");
        cy.get("#menuBtnLogin").should("not.exist");
        cy.get("[data-testid=footer]").should("not.exist");
    });
});

describe("Mobile Landing Page", () => {
    it("Displays mobile landing page content", () => {
        cy.viewport("iphone-6");
        cy.visit("/");

        cy.get("[data-testid=mobile-access-card-carousel]")
            .scrollIntoView()
            .should("be.visible");

        cy.get("[data-testid=mobile-access-card-HealthRecords]").should(
            "be.visible"
        );
        cy.get("[data-testid=mobile-read-more-button-HealthRecords]").should(
            "be.visible"
        );
        cy.get('[data-testid="mobile-access-card-carousel"]')
            .find(".v-carousel__controls button")
            .eq(1)
            .click();

        cy.get("[data-testid=mobile-access-card-DependentRecords]").should(
            "be.visible"
        );
        cy.get("[data-testid=mobile-read-more-button-DependentRecords]").should(
            "be.visible"
        );
        cy.get('[data-testid="mobile-access-card-carousel"]')
            .find(".v-carousel__controls button")
            .eq(2)
            .click();

        cy.get("[data-testid=mobile-access-card-RecordsManagement]").should(
            "be.visible"
        );
        cy.get(
            "[data-testid=mobile-read-more-button-RecordsManagement]"
        ).should("be.visible");
        cy.get('[data-testid="mobile-access-card-carousel"]')
            .find(".v-carousel__controls button")
            .eq(3)
            .click();

        cy.get("[data-testid=mobile-read-more-button-HealthServices]").should(
            "be.visible"
        );

        cy.get("[data-testid=mobile-health-services-card-HealthLinkBC]")
            .scrollIntoView()
            .should("be.visible");
        cy.get("[data-testid=mobile-read-more-button-HealthLinkBC]").should(
            "be.visible"
        );
        cy.get('[data-testid="mobile-health-services-card-carousel"]')
            .find(".v-carousel__controls button")
            .eq(1)
            .click();

        cy.get("[data-testid=mobile-health-services-card-Call811]").should(
            "be.visible"
        );
        cy.get("[data-testid=mobile-read-more-button-Call811]").should(
            "be.visible"
        );
        cy.get('[data-testid="mobile-health-services-card-carousel"]')
            .find(".v-carousel__controls button")
            .eq(2)
            .click();

        cy.get("[data-testid=mobile-health-services-card-FindDoctor]").should(
            "be.visible"
        );
        cy.get("[data-testid=mobile-read-more-button-FindDoctor]").should(
            "be.visible"
        );
    });
});
