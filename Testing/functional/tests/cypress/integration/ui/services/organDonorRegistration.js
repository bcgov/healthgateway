import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("Services - Organ Donor Registration Card", () => {
    beforeEach(() => {
        setupStandardFixtures();
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

    it("Verify unregistered patient", () => {
        cy.intercept(
            "GET",
            `**/PatientData/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A*OrganDonorRegistrationStatus*`,
            {
                fixture: "PatientData/donorRegistrationNotRegistered.json",
            }
        );

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/services"
        );

        cy.get("[data-testid=organ-donor-registration-status]")
            .should("be.visible")
            .contains("Not Registered");

        cy.get("[data-testid=organ-donor-registration-decision-no-file]")
            .should("be.visible")
            .contains("Not Available");
    });

    it("Verify registered patient and download", () => {
        cy.intercept(
            "GET",
            `**/PatientData/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A*OrganDonorRegistrationStatus*`,
            {
                fixture: "PatientData/donorRegistrationRegistered.json",
            }
        );

        cy.intercept(
            "GET",
            "**/patientservice/PatientData/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/file/bctransplantorgandonor_14bac0b6-9e95-4a1b-b6fd-d354edfce4e7?api-version=2.0",
            {
                fixture: "PatientData/donorRegistrationDownload.json",
            }
        );

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/services"
        );

        cy.get("[data-testid=organ-donor-registration-status]")
            .should("be.visible")
            .contains("Registered");

        cy.get(
            "[data-testid=organ-donor-registration-card] [data-testid=organ-donor-registration-status-info-button]"
        )
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
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.verifyDownload("Organ_Donor_Registration.pdf", {
            timeout: 60000,
            interval: 5000,
        });
    });

    it("Verify registered patient and no download", () => {
        cy.intercept(
            "GET",
            `**/PatientData/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A*OrganDonorRegistrationStatus*`,
            {
                fixture:
                    "PatientData/donorRegistrationRegisteredNoDownload.json",
            }
        );

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/services"
        );

        cy.get("[data-testid=organ-donor-registration-status]")
            .should("be.visible")
            .contains("Registered");

        cy.get(
            "[data-testid=organ-donor-registration-decision-registered-no-file]"
        )
            .should("be.visible")
            .contains(
                "Download only available to registrants after November 2022"
            );
    });
});

describe("Services - Organ Donor Registration Card - ODR Dataset Blocked", () => {
    it("Should not show Organ Donor Registration Card", () => {
        setupStandardFixtures({
            userProfileFixture:
                "UserProfileService/userProfileOrganDonorRegistrationDatasetBlocked.json",
        });
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

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/services"
        );

        cy.get("[data-testid=organ-donor-registration-card]").should(
            "not.exist"
        );

        cy.get("[data-testid=errorBanner]")
            .should("be.visible")
            .contains(
                "Organ Donor Registration is not available at this time. Please try again later."
            );
    });
});
