const { AuthMethod } = require("../../../../support/constants");
import {
    getCardSelector,
    getTabButtonSelector,
} from "../../../../support/functions/dependent";

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

    it("Validate Service Selection", () => {
        const hdid = dependent1.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=export-record-btn]`).should(
            "be.disabled",
            "be.visible"
        );

        cy.get(`${tabSelector} [data-testid=info-text]`).should("be.visible");

        // display visual when no record type selected (mobile and desktop)
        cy.get(`${tabSelector} [data-testid=info-image]`).should("be.visible");
        cy.viewport("iphone-6");
        cy.get(`${tabSelector} [data-testid=info-image]`).should("be.visible");
        cy.viewport(1440, 600);

        cy.get(`${tabSelector} [data-testid=report-type]`).select(
            "COVID‑19 Tests"
        );

        cy.get(`${tabSelector} [data-testid=export-record-btn]`).should(
            "be.enabled",
            "be.visible"
        );

        cy.get(`${tabSelector} [data-testid=report-type]`).select("");

        cy.get(`${tabSelector} [data-testid=export-record-btn]`).should(
            "be.disabled",
            "be.visible"
        );
    });

    it("Validate Medication Report", () => {
        const hdid = dependent2.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=report-type]`).select("Medications");
        cy.get(
            `${tabSelector} [data-testid=medication-history-report-table]`
        ).should("not.exist");

        cy.get(`${tabSelector} [data-testid=export-record-btn]`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=export-record-btn] .v-list-item`)
            .first()
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate MSP Visits Report", () => {
        const hdid = dependent2.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=report-type]`).select(
            "Health Visits"
        );

        cy.get(`${tabSelector} [data-testid=msp-visits-report-table]`).should(
            "not.exist"
        );

        cy.get(`${tabSelector} [data-testid=export-record-btn]`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=export-record-btn] .v-list-item`)
            .first()
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate COVID-19 Report", () => {
        const hdid = dependent1.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=report-type]`).select(
            "COVID‑19 Tests"
        );

        cy.get(`${tabSelector} [data-testid=covid19-report-table]`).should(
            "not.exist"
        );

        cy.get(`${tabSelector} [data-testid=export-record-btn]`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=export-record-btn] .v-list-item`)
            .first()
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate Immunization Report", () => {
        const hdid = dependent1.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=report-type]`).select(
            "Immunizations"
        );

        cy.get(
            `${tabSelector} [data-testid=immunization-history-report-table]`
        ).should("not.exist");

        cy.get(`${tabSelector} [data-testid=export-record-btn]`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=export-record-btn] .v-list-item`)
            .first()
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate Special Authority Report", () => {
        const hdid = specialAuthorityDependent.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=report-type]`).select(
            "Special Authority"
        );

        cy.get(
            `${tabSelector} [data-testid=medication-request-report-table]`
        ).should("not.exist");

        cy.get(`${tabSelector} [data-testid=export-record-btn]`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=export-record-btn] .v-list-item`)
            .first()
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate Laboratory Report", () => {
        const hdid = dependent1.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=report-type]`).select("Lab Results");

        cy.get(`${tabSelector} [data-testid=laboratory-report-table]`).should(
            "not.exist"
        );

        cy.get(`${tabSelector} [data-testid=export-record-btn]`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=export-record-btn] .v-list-item`)
            .first()
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate Hospital Visits Report", () => {
        const hdid = dependent1.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=report-type]`).select(
            "Hospital Visits"
        );

        cy.get(
            `${tabSelector} [data-testid=hospital-visit-report-table]`
        ).should("not.exist");

        cy.get(`${tabSelector} [data-testid=export-record-btn]`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=export-record-btn] .v-list-item`)
            .first()
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");

        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });
});
