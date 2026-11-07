import {
    configureFakeSettings,
    fakeLogin,
    useFakeEnvironment,
} from "../../../support/functions/fakeEnvironment";
import { setupStandardFixtures } from "../../../support/functions/intercept";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();

describe("Immunization History Report", () => {
    let sensitiveDocText =
        "The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off.";

    beforeEach(() => {
        cy.setupDownloads();
        configureFakeSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.intercept("GET", "**/Immunization?hdid=*", {
            fixture: "ImmunizationService/immunization.json",
        });
        // Any PDF will do for the download; the API wraps it as a Report.
        cy.fixture("LaboratoryService/laboratoryReportPdf.json").then(
            ({ resourcePayload }) => {
                cy.intercept("POST", "**/Report", {
                    statusCode: 200,
                    body: {
                        resourcePayload: {
                            fileName: "HealthGatewayImmunizationReport.pdf",
                            data: resourcePayload.data,
                        },
                        totalResultCount: 1,
                        pageIndex: null,
                        pageSize: null,
                        resultStatus: 1,
                        resultError: null,
                    },
                });
            }
        );

        setupStandardFixtures();

        fakeLogin("/reports");
    });

    it("Validate Immunization History Report", () => {
        cy.vSelect("[data-testid=report-type]", "Immunizations");

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
        configureFakeSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        fakeLogin("/reports");

        cy.vSelect("[data-testid=report-type]", "Immunizations");

        cy.get("[data-testid=immunizationDateItem]", { timeout: 60000 })
            .contains(validDoseDate1)
            .should("be.visible");
        cy.get("[data-testid=immunizationDateItem]")
            .contains(invalidDoseDate1)
            .should("be.visible");
    });
});
