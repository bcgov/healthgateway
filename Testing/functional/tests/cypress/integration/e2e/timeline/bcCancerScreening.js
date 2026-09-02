import { AuthMethod } from "../../../support/constants";
import {
    validateAttachmentDownload,
    validateFileDownload,
} from "../../../support/functions/timeline";

describe("BC Cancer download integration", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [{ name: "bcCancerScreening", enabled: true }],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/timeline"
        );
        cy.checkTimelineHasLoaded();
    });

    it("Downloads a BC Cancer letter and its attachment", () => {
        // Program-specific rendering is covered with UI fixtures. This keeps
        // one real Patient Data and download integration path.
        cy.get("[data-testid=timelineCard]")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                cy.get("[data-testid=bccancerscreeningTitle]").click({
                    force: true,
                });
                validateFileDownload(
                    "[data-testid=bc-cancer-screening-download-button]",
                    false
                );
                validateAttachmentDownload();
            });
    });
});
