import { performSearch } from "../../utilities/supportUtilities";
import { getTableRows } from "../../utilities/sharedUtilities";

const hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const phnWithValidDoses = "9042146714";
const phnWithInvalidDoses = "9735352535";
const phnWithBlockedImmunizations = "9873659643";
const switchName = "Immunization";
const auditBlockReason = "Test block reason";
const auditUnblockReason = "Test unblock reason";

function checkAgentAuditHistory() {
    cy.get("[data-testid=agent-audit-history-title]")
        .should("be.visible")
        .click();

    cy.get("[data-testid=agent-audit-history-table]").should("be.visible");

    return cy.get("[data-testid=agent-audit-history-count]").invoke("text");
}

function validateCovid19InputContainsError(inputId) {
    cy.get(inputId)
        .parent()
        .parent()
        .parent()
        .within(() => {
            cy.get("div").contains("Required").should("be.visible");
        });
}

function validateMailAddressFormCancel() {
    cy.get("[data-testid=mail-button]").click();
    cy.get("[data-testid=address-confirmation-form]").should("be.visible");
    cy.get("[data-testid=address-cancel-button]").click();
    cy.get("[data-testid=address-confirmation-form]").should("not.exist");
}

function validateMailAddressFormRequiredInputs() {
    cy.get("[data-testid=mail-button]").click();
    cy.get("[data-testid=address-confirmation-form]").should("exist");
    cy.get("div").contains("Required").should("not.exist");

    cy.get("[data-testid=address-lines-input]").clear();
    cy.get("[data-testid=city-input]").clear();
    cy.get('[data-testid="country-input"]')
        .parent()
        .within(() => {
            cy.get('button[aria-label="Clear"]').click();
        });
    cy.get("body").click(0, 0);

    cy.get("[data-testid=address-confirmation-button]").click();

    validateCovid19InputContainsError("[data-testid=address-lines-input]");
    validateCovid19InputContainsError("[data-testid=city-input]");
    validateCovid19InputContainsError("[data-testid=province-input]");
    validateCovid19InputContainsError("[data-testid=postal-code-input]");

    cy.get("[data-testid=address-cancel-button]").click();
    cy.get("[data-testid=address-confirmation-form]").should("not.exist");
}

function validateMailAddressFormSubmission() {
    cy.get("[data-testid=mail-button]").click();
    cy.get("[data-testid=address-confirmation-form]").should("be.visible");
    cy.get("[data-testid=address-lines-input]")
        .clear()
        .type("9105 ROTTERDAM PLACE");
    cy.get("[data-testid=city-input]").clear().type("CLITHEROE");
    cy.get("[data-testid=country-input]").clear();
    cy.get(".mud-list .mud-list-item-text").contains("Canada").click();
    cy.get("[data-testid=province-input]").click();
    cy.get("[data-testid=province]").contains("British Columbia").click();
    cy.get("[data-testid=postal-code-input]").clear().type("V3X 4J5");
    cy.get("[data-testid=address-confirmation-button]").click();
    cy.get("[data-testid=address-confirmation-form]").should("not.exist");
}

function validateCovid19TreatmentAssessmentFormBackCancel() {
    cy.get("[data-testid=start-covid-19-treatment-assessment-button]").click();
    cy.url().should("include", "/covid-19-treatment-assessment");
    cy.get("[data-testid=back-button]").click();
    cy.url().should("include", "/patient-details");
    cy.get("[data-testid=start-covid-19-treatment-assessment-button]").click();
    cy.url().should("include", "/covid-19-treatment-assessment");
    cy.scrollTo("bottom", { ensureScrollable: false });
    cy.get("[data-testid=cancel-covid-19-treatment-assessment]").click();
    cy.url().should("include", "/patient-details");
}

