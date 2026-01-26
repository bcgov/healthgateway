import { AuthMethod } from "../../../support/constants";

const homePath = "/home";

const encounterModule = "Encounter";
const immunizationModule = "Immunization";
const laboratoryModule = "Laboratory";

const encounterTitle = "Health Visits";
const immunizationTitle = "Immunizations";
const laboratoryTitle = "COVID‑19 Tests";

const addQuickLinkButtonSelector = "[data-testid=add-quick-link-button]";
const addQuickLinkChipSelector =
    "[data-testid=quick-link-modal-text] span.v-chip";
const addQuickLinkCancelButtonSelector =
    "[data-testid=cancel-add-quick-link-btn]";
const addQuickLinkSubmitButtonSelector = "[data-testid=add-quick-link-btn]";
const addQuickLinkModalTextSelector = "[data-testid=quick-link-modal-text]";
const quickLinkCardSelector = "[data-testid=quick-link-card]";
const cardButtonTitleSelector = "[data-testid=card-button-title]";
const quickLinkMenuButtonSelector = "[data-testid=card-menu-button]";
const quickLinkRemoveButtonSelector = "[data-testid=remove-quick-link-button]";

const defaultTimeout = 60000;

function getQuickLinkChip(module) {
    return cy.get(`${addQuickLinkChipSelector}[name=${module}-filter]`);
}

function getQuickLinkCard(title) {
    return cy
        .contains(cardButtonTitleSelector, title)
        .parents(quickLinkCardSelector);
}

function clickRemoveQuickLinkSelector(isPost = false) {
    if (isPost) {
        cy.intercept("POST", "**/UserProfile/*/preference*").as(
            "postUserProfilePreference"
        );
    } else {
        cy.intercept("PUT", "**/UserProfile/*/preference*").as(
            "putUserProfilePreference"
        );
    }

    cy.get(quickLinkRemoveButtonSelector).should("be.visible").click();

    if (isPost) {
        cy.wait("@postUserProfilePreference", { timeout: defaultTimeout });
    } else {
        cy.wait("@putUserProfilePreference", { timeout: defaultTimeout });
    }
}

