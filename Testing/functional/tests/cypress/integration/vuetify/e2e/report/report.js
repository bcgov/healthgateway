const { AuthMethod } = require("../../../../support/constants");

describe("Reports", () => {
    let sensitiveDocText =
        "The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off.";
    beforeEach(() => {
        cy.setupDownloads();
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
                {
                    name: "medication",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
                {
                    name: "specialAuthorityRequest",
                    enabled: true,
                },
                {
                    name: "labResult",
                    enabled: true,
                },
                {
                    name: "hospitalVisit",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate Service Selection", () => {
        cy.get("[data-testid=export-record-btn]").should(
            "be.disabled",
            "be.visible"
        );

        cy.get("[data-testid=info-text]").should(
            "have.text",
            " Select a record type above to create a report "
        );

        // display visual when no record type selected (mobile and desktop)
        cy.get("[data-testid=info-image]").should("be.visible");
        cy.viewport("iphone-6");
        cy.get("[data-testid=info-image]").should("be.visible");
        cy.viewport(1440, 600);

        cy.vSelect("[data-testid=report-type]", "Medications");

        cy.get("[data-testid=export-record-btn]").should(
            "be.enabled",
            "be.visible"
        );
    });

    it("Validate Medication Report", () => {
        cy.vSelect("[data-testid=report-type]", "Medications");
        cy.get("[data-testid=medication-history-report-table]").should(
            "be.visible"
        );

        cy.viewport("iphone-6");
        cy.get("[data-testid=medication-history-report-table]").should(
            "not.be.visible"
        );
        cy.viewport(1440, 600);

        cy.get("[data-testid=export-record-btn]").click();

        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-text]").should(
            "have.text",
            sensitiveDocText
        );

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate MSP Visits Report", () => {
        cy.vSelect("[data-testid=report-type]", "Health Visits");

        cy.get("[data-testid=msp-visits-report-table]").should("be.visible");

        cy.viewport("iphone-6");
        cy.get("[data-testid=msp-visits-report-table]").should(
            "not.be.visible"
        );
        cy.viewport(1440, 600);

        cy.get("[data-testid=export-record-btn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-text]").should(
            "have.text",
            sensitiveDocText
        );

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate COVID-19 Report", () => {
        cy.vSelect("[data-testid=report-type]", "COVID‑19 Tests");

        cy.get("[data-testid=covid19-report-table]").should("be.visible");
        cy.get("[data-testid=covid19DateItem]", { timeout: 60000 })
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);

        cy.viewport("iphone-6");
        cy.get("[data-testid=covid19-report-table]").should("not.be.visible");
        cy.viewport(1440, 600);

        cy.get("[data-testid=export-record-btn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-text]").should(
            "have.text",
            sensitiveDocText
        );

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate Immunization Report", () => {
        cy.vSelect("[data-testid=report-type]", "Immunizations");

        cy.get("[data-testid=immunization-history-report-table]").should(
            "be.visible"
        );
        cy.get("[data-testid=immunizationDateItem]", { timeout: 60000 })
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);

        cy.viewport("iphone-6");
        cy.get("[data-testid=immunization-history-report-table]").should(
            "not.be.visible"
        );
        cy.viewport(1440, 600);

        cy.get("[data-testid=export-record-btn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-text]").should(
            "have.text",
            sensitiveDocText
        );

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate Special Authority Report", () => {
        cy.vSelect("[data-testid=report-type]", "Special Authority");

        cy.get("[data-testid=medication-request-report-table]").should(
            "be.visible"
        );

        cy.viewport("iphone-6");
        cy.get("[data-testid=medication-request-report-table]").should(
            "not.be.visible"
        );
        cy.viewport(1440, 600);

        cy.get("[data-testid=export-record-btn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-text]").should(
            "have.text",
            sensitiveDocText
        );

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate Laboratory Report", () => {
        cy.vSelect("[data-testid=report-type]", "Lab Results");

        cy.get("[data-testid=laboratory-report-table]").should("be.visible");
        cy.get("[data-testid=labResultDateItem]", { timeout: 60000 })
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);

        cy.viewport("iphone-6");
        cy.get("[data-testid=laboratory-report-table]").should(
            "not.be.visible"
        );
        cy.viewport(1440, 600);

        cy.get("[data-testid=export-record-btn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-text]").should(
            "have.text",
            sensitiveDocText
        );

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate Hospital Visits Report", () => {
        cy.vSelect("[data-testid=report-type]", "Hospital Visits");

        cy.get("[data-testid=hospital-visit-report-table]").should(
            "be.visible"
        );
        cy.get("[data-testid=hospital-visit-date]", { timeout: 60000 })
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);

        cy.viewport("iphone-6");
        cy.get("[data-testid=hospital-visit-report-table]").should(
            "not.be.visible"
        );
        cy.viewport(1440, 600);

        cy.get("[data-testid=export-record-btn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-text]").should(
            "have.text",
            sensitiveDocText
        );

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });
});

describe("Reports - Notes", () => {
    let sensitiveDocText =
        "The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off.";
    beforeEach(() => {
        cy.setupDownloads();
        cy.configureSettings({
            datasets: [
                {
                    name: "note",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.hthgtwy20.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate Notes Report", () => {
        cy.vSelect("[data-testid=report-type]", "My Notes");

        cy.get("[data-testid=report-sample]").should("be.visible");

        cy.viewport("iphone-6");
        cy.get("[data-testid=notes-report-table]").should("not.be.visible");
        cy.viewport(1440, 600);

        cy.get("[data-testid=export-record-btn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-text]").should(
            "have.text",
            sensitiveDocText
        );

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });
});