function validateCovid19TreatmentAssessmentInfoMessageForRadioSelection() {
    cy.get("[data-testid=start-covid-19-treatment-assessment-button]").click();

    cy.get("[data-testid=assessment-question-1]").within(() => {
        cy.get("[data-testid=assessment-option-yes]").click();
        cy.get("[data-testid=treatment-benefit-not-indicated]").should(
            "not.exist"
        );
        cy.get("[data-testid=assessment-option-no]").click();
        cy.get("[data-testid=treatment-benefit-not-indicated]").should(
            "be.visible"
        );
    });

    cy.get("[data-testid=assessment-question-2]").within(() => {
        cy.get("[data-testid=assessment-option-yes]").click();
        cy.get("[data-testid=treatment-benefit-not-indicated]").should(
            "not.exist"
        );
        cy.get("[data-testid=assessment-option-no]").click();
        cy.get("[data-testid=treatment-benefit-not-indicated]").should(
            "be.visible"
        );
    });

    cy.scrollTo("bottom", { ensureScrollable: false });

    cy.get("[data-testid=assessment-question-6]").within(() => {
        cy.get("[data-testid=assessment-option-yes]").click();
        cy.get("[data-testid=treatment-benefit-indicated]").should(
            "be.visible"
        );
        cy.get("[data-testid=assessment-option-no]").click();
        cy.get(" [data-testid=treatment-benefit-indicated]").should(
            "not.exist"
        );
        cy.get("[data-testid=assessment-option-not-sure]").click();
        cy.get("[data-testid=treatment-benefit-indicated]").should("not.exist");
    });

    cy.get("[data-testid=assessment-question-7]").within(() => {
        cy.get("[data-testid=assessment-option-yes]").click();
        cy.get(" [data-testid=treatment-benefit-indicated]").should(
            "be.visible"
        );
        cy.get("[data-testid=assessment-option-no]").click();
        cy.get("[data-testid=treatment-benefit-indicated]").should("not.exist");
        cy.get("[data-testid=assessment-option-not-sure]").click();
        cy.get("[data-testid=treatment-benefit-indicated]").should("not.exist");
    });

    cy.get("[data-testid=assessment-question-8]").within(() => {
        cy.get("[data-testid=assessment-option-yes]").click();
        cy.get("[data-testid=treatment-benefit-indicated]").should(
            "be.visible"
        );

        cy.get("[data-testid=assessment-option-no]").click();
        cy.get("[data-testid=treatment-benefit-indicated]").should("not.exist");
    });

    cy.get("[data-testid=assessment-question-9]").within(() => {
        cy.get("[data-testid=assessment-option-yes]").click();
        cy.get("[data-testid=treatment-benefit-indicated]").should("not.exist");
        cy.get("[data-testid=assessment-option-no]").click();
        cy.get("[data-testid=treatment-benefit-indicated]").should(
            "be.visible"
        );
        cy.get("[data-testid=assessment-option-not-sure]").click();
        cy.get("[data-testid=treatment-benefit-indicated]").should("not.exist");
    });

    cy.get("[data-testid=cancel-covid-19-treatment-assessment]").click();
    cy.url().should("include", "/patient-details");
}

function validateCovid19TreatmentAssessmentFormRequiredInputs() {
    cy.get("[data-testid=start-covid-19-treatment-assessment-button]").click();
    cy.url().should("include", "/covid-19-treatment-assessment");
    cy.scrollTo("bottom", { ensureScrollable: false });
    cy.get("[data-testid=submit-covid-19-treatment-assessment]").click();

    cy.scrollTo("top", { ensureScrollable: false });
    validateCovid19InputContainsError("[data-testid=phone-number-input]");
    cy.get("[data-testid=assessment-question-]")
        .contains("Do you have a family doctor or nurse practitioner?")
        .parent()
        .within(() => {
            cy.get("div").contains("Required").should("not.exist");
        });
    cy.get("[data-testid=assessment-question-1] div")
        .contains("Required")
        .should("be.visible");
    cy.get("[data-testid=assessment-question-2] div")
        .contains("Required")
        .should("be.visible");
    cy.get("[data-testid=assessment-question-3] div")
        .contains("Required")
        .should("be.visible");
    cy.get("[data-testid=assessment-question-4] div")
        .contains("Required")
        .should("be.visible");

    cy.get("[data-testid=assessment-question-4]").scrollIntoView();
    cy.get("[data-testid=assessment-question-5] div")
        .contains("Required")
        .should("not.exist");
    cy.get("[data-testid=assessment-question-6] div")
        .contains("Required")
        .should("be.visible");
    cy.get("[data-testid=assessment-question-7] div")
        .contains("Required")
        .should("not.exist");

    cy.scrollTo("bottom", { ensureScrollable: false });
    cy.get("[data-testid=assessment-question-8] div")
        .contains("Required")
        .should("not.exist");
    cy.get("[data-testid=assessment-question-9] div")
        .contains("Required")
        .should("not.exist");
    cy.get("[data-testid=assessment-question-10] div")
        .contains("Required")
        .should("be.visible");

    cy.get("[data-testid=cancel-covid-19-treatment-assessment]").click();
    cy.url().should("include", "/patient-details");
}

