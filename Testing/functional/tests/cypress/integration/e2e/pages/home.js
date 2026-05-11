const { AuthMethod } = require("../../../support/constants");
const homeUrl = "/home";
const timelineUrl = "/timeline";
const downloadUrl = "/reports";
const profileUrl = "/profile";
const defaultTimeout = 60000;

describe("Home Page", () => {
    beforeEach(() => {
        cy.configureSettings({
            homepage: {
                showImmunizationRecordLink: true,
            },
            datasets: [
                {
                    name: "specialAuthorityRequest",
                    enabled: true,
                },
                {
                    name: "medication",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
                {
                    name: "healthVisit",
                    enabled: true,
                },
                {
                    name: "note",
                    enabled: true,
                },
            ],
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );
    });

    it("Home - Health Records Card link to Health Records", () => {
        cy.contains("[data-testid=card-button-title]", "Health Records")
            .parents("[data-testid=health-records-card]")
            .should("be.visible", "be.enabled")
            .click();

        cy.url().should("include", timelineUrl);
        // When a result is returned, the content placeholder will not be displayed.
        cy.get("[data-testid=content-placeholders]").should("be.visible");
        cy.get("[data-testid=loading-toast]").should("exist");
        cy.get("[data-testid=content-placeholders]").should("not.exist");
        cy.get("[data-testid=loading-toast]").should("exist");
        cy.get("[data-testid=timeline-record-count]").should("be.visible");
    });

    it("Home - Immunization Card link to Download Immunization", () => {
        cy.intercept("GET", "**/Immunization*").as("getImmunizations");
        cy.contains("[data-testid=card-button-title]", "Immunization Record")
            .parents("[data-testid=immunization-record-card-button]")
            .should("be.visible", "be.enabled")
            .click();

        cy.url().should("include", downloadUrl);
        cy.wait("@getImmunizations", { timeout: defaultTimeout });
        cy.get("[data-testid=immunization-history-report-table]").should(
            "be.visible"
        );
        cy.get("[data-testid=immunizationDateItem]", { timeout: 60000 })
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);
    });

    it("Home - Medication Card link to Health Records", () => {
        cy.contains("[data-testid=card-button-title]", "Medications")
            .parents("[data-testid=quick-link-card]")
            .should("be.visible", "be.enabled")
            .click();

        cy.url().should("include", timelineUrl);
        // Medication takes the longest to return results. The content placeholder will dispaly until Medication finishes.
        cy.get("[data-testid=content-placeholders]").should("be.visible");
        cy.get("[data-testid=loading-toast]").should("exist");
        cy.get("[data-testid=content-placeholders]").should("not.exist");
        cy.get("[data-testid=loading-toast]").should("not.exist");
        cy.get("[data-testid=timeline-record-count]").should("be.visible");
    });

    it("Home - Side Menu Timeline link to Health Records", () => {
        cy.get("[data-testid=menu-btn-health-records-link]")
            .should("be.visible", "be.enabled")
            .click();

        cy.url().should("include", timelineUrl);
        // When a result is returned, the content placeholder will not be displayed.
        cy.get("[data-testid=content-placeholders]").should("be.visible");
        cy.get("[data-testid=loading-toast]").should("exist");
        cy.get("[data-testid=content-placeholders]").should("not.exist");
        cy.get("[data-testid=loading-toast]").should("exist");
        cy.get("[data-testid=timeline-record-count]").should("be.visible");
    });
});

describe("Home page - notification settings alert", () => {
    it("SMS removed alert should display if SMS is removed", () => {
        cy.configureSettings({});

        cy.login(
            Cypress.env("keycloak.hthgtwy06.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.log("The alert should be visible when first logging in.");
        cy.get("[data-testid=incomplete-profile-banner]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=sms-removed-message]").should(
                    "be.visible"
                );

                cy.get("[data-testid=unverified-email-sms-message]").should(
                    "not.exist"
                );
            });

        cy.log("The link in the alert should go to the profile page.");
        cy.get("[data-testid=profile-preferences-link]")
            .should("be.visible")
            .click();
        cy.location("pathname").should("eq", profileUrl);

        cy.log("When returning to the home page, the alert should be gone.");
        cy.get("[data-testid=menu-btn-home-link]").should("be.visible").click();
        cy.location("pathname").should("eq", homeUrl);
        cy.contains("#subject", "Home").should("exist");
        cy.get("[data-testid=sms-removed-message]").should("not.exist");
    });

    it("SMS removed alert should not display on subsequent visits", () => {
        cy.configureSettings({});

        cy.login(
            Cypress.env("keycloak.hthgtwy06.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.contains("#subject", "Home").should("exist");
        cy.get("[data-testid=sms-removed-message]").should("not.exist");
    });
});

describe("Home page - Recommendations", () => {
    beforeEach(() => {
        cy.configureSettings({
            homepage: {
                showRecommendationsLink: true,
            },
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });

        cy.login(
            Cypress.env("keycloak.hthgtwy20.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );
    });

    it("Recommendation Quick link should be visible by default and configurable", () => {
        cy.get("[data-testid=recommendations-card-button]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=card-menu-button]")
                    .should("be.visible")
                    .click();
                cy.document()
                    .find("[data-testid=remove-quick-link-button")
                    .should("be.visible")
                    .click();
            });
        cy.get("[data-testid=recommendations-card-button]").should("not.exist");
        cy.get("[data-testid=add-quick-link-button]").click();
        cy.get("[data-testid=recommendations-dialog-filter]")
            .should("be.visible")
            .should("not.have.class", "v-chip--selected")
            .click();
        cy.get("[data-testid=add-quick-link-btn]").click();

        cy.get("[data-testid=recommendations-card-button]").should(
            "be.visible"
        );
    });

    it("Link should open recommendations dialog", () => {
        cy.get("[data-testid=recommendations-card-button]").click();
        cy.get("[data-testid=recommendations-dialog]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=recommendation-history-report-table]");
                cy.get("tbody tr").should("have.length.least", 1);
                cy.get(
                    "[data-testid=close-recommendations-dialog-button]"
                ).click();
            });
        cy.get("[data-testid=recommendations-dialog]").should("not.exist");
    });
});

describe("MOBILE - Home page - Recommendations", () => {
    beforeEach(() => {
        cy.configureSettings({
            homepage: {
                showRecommendationsLink: true,
            },
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.viewport("iphone-6");

        cy.login(
            Cypress.env("keycloak.hthgtwy20.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );
    });

    it("Mobile - Link should open recommendations dialog with content", () => {
        cy.get("[data-testid=recommendations-card-button]").click();
        cy.get("[data-testid=recommendations-dialog]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=recommendation-history-report-table]");
                cy.get("tbody tr").should("have.length.least", 1);
                cy.get(
                    "[data-testid=close-recommendations-dialog-button]"
                ).click();
            });
        cy.get("[data-testid=recommendations-dialog]").should("not.exist");
    });
});
