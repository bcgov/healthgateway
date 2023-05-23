import { performSearch } from "../../utilities/supportUtilities";
import { getTableRows } from "../../utilities/sharedUtilities";

const hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const switchName = "Immunization";
const auditBlockReason = "Test block reason";
const auditUnblockReason = "Test unblock reason";

function checkAgentAuditHistory(expectedRows = null) {
    cy.get("[data-testid=agent-audit-history-title]")
        .should("be.visible")
        .click();

    cy.get("[data-testid=agent-audit-history-table]").should("be.visible");

    let historyCount = 0;
    return new Promise((resolve, reject) => {
        cy.get("[data-testid=agent-audit-history-title]")
            .invoke("text")
            .then((text) => {
                const regex = /\((\d+)\)/g;
                const match = regex.exec(text);
                historyCount = Number(match[1]);
                if (expectedRows != null && expectedRows >= 0) {
                    expect(historyCount).to.equal(expectedRows);
                }
                resolve(historyCount);
            });
    });
}

describe("Patient details message verification", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify message verification", () => {
        performSearch("HDID", hdid);

        cy.get("[data-testid=user-table]")
            .find("tbody tr.mud-table-row")
            .first()
            .click();

        cy.get("[data-testid=patient-name]").should("be.visible");
        cy.get("[data-testid=patient-dob]").should("be.visible");
        cy.get("[data-testid=patient-phn]").should("be.visible");
        cy.get("[data-testid=patient-hdid]")
            .should("be.visible")
            .contains(hdid);
        cy.get("[data-testid=patient-physical-address]").should("be.visible");
        cy.get("[data-testid=patient-mailing-address]").should("be.visible");
        cy.get("[data-testid=profile-created-datetime]").should("be.visible");
        cy.get("[data-testid=profile-last-login-datetime]").should(
            "be.visible"
        );
        getTableRows("[data-testid=messaging-verification-table]").should(
            "have.length",
            2
        );
    });

    it("Verify block access initial", () => {
        performSearch("HDID", hdid);
        cy.get("[data-testid=user-table]")
            .find("tbody tr.mud-table-row")
            .first()
            .click();

        cy.get("[data-testid=block-access-switches]").should("be.visible");
        cy.get("[data-testid=block-access-cancel]").should("not.exist");
        cy.get("[data-testid=block-access-save]").should("not.exist");
    });

    it("Verify block access change can be cancelled", () => {
        performSearch("HDID", hdid);
        cy.get("[data-testid=user-table]")
            .find("tbody tr.mud-table-row")
            .first()
            .click();

        cy.get("[data-testid=block-access-loader]").should("not.be.visible");

        cy.get(`[data-testid=block-access-switch-${switchName}]`)
            .should("exist")
            .click();

        cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
            "be.checked"
        );

        cy.get("[data-testid=block-access-save]").should("be.visible");
        cy.get("[data-testid=block-access-cancel]")
            .should("be.visible")
            .click();

        cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
            "not.be.checked"
        );
    });

    it("Verify block access can be blocked with audit reason.", async () => {
        performSearch("HDID", hdid);
        cy.get("[data-testid=user-table]")
            .find("tbody tr.mud-table-row")
            .first()
            .click();

        cy.get("[data-testid=block-access-loader]").should("not.be.visible");

        const presaveCount = await checkAgentAuditHistory();

        cy.get(`[data-testid=block-access-switch-${switchName}]`)
            .should("exist")
            .click();

        cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
            "be.checked"
        );

        cy.get("[data-testid=block-access-cancel]").should(
            "exist",
            "be.visible"
        );
        cy.get("[data-testid=block-access-save]")
            .should("exist", "be.visible")
            .click();

        cy.get("[data-testid=audit-reason-input")
            .should("be.visible")
            .type(auditBlockReason);

        cy.get("[data-testid=audit-confirm-button]")
            .should("be.visible")
            .click({ force: true });

        cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
            "be.checked"
        );

        // Check agent audit history
        await checkAgentAuditHistory(presaveCount + 1);
    });

    it("Verify block access can be unblocked with audit reason.", async () => {
        performSearch("HDID", hdid);
        cy.get("[data-testid=user-table]")
            .find("tbody tr.mud-table-row")
            .first()
            .click();

        cy.get("[data-testid=block-access-loader]").should("not.be.visible");

        cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
            "be.checked"
        );

        cy.get(`[data-testid=block-access-switch-${switchName}]`)
            .should("exist")
            .click();

        const presaveCount = await checkAgentAuditHistory();

        cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
            "not.be.checked"
        );

        cy.get("[data-testid=block-access-cancel]").should(
            "exist",
            "be.visible"
        );
        cy.get("[data-testid=block-access-save]")
            .should("exist", "be.visible")
            .click();

        cy.get("[data-testid=audit-reason-input")
            .should("be.visible")
            .type(auditUnblockReason);

        cy.get("[data-testid=audit-confirm-button]")
            .should("be.visible")
            .click({ force: true });

        cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
            "not.be.checked"
        );

        // Check agent audit history
        await checkAgentAuditHistory(presaveCount + 1);
    });
});