function validateCovid19TreatmentAssessmentFormSubmission() {
    cy.get("[data-testid=start-covid-19-treatment-assessment-button]").click();
    cy.url().should("include", "/covid-19-treatment-assessment");

    cy.get("[data-testid=phone-number-input]").clear().type("2505556000");

    cy.get("[data-testid=assessment-question-]")
        .contains("Do you have a family doctor or nurse practitioner?")
        .parent()
        .within(() => {
            cy.get(
                "[data-testid=assessment-option-input] [data-testid=assessment-option-yes]"
            ).click();
        });

    cy.get(
        "[data-testid=assessment-question-1] [data-testid=assessment-option-no]"
    ).click();
    cy.get(
        "[data-testid=assessment-question-2] [data-testid=assessment-option-no]"
    ).click();
    cy.get(
        "[data-testid=assessment-question-3] [data-testid=assessment-option-no]"
    ).click();
    cy.get(
        "[data-testid=assessment-question-4] [data-testid=assessment-option-not-sure]"
    ).click();

    cy.scrollTo("bottom", { ensureScrollable: false });

    cy.get(
        '[data-testid=assessment-question-5] button[aria-label="Open Date Picker"]'
    ).click();
    cy.get(
        "[data-testid=assessment-question-5] .mud-picker-container .mud-picker-content .mud-picker-calendar-container .mud-picker-calendar-transition .mud-picker-calendar"
    ).within(() => {
        cy.get(
            "button.mud-current.mud-button-outlined.mud-button-outlined-primary"
        ).click();
    });

    cy.get(
        "[data-testid=assessment-question-6] [data-testid=assessment-option-yes]"
    ).click();
    cy.get(
        "[data-testid=assessment-question-7] [data-testid=assessment-option-yes]"
    ).click();
    cy.get(
        "[data-testid=assessment-question-8] [data-testid=assessment-option-yes]"
    ).click();
    cy.get(
        "[data-testid=assessment-question-9] [data-testid=assessment-option-no]"
    ).click();
    cy.get(
        "[data-testid=assessment-question-10] [data-testid=assessment-option-no]"
    ).click();

    cy.get("[data-testid=notes-input]")
        .clear()
        .type("Test Covid19 Treatment Assessment Note Input.");

    cy.get("[data-testid=submit-covid-19-treatment-assessment]").click();
    cy.get("[data-testid=address-confirmation-form]").should("be.visible");
    cy.get("[data-testid=address-confirmation-button]").click();
    cy.get("[data-testid=address-confirmation-form]").should("not.exist");
    cy.url().should("include", "/patient-details");
}

function validatePrintVaccineCardSubmission() {
    cy.intercept("GET", `**/Document?phn=${phnWithInvalidDoses}`).as(
        "getVaccineCard"
    );
    cy.scrollTo("bottom", { ensureScrollable: false });
    cy.get("[data-testid=print-button]").click();

    cy.wait("@getVaccineCard").then((interception) => {
        cy.verifyDownload("VaccineProof.pdf", {
            timeout: 60000,
            interval: 5000,
        });
    });
}

describe("Patient details page as admin user", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify message verification", () => {
        performSearch("HDID", hdid);

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

    it("Verify dependent section", () => {
        performSearch("HDID", hdid);
        getTableRows("[data-testid=dependent-table]")
            .should("have.length.gt", 1)
            .first()
            .within(() => {
                cy.get("[data-testid=dependent-name]").contains("John Tester");
                cy.get("[data-testid=dependent-dob]").contains("2005-01-01");
                cy.get("[data-testid=dependent-phn]").contains("9874307215");
                cy.get("[data-testid=dependent-address]").should(
                    "not.be.empty"
                );
                cy.get("[data-testid=dependent-expiry-date]")
                    .invoke("text")
                    .should("match", /^\d{4}-\d{2}-\d{2}$/);
                cy.get("[data-testid=dependent-protected-icon]").should(
                    "not.exist"
                );
            });
    });

    it("Verify covid immunization and assessment sections not shown", () => {
        performSearch("PHN", phnWithInvalidDoses);
        cy.get("[data-testid=patient-phn]")
            .should("be.visible")
            .contains(phnWithInvalidDoses);
        cy.scrollTo("bottom", { ensureScrollable: false });
        cy.get("[data-testid=immunization-table]").should("not.exist");
        cy.get("[data-testid=assessment-history-table]").should("not.exist");
    });

    it("Verify block access initial", () => {
        performSearch("HDID", hdid);

        cy.get("[data-testid=block-access-switches]").should("be.visible");
        cy.get(`[data-testid*="block-access-switch"]`).should(
            "not.have.attr",
            "readonly"
        );
        cy.get("[data-testid=block-access-cancel]").should("not.exist");
        cy.get("[data-testid=block-access-save]").should("not.exist");
    });

    it("Verify block access change can be cancelled", () => {
        performSearch("HDID", hdid);

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

    it("Verify block access can be blocked with audit reason.", () => {
        performSearch("HDID", hdid);

        cy.get("[data-testid=block-access-loader]").should("not.be.visible");

        checkAgentAuditHistory().then((presaveCount) => {
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
            checkAgentAuditHistory().then((postsaveCount) => {
                expect(Number(postsaveCount)).to.equal(
                    Number(presaveCount) + 1
                );
            });
        });
    });

    it("Verify block access can be unblocked with audit reason.", () => {
        performSearch("HDID", hdid);

        cy.get("[data-testid=block-access-loader]").should("not.be.visible");

        cy.get(`[data-testid=block-access-switch-${switchName}]`).should(
            "be.checked"
        );

        cy.get(`[data-testid=block-access-switch-${switchName}]`)
            .should("exist")
            .click();

        checkAgentAuditHistory().then((presaveCount) => {
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
            checkAgentAuditHistory().then((postsaveCount) => {
                expect(Number(postsaveCount)).to.equal(
                    Number(presaveCount) + 1
                );
            });
        });
    });
});

describe("Patient details page as reviewer", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_reviewer_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    // verify that the reviewer cannot use the block access controls
    it("Verify block access readonly", () => {
        performSearch("HDID", hdid);

        cy.get(`[data-testid*="block-access-switch-"]`).each(($el) => {
            // follow the mud tag structure to find the mud-readonly class
            cy.wrap($el).parent().parent().should("have.class", "mud-readonly");
        });

        // Clicke any switch and check if the dirty state has exposed the save and cancel buttons
        cy.get(`[data-testid=block-access-switch-${switchName}]`)
            .should("exist")
            .click();

        cy.get("[data-testid=block-access-cancel]").should("not.exist");
        cy.get("[data-testid=block-access-save]").should("not.exist");
    });
});

