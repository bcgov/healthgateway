const { AuthMethod } = require("../../../support/constants");
import {
    getCardSelector,
    getTabButtonSelector,
} from "../../../support/functions/dependent";

// JENNIFER TESTFOUR
const dependent1 = {
    hdid: "162346565465464564565463257",
};

// MICHAEL TESTERTWO
const dependent2 = {
    hdid: "BNV554213556",
};

// ROMIL SWAN
const specialAuthorityDependent = {
    hdid: "IASGH65211V6WHXKGQDSEJAHYMYR",
};

function testReportSelection(
    cardSelector,
    tabButtonSelector,
    tabSelector,
    reportType,
    reportTableTest
) {
    cy.get(cardSelector)
        .should("be.visible")
        .within(() => {
            cy.get(tabButtonSelector)
                .should("be.visible")
                .should("not.be.disabled")
                .click();

            cy.get(tabSelector).within(() => {
                cy.vSelect("[data-testid=report-type]", reportType);

                reportTableTest();

                cy.get(`[data-testid=export-record-btn]`)
                    .should("be.enabled", "be.visible")
                    .click();
            });
        });
    cy.get(`[data-testid=export-record-menu] .v-list-item`).first().click();

    cy.get("[data-testid=generic-message-modal]").should("be.visible");

    cy.get("[data-testid=generic-message-submit-btn]").click();

    cy.get("[data-testid=generic-message-modal]").should("not.exist");
}

describe("Reports", () => {
    beforeEach(() => {
        cy.setupDownloads();
        cy.configureSettings({
            dependents: {
                enabled: true,
                timelineEnabled: true,
                datasets: [
                    {
                        name: "note",
                        enabled: false,
                    },
                ],
            },
            datasets: [
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
                {
                    name: "healthVisit",
                    enabled: true,
                },
                {
                    name: "hospitalVisit",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
                {
                    name: "labResult",
                    enabled: true,
                },
                {
                    name: "medication",
                    enabled: true,
                },
                {
                    name: "note",
                    enabled: true,
                },
                {
                    name: "specialAuthorityRequest",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
    });

    it("Validate Service Selection - Desktop", () => {
        const hdid = dependent1.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `[data-testid=report-tab]`;

        cy.get(cardSelector)
            .should("be.visible")
            .within(() => {
                cy.get(tabButtonSelector)
                    .should("be.visible")
                    .should("not.be.disabled")
                    .click();

                cy.get(tabSelector).within(() => {
                    cy.get(`[data-testid=export-record-btn]`).should(
                        "be.disabled",
                        "be.visible"
                    );

                    cy.get(`[data-testid=info-text]`).should("be.visible");

                    // display visual when no record type selected
                    cy.get(`[data-testid=info-image]`).should("be.visible");
                    cy.vSelect(`[data-testid=report-type]`, "COVID‑19 Tests");

                    cy.get(`[data-testid=export-record-btn]`).should(
                        "be.enabled",
                        "be.visible"
                    );
                });
            });
    });

    it("Validate Service Selection - Mobile", () => {
        const hdid = dependent1.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `[data-testid=report-tab]`;

        cy.viewport("iphone-6");

        cy.get(cardSelector)
            .should("be.visible")
            .within(() => {
                cy.get(tabButtonSelector)
                    .should("be.visible")
                    .should("not.be.disabled")
                    .click();

                cy.get(tabSelector).within(() => {
                    cy.get(`[data-testid=export-record-btn]`).should(
                        "be.disabled",
                        "be.visible"
                    );

                    cy.get(`[data-testid=info-text]`).should("be.visible");

                    // display visual when no record type selected
                    cy.get(`[data-testid=info-image]`).should("be.visible");
                    cy.vSelect(`[data-testid=report-type]`, "COVID‑19 Tests");

                    cy.get(`[data-testid=export-record-btn]`).should(
                        "be.enabled",
                        "be.visible"
                    );
                });
            });
    });

    it("Validate Medication Report", () => {
        const hdid = dependent2.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `[data-testid=report-tab]`;

        cy.get(cardSelector)
            .should("be.visible")
            .within(() => {
                cy.get(tabButtonSelector)
                    .should("be.visible")
                    .should("not.be.disabled")
                    .click();

                cy.get(tabSelector).within(() => {
                    cy.vSelect("[data-testid=report-type]", "Medications");

                    cy.get(
                        `[data-testid=medication-history-report-table]`
                    ).should("not.exist");

                    cy.get(`[data-testid=export-record-btn]`)
                        .should("be.enabled", "be.visible")
                        .click();
                });
                cy.document()
                    .find(`[data-testid=export-record-menu] .v-list-item`)
                    .first()
                    .click();
            });

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate MSP Visits Report", () => {
        const hdid = dependent2.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `[data-testid=report-tab]`;

        testReportSelection(
            cardSelector,
            tabButtonSelector,
            tabSelector,
            "Health Visits",
            () => {
                cy.get(
                    `${tabSelector} [data-testid=msp-visits-report-table]`
                ).should("not.exist");
            }
        );
    });

    it("Validate COVID-19 Report", () => {
        const hdid = dependent1.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `[data-testid=report-tab]`;

        testReportSelection(
            cardSelector,
            tabButtonSelector,
            tabSelector,
            "COVID‑19 Tests",
            () => {
                cy.get(
                    `${tabSelector} [data-testid=covid19-report-table]`
                ).should("not.exist");
            }
        );
    });

    it("Validate Immunization Report", () => {
        const hdid = dependent1.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `[data-testid=report-tab]`;

        testReportSelection(
            cardSelector,
            tabButtonSelector,
            tabSelector,
            "Immunizations",
            () => {
                cy.get(
                    `${tabSelector} [data-testid=immunization-history-report-table]`
                ).should("not.exist");
            }
        );
    });

    // test should be skipped until PHSA fixes test data for this dependent
    it.skip("Validate Special Authority Report", () => {
        const hdid = specialAuthorityDependent.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `[data-testid=report-tab]`;

        testReportSelection(
            cardSelector,
            tabButtonSelector,
            tabSelector,
            "Special Authority",
            () => {
                cy.get(
                    `${tabSelector} [data-testid=medication-request-report-table]`
                ).should("not.exist");
            }
        );
    });

    // test should be skipped until PHSA fixes test data for this dependent
    it.skip("Validate Laboratory Report", () => {
        const hdid = dependent1.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `[data-testid=report-tab]`;

        testReportSelection(
            cardSelector,
            tabButtonSelector,
            tabSelector,
            "Lab Results",
            () => {
                cy.get(
                    `${tabSelector} [data-testid=laboratory-report-table]`
                ).should("not.exist");
            }
        );
    });

    it("Validate Hospital Visits Report", () => {
        const hdid = dependent1.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `[data-testid=report-tab]`;

        testReportSelection(
            cardSelector,
            tabButtonSelector,
            tabSelector,
            "Hospital Visits",
            () => {
                cy.get(
                    `${tabSelector} [data-testid=hospital-visit-report-table]`
                ).should("not.exist");
            }
        );
    });
});
