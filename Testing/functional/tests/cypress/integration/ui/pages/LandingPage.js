describe("Landing Page", () => {
    it("Title", () => {
        cy.visit("/");
        cy.title().should("eq", "Health Gateway");
    });

    it("Sign Up Button", () => {
        cy.visit("/");
        cy.get("#btnStart")
            .should("be.visible")
            .should("have.attr", "href", "/registration")
            .contains("Register");
    });

    it("Login Button", () => {
        cy.visit("/");
        cy.get("[data-testid=btnLogin]")
            .should("be.visible")
            .should("have.attr", "href", "/login");
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

    it.skip("Device Previews", () => {
        cy.visit("/");
        cy.log("Laptop preview should be displayed by default");
        cy.get("[data-testid=preview-device-button-laptop]").should(
            "have.class",
            "bg-grey-lighten-3"
        );
        cy.get("[data-testid=preview-device-button-tablet]").should(
            "not.have.class",
            "bg-grey-lighten-3"
        );
        cy.get("[data-testid=preview-device-button-smartphone]").should(
            "not.have.class",
            "bg-grey-lighten-3"
        );
        cy.get("[data-testid=preview-image-laptop]")
            .scrollIntoView()
            .should("be.visible");
        cy.get("[data-testid=preview-image-tablet]").should("not.be.visible");
        cy.get("[data-testid=preview-image-smartphone]").should(
            "not.be.visible"
        );

        cy.log(
            "Tablet preview button should switch the displayed image and enabled buttons"
        );
        cy.get("[data-testid=preview-device-button-tablet]").click();
        cy.get("[data-testid=preview-device-button-laptop]").should(
            "not.have.class",
            "bg-grey-lighten-3"
        );
        cy.get("[data-testid=preview-device-button-tablet]").should(
            "have.class",
            "bg-grey-lighten-3"
        );
        cy.get("[data-testid=preview-device-button-smartphone]").should(
            "not.have.class",
            "bg-grey-lighten-3"
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
            "not.have.class",
            "bg-grey-lighten-3"
        );
        cy.get("[data-testid=preview-device-button-tablet]").should(
            "not.have.class",
            "bg-grey-lighten-3"
        );
        cy.get("[data-testid=preview-device-button-smartphone]").should(
            "have.class",
            "bg-grey-lighten-3"
        );
        cy.get("[data-testid=preview-image-laptop]").should("not.be.visible");
        cy.get("[data-testid=preview-image-tablet]").should("not.be.visible");
        cy.get("[data-testid=preview-image-smartphone]").should("be.visible");

        cy.log(
            "Laptop preview button should switch the displayed image and enabled buttons"
        );
        cy.get("[data-testid=preview-device-button-laptop]").click();
        cy.get("[data-testid=preview-device-button-laptop]").should(
            "have.class",
            "bg-grey-lighten-3"
        );
        cy.get("[data-testid=preview-device-button-tablet]").should(
            "not.have.class",
            "bg-grey-lighten-3"
        );
        cy.get("[data-testid=preview-device-button-smartphone]").should(
            "not.have.class",
            "bg-grey-lighten-3"
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

    it.skip("Validate clinical doc tile when dataset enabled", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
            ],
        });
        cy.visit("/");
        cy.get("[data-testid=active-dataset-tile-ClinicalDocument]").should(
            "be.visible"
        );
    });

    it.skip("Validate no clinical doc tile when dataset disabled", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "medication",
                    enabled: true,
                },
            ],
        });
        cy.visit("/");
        cy.get("[data-testid=active-dataset-tiles-header").should("be.visible");
        cy.get("[data-testid=active-service-tiles-header").should("not.exist");
        cy.get("[data-testid=active-dataset-tile-Medication]").should(
            "be.visible"
        );
        cy.get("[data-testid=active-dataset-tile-ClinicalDocument]").should(
            "not.exist"
        );
    });

    it.skip("Validate proof of vaccination tile when setting enabled", () => {
        cy.configureSettings({
            covid19: {
                publicCovid19: {
                    showFederalProofOfVaccination: true,
                },
            },
        });
        cy.visit("/");
        cy.get("[data-testid=active-service-tiles-header").should("be.visible");
        cy.get("[data-testid=active-dataset-tiles-header").should("not.exist");
        cy.get("[data-testid=active-service-tile-ProofOfVaccination]").should(
            "be.visible"
        );
    });

    it.skip("Validate no proof of vaccination tile when setting not enabled", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "medication",
                    enabled: true,
                },
            ],
        });
        cy.visit("/");
        cy.get("[data-testid=active-dataset-tiles-header").should("be.visible");
        cy.get("[data-testid=active-dataset-tile-Medication]").should(
            "be.visible"
        );
        cy.get("[data-testid=active-service-tile-ProofOfVaccination]").should(
            "not.exist"
        );
    });

    it("Should not show dataset access and services section if no active datasets", () => {
        cy.configureSettings({});
        cy.visit("/");
        cy.get("[data-testid=active-dataset-tiles-header").should("not.exist");
        cy.get("[data-testid=active-service-tiles-header").should("not.exist");
    });

    it.skip("Should show both sections and tiles if configuration is correct", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "medication",
                    enabled: true,
                },
            ],
            covid19: {
                publicCovid19: {
                    showFederalProofOfVaccination: true,
                },
            },
        });
        cy.visit("/");
        cy.get("[data-testid=active-dataset-tiles-header").should("be.visible");
        cy.get("[data-testid=active-service-tiles-header").should("be.visible");
        cy.get("[data-testid=active-dataset-tile-Medication]").should(
            "be.visible"
        );
        cy.get("[data-testid=active-service-tile-ProofOfVaccination]").should(
            "be.visible"
        );
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