describe("Quick Links", () => {
    it("Add, Verify Timeline Link and Remove Quick Link", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );

        // Validate home page has displayed before clicking on quick link.
        cy.get("[data-testid=health-records-card]").should("be.visible");

        cy.log("Adding a quick link");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();
        getQuickLinkChip(laboratoryModule)
            .should("exist")
            .click({ force: true });
        cy.get(addQuickLinkSubmitButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.log("Verifying add quick link button is disabled");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("not.be.enabled");

        cy.log("Verifying quick link card is present and links to timeline");
        cy.intercept("GET", `**/Communication/*`).as("getCommunication");
        cy.intercept("GET", "**/Laboratory/Covid19Orders*").as(
            "getCovid19Orders"
        );

        getQuickLinkCard(laboratoryTitle)
            .should("be.visible", "be.enabled")
            .click();

        cy.wait("@getCommunication", { timeout: defaultTimeout });
        cy.wait("@getCovid19Orders", { timeout: defaultTimeout });
        cy.checkTimelineHasLoaded();

        cy.log("Verifying filter is active");
        cy.contains("[data-testid=filter-label]", laboratoryTitle);
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterContainer]").should("be.visible");
        cy.get("[data-testid=Covid19TestResult-filter]")
            .should("exist")
            .should("have.class", "v-chip--selected");
        cy.get("[data-testid=btnFilterCancel]").click();
        cy.get("[data-testid=covid19testresultTitle]").should("be.visible");

        cy.log("Returning to home page");
        cy.get("[data-testid=menu-btn-home-link]").should("be.visible").click();

        cy.log("Removing quick link");
        getQuickLinkCard(laboratoryTitle).within(() => {
            cy.get(quickLinkMenuButtonSelector)
                .should("be.visible", "be.enabled")
                .click();
        });

        clickRemoveQuickLinkSelector();

        cy.log("Verifying quick link card no longer exists");
        cy.contains(cardButtonTitleSelector, laboratoryTitle).should(
            "not.exist"
        );

        cy.log("Verifying add quick link button has been re-enabled");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("be.enabled");
    });

    it("Add, Cancel, Add and Remove Multiple Quick Links", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );

        // Validate home page has displayed before clicking on quick link.
        cy.get("[data-testid=health-records-card]").should("be.visible");

        cy.log("Opening add quick link modal");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible", "be.enabled")
            .click();

        cy.log("Verifying 3 checkboxes exist");
        cy.get(addQuickLinkChipSelector).should("have.length", 3);
        cy.log(
            "Verifying submit button is disabled beccause no quick links have been selected"
        );
        cy.get(addQuickLinkSubmitButtonSelector)
            .should("be.visible")
            .should("not.be.enabled");

        cy.log("Selecting encounters and immunization checkboxes");
        getQuickLinkChip(encounterModule).should("exist").click();
        getQuickLinkChip(immunizationModule).should("exist").click();
        getQuickLinkChip(laboratoryModule)
            .should("exist")
            .should("not.have.class", "v-chip--selected");

        // Cancel
        cy.get(addQuickLinkCancelButtonSelector).click();
        cy.contains(cardButtonTitleSelector, encounterTitle).should(
            "not.exist"
        );
        cy.contains(cardButtonTitleSelector, immunizationTitle).should(
            "not.exist"
        );
        cy.contains(cardButtonTitleSelector, laboratoryTitle).should(
            "not.exist"
        );

        // Open quick link modal again
        cy.log("Opening add quick link modal again to verify cancel");
        cy.get(addQuickLinkButtonSelector).click();
        cy.get(addQuickLinkModalTextSelector).should("exist");

        cy.log(
            "Verifying submit button is disabled beccause no quick links have been selected"
        );
        cy.get(addQuickLinkSubmitButtonSelector)
            .should("be.visible")
            .should("not.be.enabled");

        // Verify quick links are back in modal
        getQuickLinkChip(encounterModule)
            .should("exist")
            .should("not.have.class", "v-chip--selected");
        getQuickLinkChip(immunizationModule)
            .should("exist")
            .should("not.have.class", "v-chip--selected");
        getQuickLinkChip(laboratoryModule)
            .should("exist")
            .should("not.have.class", "v-chip--selected");

        // Select quick links again to add
        cy.log("Select quick links again to add");
        getQuickLinkChip(encounterModule).should("exist").click();
        getQuickLinkChip(immunizationModule).should("exist").click();
        getQuickLinkChip(laboratoryModule)
            .should("exist")
            .should("not.have.class", "v-chip--selected");

        cy.log("Adding quick links");
        cy.get(addQuickLinkSubmitButtonSelector).click();

        cy.log("Verifying quick link cards are present");
        getQuickLinkCard(encounterTitle).should("be.visible", "be.enabled");
        getQuickLinkCard(immunizationTitle).should("be.visible", "be.enabled");
        cy.contains(cardButtonTitleSelector, laboratoryTitle).should(
            "not.exist"
        );

        cy.log("Opening add quick link modal");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible", "be.enabled")
            .click();

        cy.log(
            "Verifying submit button is disabled beccause no quick links have been selected"
        );
        cy.get(addQuickLinkSubmitButtonSelector)
            .should("be.visible")
            .should("not.be.enabled");

        cy.log("Verifying 1 checkbox remains");
        cy.get(addQuickLinkChipSelector).should("have.length", 1);
        getQuickLinkChip(laboratoryModule)
            .should("exist")
            .should("not.have.class", "v-chip--selected");

        cy.log("Cancelling add quick link modal");
        cy.get(addQuickLinkCancelButtonSelector)
            .should("be.visible", "be.enabled")
            .click();

        cy.log("Removing quick links");
        getQuickLinkCard(encounterTitle).within(() => {
            cy.get(quickLinkMenuButtonSelector).click();
        });
        clickRemoveQuickLinkSelector();
        cy.contains(cardButtonTitleSelector, encounterTitle).should(
            "not.exist"
        );
        getQuickLinkCard(immunizationTitle).within(() => {
            cy.get(quickLinkMenuButtonSelector)
                .should("be.visible", "be.enabled")
                .click();
        });
        clickRemoveQuickLinkSelector();
        cy.contains(cardButtonTitleSelector, immunizationTitle).should(
            "not.exist"
        );

        cy.log("Opening add quick link modal");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible", "be.enabled")
            .click();

        cy.log("Verifying 3 checkboxes exist");
        cy.get(addQuickLinkChipSelector).should("have.length", 3);

        cy.log(
            "Verifying submit button is disabled beccause no quick links have been selected"
        );
        cy.get(addQuickLinkSubmitButtonSelector)
            .should("be.visible")
            .should("not.be.enabled");
    });
});

// Organ Donor Quick Link tests
const organDonorQuickLinkCardSelector =
    "[data-testid=organ-donor-registration-card]";
const organDonorAddQuickLinkChipSelector =
    "[data-testid=organ-donor-registration-filter]";

