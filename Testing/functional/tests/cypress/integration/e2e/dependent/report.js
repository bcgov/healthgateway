const { AuthMethod } = require("../../../support/constants");
import {
    getCardSelector,
    getTabButtonSelector,
} from "../../../support/functions/dependent";

const existingDependent = {
    hdid: "162346565465464564565463257",
    phn: "9874307175",
    dateOfBirth: "2015-Aug-20",
    otherDelegateCount: 0,
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
        const hdid = existingDependent.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] button`).should(
            "be.disabled",
            "be.visible"
        );

        cy.get(`${tabSelector} [data-testid=infoText]`).should("be.visible");

        // display visual when no record type selected (mobile and desktop)
        cy.get(`${tabSelector} [data-testid=infoImage]`).should("be.visible");
        cy.viewport("iphone-6");
        cy.get(`${tabSelector} [data-testid=infoImage]`).should("be.visible");
        cy.viewport(1440, 600);

        cy.get(`${tabSelector} [data-testid=reportType]`).select(
            "COVID‑19 Tests"
        );

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] button`).should(
            "be.enabled",
            "be.visible"
        );

        cy.get(`${tabSelector} [data-testid=reportType]`).select("");

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] button`).should(
            "be.disabled",
            "be.visible"
        );
    });

    it.skip("Validate Medication Report", () => {
        const hdid = existingDependent.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=reportType]`).select("Medications");
        cy.get(
            `${tabSelector} [data-testid=medication-history-report-table]`
        ).should("not.exist");

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] button`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] a`)
            .first()
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");

        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });

    it.skip("Validate MSP Visits Report", () => {
        const hdid = existingDependent.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=reportType]`).select(
            "Health Visits"
        );

        cy.get(`${tabSelector} [data-testid=msp-visits-report-table]`).should(
            "not.exist"
        );

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] button`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] a`)
            .first()
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");

        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });

    it("Validate COVID-19 Report", () => {
        const hdid = existingDependent.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=reportType]`).select(
            "COVID‑19 Tests"
        );

        cy.get(`${tabSelector} [data-testid=covid19-report-table]`).should(
            "not.exist"
        );

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] button`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] a`)
            .first()
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");

        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });

    it("Validate Immunization Report", () => {
        const hdid = existingDependent.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=reportType]`).select(
            "Immunizations"
        );

        cy.get(
            `${tabSelector} [data-testid=immunization-history-report-table]`
        ).should("not.exist");

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] button`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] a`)
            .first()
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");

        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });

    it("Validate Special Authority Report", () => {
        const hdid = existingDependent.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=reportType]`).select(
            "Special Authority"
        );

        cy.get(
            `${tabSelector} [data-testid=medication-request-report-table]`
        ).should("not.exist");

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] button`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] a`)
            .first()
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");

        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });

    it("Validate Laboratory Report", () => {
        const hdid = existingDependent.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=reportType]`).select("Lab Results");

        cy.get(`${tabSelector} [data-testid=laboratory-report-table]`).should(
            "not.exist"
        );

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] button`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] a`)
            .first()
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");

        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });

    it("Validate Hospital Visits Report", () => {
        const hdid = existingDependent.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "report");
        const tabSelector = `${cardSelector} [data-testid=report-tab]`;

        cy.get(tabButtonSelector)
            .should("be.visible")
            .should("not.be.disabled")
            .should("not.have.class", "disabled")
            .click();

        cy.get(`${tabSelector} [data-testid=reportType]`).select(
            "Hospital Visits"
        );

        cy.get(
            `${tabSelector} [data-testid=hospital-visit-report-table]`
        ).should("not.exist");

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] button`)
            .should("be.enabled", "be.visible")
            .click();

        cy.get(`${tabSelector} [data-testid=exportRecordBtn] a`)
            .first()
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");

        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });
});
