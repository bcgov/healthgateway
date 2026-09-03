import { AuthMethod } from "../../../support/constants";
import {
    getCardSelector,
    getTabButtonSelector,
} from "../../../support/functions/dependent";
import { confirmAndVerifyReportDownload } from "../../../support/functions/report";
import { prepareImmunizationWait } from "../../../support/functions/timeline";

// Jennifer T is seeded with dependent Immunization report data.
const dependentHdid = "162346565465464564565463257";

// This is the representative dependent report download. Dataset-specific
// presentation belongs in fixture-backed UI report specs.
describe("Dependent report download integration", () => {
    it("Downloads an Immunization report", () => {
        cy.setupDownloads();
        cy.configureSettings({
            dependents: {
                enabled: true,
                timelineEnabled: true,
            },
            datasets: [{ name: "immunization", enabled: true }],
        });

        const waitForImmunizations = prepareImmunizationWait(dependentHdid);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );

        cy.get(getCardSelector(dependentHdid)).within(() => {
            cy.get(getTabButtonSelector(dependentHdid, "report")).click();
            cy.get("[data-testid=report-tab]").within(() => {
                cy.vSelect("[data-testid=report-type]", "Immunizations");
                waitForImmunizations();
                cy.get("[data-testid=export-record-btn]").click();
            });
        });

        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();
        confirmAndVerifyReportDownload();
    });
});
