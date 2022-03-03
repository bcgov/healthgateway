const { AuthMethod } = require("../../../support/constants");

describe("Reports", () => {
    let sensitiveDocText =
        " The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off. ";
    beforeEach(() => {
        cy.setupDownloads();
        cy.enableModules([
            "Encounter",
            "Medication",
            "Laboratory",
            "Immunization",
            "MedicationRequest",
            "Note",
            "AllLaboratory",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate Service Selection", () => {
        cy.get("[data-testid=exportRecordBtn] button").should(
            "be.disabled",
            "be.visible"
        );

        cy.get("[data-testid=infoText]").should(
            "have.text",
            " Select a record type above to create a report "
        );

        // display visual when no record type selected (mobile and desktop)
        cy.get("[data-testid=infoImage]").should("be.visible");
        cy.viewport("iphone-6");
        cy.get("[data-testid=infoImage]").should("be.visible");
        cy.viewport(1440, 600);

        cy.get("[data-testid=reportType]").select("Medications");

        cy.get("[data-testid=exportRecordBtn] button").should(
            "be.enabled",
            "be.visible"
        );

        cy.get("[data-testid=reportType]").select("");

        cy.get("[data-testid=exportRecordBtn] button").should(
            "be.disabled",
            "be.visible"
        );
    });

    it("Validate Medication Report", () => {
        cy.get("[data-testid=reportType]").select("Medications");
        cy.get("[data-testid=reportSample]").should("be.visible");

        cy.viewport("iphone-6");
        cy.get("[data-testid=reportSample]").should("not.be.visible");
        cy.viewport(1440, 600);

        cy.get("[data-testid=exportRecordBtn] button").click();

        cy.get("[data-testid=exportRecordBtn] a").first().click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");

        cy.get("[data-testid=genericMessageText]").should(
            "have.text",
            sensitiveDocText
        );

        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });

    it("Validate MSP Visits Report", () => {
        cy.get("[data-testid=reportType]").select("Health Visits");

        cy.get("[data-testid=reportSample]").should("be.visible");

        cy.viewport("iphone-6");
        cy.get("[data-testid=reportSample]").should("not.be.visible");
        cy.viewport(1440, 600);

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

    it("Validate COVID-19 Report", () => {
        cy.get("[data-testid=reportType]").select("COVID-19 Test Results");

        cy.get("[data-testid=reportSample]").should("be.visible");
        cy.get("[data-testid=covid19DateItem]", { timeout: 60000 })
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);

        cy.viewport("iphone-6");
        cy.get("[data-testid=reportSample]").should("not.be.visible");
        cy.viewport(1440, 600);

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

    it("Validate Immunization Report", () => {
        cy.get("[data-testid=reportType]").select("Immunizations");

        cy.get("[data-testid=reportSample]").should("be.visible");
        cy.get("[data-testid=immunizationDateItem]", { timeout: 60000 })
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);

        cy.viewport("iphone-6");
        cy.get("[data-testid=reportSample]").should("not.be.visible");
        cy.viewport(1440, 600);

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

    it("Validate Special Authority Report", () => {
        cy.get("[data-testid=reportType]").select("Special Authority Requests");

        cy.get("[data-testid=reportSample]").should("be.visible");

        cy.viewport("iphone-6");
        cy.get("[data-testid=reportSample]").should("not.be.visible");
        cy.viewport(1440, 600);

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

    it("Validate Notes Report", () => {
        cy.get("[data-testid=reportType]").select("My Notes");

        cy.get("[data-testid=reportSample]").should("be.visible");

        cy.viewport("iphone-6");
        cy.get("[data-testid=reportSample]").should("not.be.visible");
        cy.viewport(1440, 600);

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

    it("Validate Laboratory Report", () => {
        cy.get("[data-testid=reportType]").select("Laboratory Tests");

        cy.get("[data-testid=reportSample]").should("be.visible");
        cy.get("[data-testid=labResultDateItem]", { timeout: 60000 })
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);

        cy.viewport("iphone-6");
        cy.get("[data-testid=reportSample]").should("not.be.visible");
        cy.viewport(1440, 600);

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
