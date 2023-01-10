const { AuthMethod } = require("../../../support/constants");

describe("Immunization History Report", () => {
    let sensitiveDocText =
        " The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off. ";

    beforeEach(() => {
        cy.setupDownloads();
        let isLoading = false;
        cy.enableModules("Immunization");
        cy.intercept("GET", "**/Immunization?*", (req) => {
            if (!isLoading) {
                req.reply({
                    fixture: "ImmunizationService/immunizationrefresh.json",
                });
            } else {
                req.reply({
                    fixture: "ImmunizationService/immunization.json",
                });
            }
            isLoading = !isLoading;
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate Immunization Loading", () => {
        cy.get("[data-testid=reportType]").select("Immunizations");
        cy.get("[data-testid=timelineLoading]").should("be.visible");
        cy.get("[data-testid=timelineLoading]").should("not.be.visible");
    });

    it("Validate Immunization History Report", () => {
        cy.get("[data-testid=reportType]").select("Immunizations");

        cy.get("[data-testid=reportSample]").scrollTo("bottom");

        cy.get("[data-testid=reportSample]").should("be.visible");

        cy.get("[data-testid=immunizationDateTitle]").should("be.visible");
        cy.get("[data-testid=immunizationProviderClinicTitle]").should(
            "be.visible"
        );
        cy.get("[data-testid=immunizationNameTitle]").should("be.visible");
        cy.get("[data-testid=immunizationAgentTitle]").should("be.visible");

        cy.get("[data-testid=immunizationDateItem]")
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);
        cy.get("[data-testid=immunizationNameItem]").should("be.visible");
        cy.get("[data-testid=immunizationProviderClinicItem]").should(
            "be.visible"
        );
        cy.get("[data-testid=immunizationAgentItem]").should("be.visible");

        cy.get("[data-testid=recommendationTitle]").should("be.visible");
        cy.get("[data-testid=recommendationDateTitle]").should("be.visible");

        cy.get("[data-testid=recommendationItem]")
            .scrollIntoView()
            .should("be.visible");
        cy.get("[data-testid=recommendationDateItem]")
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);

        cy.get("[data-testid=exportRecordBtn] button")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=exportRecordBtn] a").first().click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");

        cy.get("[data-testid=genericMessageText]").should(
            "have.text",
            sensitiveDocText
        );

        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });
});

describe("Export Reports - Immunizations - Invalid Doses", () => {
    it("Immunization Report - Partially Vaccinated 1 Valid Dose and 1 Invalid Dose", () => {
        const validDoseDate1 = "2021-Jul-14";
        const invalidDoseDate1 = "2021-Mar-30";

        cy.intercept("GET", "**/Immunization?*", {
            fixture: "ImmunizationService/immunizationInvalidDoses.json",
        });
        cy.enableModules(["Immunization"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );

        cy.get("[data-testid=reportType]").select("Immunizations");

        cy.get("[data-testid=immunizationDateItem]", { timeout: 60000 })
            .contains(validDoseDate1)
            .should("be.visible");
        cy.get("[data-testid=immunizationDateItem]")
            .contains(invalidDoseDate1)
            .should("be.visible");
    });
});
