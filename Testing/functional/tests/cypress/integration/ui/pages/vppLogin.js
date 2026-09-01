const path = "/vppLogin";
const accessMyHealthUrl =
    "https://dev.vpp.patientportal.ca-1.healtheintent.com/";
const healthGatewayUrl = "https://www.healthgateway.gov.bc.ca";

describe("VPP Login View", () => {
    beforeEach(() => {
        cy.configureSettings({});
        cy.visit(path);
    });

    it("Navigates to login", () => {
        cy.get("[data-testid=continue-to-hgw-button]")
            .should("be.visible")
            .click();
        cy.location("pathname").should("eq", "/login");
    });

    it("Displays the configured external links", () => {
        cy.get("[data-testid=click-hgw-link]")
            .should("be.visible")
            .should("have.attr", "href", healthGatewayUrl);

        cy.get("@config").should(
            "have.nested.property",
            "webClient.accessMyHealthUrl",
            accessMyHealthUrl
        );
        cy.get("[data-testid=cancel-button]").should("be.visible");
    });
});