describe("Patient Details as Support", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_support_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify covid immunization section (not blocked), assessment section and contains invalid dose", () => {
        performSearch("PHN", phnWithInvalidDoses);
        cy.get("[data-testid=patient-phn]")
            .should("be.visible")
            .contains(phnWithInvalidDoses);
        cy.get("[data-testid=patient-hdid]").should("not.exist");

        cy.scrollTo("bottom", { ensureScrollable: false });
        getTableRows("[data-testid=immunization-table]").should(
            "have.length.greaterThan",
            0
        );
        cy.get("[data-testid=invalid-dose-alert").should("be.visible");
        getTableRows("[data-testid=assessment-history-table]").should(
            "have.length.greaterThan",
            0
        );
        cy.get("[data-testid=mail-button]").should("be.visible", "be.enabled");
        cy.get("[data-testid=print-button]").should("be.visible", "be.enabled");
        cy.get(
            "[data-testid=start-covid-19-treatment-assessment-button]"
        ).should("be.visible", "be.enabled");

        validateMailAddressFormCancel();
        validateMailAddressFormRequiredInputs();
        validateCovid19TreatmentAssessmentFormBackCancel();
        validateCovid19TreatmentAssessmentFormRequiredInputs();
        validateCovid19TreatmentAssessmentInfoMessageForRadioSelection();
        validateMailAddressFormSubmission();
        validateCovid19TreatmentAssessmentFormSubmission();
        validatePrintVaccineCardSubmission();
    });

    it("Verify covid immunization section (not blocked), assessment section and contains valid dose", () => {
        performSearch("PHN", phnWithValidDoses);
        cy.get("[data-testid=patient-phn]")
            .should("be.visible")
            .contains(phnWithValidDoses);
        cy.get("[data-testid=patient-hdid]").should("not.exist");

        getTableRows("[data-testid=immunization-table]").should(
            "have.length.greaterThan",
            0
        );
        getTableRows("[data-testid=assessment-history-table]").should(
            "have.length.greaterThan",
            0
        );
        cy.get("[data-testid=invalid-dose-alert").should("not.exist");
    });

    it("Verify covid immunization and assessment sections blocked", () => {
        performSearch("PHN", phnWithBlockedImmunizations);
        cy.get("[data-testid=patient-phn]")
            .should("be.visible")
            .contains(phnWithBlockedImmunizations);
        cy.get("[data-testid=patient-hdid]").should("not.exist");

        getTableRows("[data-testid=immunization-table]").should("not.exist");
        getTableRows("[data-testid=assessment-history-table]").should(
            "not.exist"
        );
        cy.get("[data-testid=blocked-immunization-alert").should("be.visible");
    });

    it("Verify dependent section not shown", () => {
        performSearch("PHN", phnWithValidDoses);
        cy.get("[data-testid=patient-name]").should("be.visible");
        cy.get("[data-testid=dependent-table]").should("not.exist");
    });
});
