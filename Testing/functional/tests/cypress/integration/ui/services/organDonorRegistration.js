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

        cy.get("[data-testid=generic-message-modal]").should("be.visible");
    });
});

describe("Services - Organ Donor Registration Card - ODR Dataset Blocked", () => {
    beforeEach(() => {
        cy.intercept("GET", `**/UserProfile/*`, {
            fixture:
                "UserProfileService/userProfileOrganDonorRegistrationDatasetBlocked.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/services"
        );
    });

    it("Should not show Organ Donor Registration Card", () => {
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

        cy.get("[data-testid=organ-donor-registration-card]").should(
            "not.exist"
        );

        cy.get("[data-testid=singleErrorHeader")
            .should("be.visible")
            .contains(
                "Organ Donor Registration is not available at this time. Please try again later."
            );
    });
});
