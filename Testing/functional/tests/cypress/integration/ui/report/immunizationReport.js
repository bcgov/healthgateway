import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("Immunization History Report", () => {
    let sensitiveDocText =
        "The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off.";

    beforeEach(() => {
        cy.setupDownloads();
        let isLoading = false;
        cy.configureSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.intercept("GET", "**/Immunization?hdid=*", (req) => {
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

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate Immunization History Report", () => {
        cy.vSelect("[data-testid=report-type]", "Immunizations");

        // Test refresh by checking if skeleton is displayed or not
        cy.get("[data-testid=table-skeleton-loader]").should("be.visible");

        cy.get("[data-testid=table-skeleton-loader]").should("not.exist");

        cy.get("[data-testid=report-sample]").scrollTo("bottom", {
            ensureScrollable: false,
        });

        cy.get("[data-testid=report-sample]").should("be.visible");

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

describe("Export Reports - Immunizations - Invalid Doses", () => {
    it("Immunization Report - Partially Vaccinated 1 Valid Dose and 1 Invalid Dose", () => {
        const validDoseDate1 = "2021-Jul-14";
        const invalidDoseDate1 = "2021-Mar-30";

        cy.intercept("GET", "**/Immunization?hdid=*", {
            fixture: "ImmunizationService/immunizationInvalidDoses.json",
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );

        cy.vSelect("[data-testid=report-type]", "Immunizations");

        cy.get("[data-testid=immunizationDateItem]", { timeout: 60000 })
            .contains(validDoseDate1)
            .should("be.visible");
        cy.get("[data-testid=immunizationDateItem]")
            .contains(invalidDoseDate1)
            .should("be.visible");
    });
});
