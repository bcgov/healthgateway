describe("Landing Page", () => {
    it("Title", () => {
        cy.visit("/");
        cy.title().should("eq", "Health Gateway");
    });

    it("Sign Up Button", () => {
        cy.visit("/");
        cy.get("#btnStart").should("be.visible").contains("Register");
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

    it("Validate What You Can Access Cards", () => {
        cy.visit("/");
        cy.log("Validate what you can access cards.");

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
    });

    it("Validate Managed Health Cards", () => {
        cy.visit("/");
        cy.log("Validate managed health cards.");

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
    beforeEach(() => {
        cy.viewport("iphone-6");
        cy.visit("/");
    });

    it("Mobile - Validate What You Can Access Cards", () => {
        cy.log("Validate mobile what you can access cards.");

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
        cy.get("[data-testid=mobile-read-more-button-HealthServices]").should(
            "be.visible"
        );
    });

    it("Mobile - Validate Explore Trusted Health Services Cards", () => {
        cy.visit("/");
        cy.log("Validate mobile explore trusted health services cards.");

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
