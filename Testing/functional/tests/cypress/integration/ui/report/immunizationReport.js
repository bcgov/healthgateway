const { AuthMethod } = require("../../../support/constants");

describe("Immunization History Report", () => {
    let sensitiveDocText =
        " The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off. ";

    beforeEach(() => {
        cy.setupDownloads();
        let isLoading = false;
        cy.enableModules("Immunization");
        cy.intercept("GET", "**/v1/api/Immunization?*", (req) => {
            req.reply((res) => {
                if (!isLoading) {
                    res.send({
                        fixture: "ImmunizationService/immunizationrefresh.json",
                    });
                } else {
                    res.send({
                        fixture: "ImmunizationService/immunization.json",
                    });
                }
                isLoading = !isLoading;
            });
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
        cy.get("[data-testid=recommendationStatusTitle]").should("be.visible");

        cy.get("[data-testid=recommendationItem]").should("be.visible");
        cy.get("[data-testid=recommendationDateItem]")
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);
        cy.get("[data-testid=recommendationStatusItem]").should("be.visible");

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
