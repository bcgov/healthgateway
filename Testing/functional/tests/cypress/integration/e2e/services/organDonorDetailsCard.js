const { AuthMethod } = require("../../../support/constants");

function verifyOrganDonorRegistrationExists(registered) {
    cy.get("[data-testid=organ-donor-registration-card]").should("exist");

    cy.get("[data-testid=card-button-title]").contains(
        "Organ Donor Registration"
    );

    if (registered) {
        cy.get("[data-testid=organ-donor-registration-status]").contains(
            "Registered"
        );

        cy.get("[data-testid=organ-donor-registration-link]").contains(
            "If needed, you can update your decision"
        );

        cy.get("[data-testid=organ-donor-registration-download-button]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.verifyDownload("Organ_Donor_Registration.pdf", {
            timeout: 60000,
            interval: 5000,
        });
    } else {
        cy.get("[data-testid=organ-donor-registration-status]").contains(
            "Not Registered"
        );

        cy.get("[data-testid=organ-donor-registration-link]").contains(
            "Register as an organ donor and save lives"
        );

        cy.get("[data-testid=organ-donor-registration-download-button]").should(
            "not.exist"
        );
    }
}

describe("Organ Donor Details Card Enabled", () => {
    beforeEach(() => {
        cy.configureSettings({
            services: {
                enabled: true,
                services: [
                    {
                        name: "organDonorRegistration",
                        enabled: true,
                    },
                ],
            },
        });
    });

    it("Verify donor registration card is registered and download is successful", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/services"
        );

        verifyOrganDonorRegistrationExists(true);
    });

    it("Verify donor registration card is not registered and download button is not displayed", () => {
        cy.login(
            Cypress.env("keycloak.laboratory.queued.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/services"
        );

        verifyOrganDonorRegistrationExists(false);
    });
});

describe("Organ Donor Details Card Disabled", () => {
    beforeEach(() => {
        cy.configureSettings({
            services: {
                enabled: true,
                services: [
                    {
                        name: "organDonorRegistration",
                        enabled: false,
                    },
                ],
            },
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/services"
        );
    });

    it("Verify donor registration card is disabled", () => {
        cy.get("[data-testid=organ-donor-registration-card]").should(
            "not.exist"
        );
    });
});