describe("Organ Donor Quick Link", () => {
    it("Remove and Add Organ Donor Card Quick Link", () => {
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
            homePath
        );

        cy.log("Verifying organ donor quick link card is present");
        cy.get(organDonorQuickLinkCardSelector).should(
            "be.visible",
            "be.enabled"
        );

        cy.log("Removing organ donor quick link");
        cy.get(organDonorQuickLinkCardSelector).within(() => {
            cy.get(quickLinkMenuButtonSelector)
                .should("be.visible", "be.enabled")
                .click();
        });
        clickRemoveQuickLinkSelector(true);

        cy.log("Verifying organ donor quick link no longer exists");
        cy.get(organDonorQuickLinkCardSelector).should("not.exist");

        cy.log("Adding organ donor quick link");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible", "be.enabled")
            .click();
        cy.get(organDonorAddQuickLinkChipSelector).should("exist").click();
        cy.get(addQuickLinkSubmitButtonSelector)
            .should("be.visible", "be.enabled")
            .click();

        cy.log("Verifying add quick link button is disabled");
        cy.get(addQuickLinkButtonSelector).should(
            "be.visible",
            "not.be.enabled"
        );

        cy.log("Verifying organ donor quick link card is present");
        cy.get(organDonorQuickLinkCardSelector).should(
            "be.visible",
            "be.enabled"
        );
    });
});

describe("Disabling organ donor services", () => {
    const testOrganDonorNotPresent = (cy) => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );
        cy.log("Verifying organ donor quicklink not present");
        cy.get(organDonorQuickLinkCardSelector).should("not.exist");

        cy.log("Verifying add organ donor chip not present");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible", "be.enabled")
            .click();
        cy.get(organDonorAddQuickLinkChipSelector).should("not.exist");
    };

    it("Should not show quick link and settings when organ donor service is disabled", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
            ],
            services: {
                services: [
                    {
                        name: "organDonorRegistration",
                        enabled: true,
                    },
                ],
            },
        });
        testOrganDonorNotPresent(cy);
    });

    it("Should not show quick link and settings when organ donor service is disabled", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
            ],
            services: {
                enabled: true,
            },
        });
        testOrganDonorNotPresent(cy);
    });
});

// Health Connect Registry Quick Link tests
const healthConnectQuickLinkCardSelector =
    "[data-testid=health-connect-registry-card]";
const healthConnectAddQuickLinkChipSelector =
    "[data-testid=health-connect-registry-filter]";
const serviceName = "healthConnectRegistry";

describe("Health Connect Registry Card", () => {
    it("Remove and add health connect registry card quick link", () => {
        cy.configureSettings({
            services: {
                enabled: true,
                services: [
                    {
                        name: serviceName,
                        enabled: true,
                    },
                ],
            },
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );

        cy.log("Verifying health connect quick link card is present");
        cy.get(healthConnectQuickLinkCardSelector).should(
            "be.visible",
            "be.enabled"
        );

        cy.log("Removing health connect quick link");
        cy.get(healthConnectQuickLinkCardSelector).within(() => {
            cy.get(quickLinkMenuButtonSelector)
                .should("be.visible", "be.enabled")
                .click();
        });
        clickRemoveQuickLinkSelector(true);

        cy.log("Verifying health connect quick link no longer exists");
        cy.get(healthConnectQuickLinkCardSelector).should("not.exist");

        cy.log("Adding health connect quick link");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible", "be.enabled")
            .click();
        cy.get(healthConnectAddQuickLinkChipSelector).should("exist").click();
        cy.get(addQuickLinkSubmitButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.log("Verifying add quick link button is disabled");
        cy.get(addQuickLinkButtonSelector).should(
            "be.visible",
            "not.be.enabled"
        );

        cy.log("Verifying health connect quick link card is present");
        cy.get(healthConnectQuickLinkCardSelector).should(
            "be.visible",
            "be.enabled"
        );
    });
});

describe("Disabling health connect services", () => {
    const testQuickLinkNotPresent = (cy) => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );
        cy.log("Verifying health connect quicklink not present");
        cy.get(healthConnectQuickLinkCardSelector).should("not.exist");

        cy.log("Verifying add health connect chip not present");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible", "be.enabled")
            .click();
        cy.get(healthConnectAddQuickLinkChipSelector).should("not.exist");
    };

    it("Should not show quick link and settings when health connect service is disabled", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
            ],
            services: {
                services: [
                    {
                        name: serviceName,
                        enabled: true,
                    },
                ],
            },
        });
        testQuickLinkNotPresent(cy);
    });

    it("Should not show quick link and settings when health connect service is disabled", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
            ],
            services: {
                enabled: true,
            },
        });
        testQuickLinkNotPresent(cy);
    });
});
