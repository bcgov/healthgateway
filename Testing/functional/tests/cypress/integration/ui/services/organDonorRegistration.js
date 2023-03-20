const { AuthMethod } = require("../../../support/constants");

describe("Services - Organ Donor Registration Card", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/services"
        );
    });

    it("Should by default handle an unregistered patient", () => {
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

        cy.intercept(
            "GET",
            `**/PatientData/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A*OrganDonorRegistrationStatus*`,
            {
                fixture: "PatientData/donorRegistrationNotRegistered.json",
            }
        );

        cy.get("[data-testid=organ-donor-registration-status]")
            .should("be.visible")
            .contains("Not Registered");

        cy.get("[data-testid=organ-donor-registration-decision-no-file]")
            .should("be.visible")
            .contains("Not Available");
    });

    it("Should handle a Registered patient", () => {
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

        cy.intercept(
            "GET",
            `**/PatientData/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A*OrganDonorRegistrationStatus*`,
            {
                fixture: "PatientData/donorRegistrationRegistered.json",
            }
        );

        cy.get("[data-testid=organ-donor-registration-status]")
            .should("be.visible")
            .contains("Registered");

        cy.get("[data-testid=organ-donor-registration-status-info-button]")
            .should("be.visible")
            .click();

        cy.get("[data-testid=organ-donor-registration-status-info-popover]")
            .should("be.visible")
            .contains(
                "Your decision about organ donation has been registered."
            );

        cy.get("[data-testid=organ-donor-registration-download-button]")
            .should("be.visible")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
    });
});
