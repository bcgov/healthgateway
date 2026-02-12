const PATH = "/vppLogin";
const ACCESS_MY_HEALTH_URL =
    "https://dev.vpp.patientportal.ca-1.healtheintent.com/";
const HGW_URL = "https://www.healthgateway.gov.bc.ca";

describe("VPP Login View", () => {
    it('Sign in button navigates to "/login" (same origin)', () => {
        cy.visit(PATH);

        cy.get('[data-testid="continue-to-hgw-button"]')
            .should("be.visible")
            .click();
        cy.location("pathname", { timeout: 10000 }).should("eq", "/login");
    });

    it("Health Gateway info link has the correct external URL", () => {
        cy.visit(PATH);

        cy.get('[data-testid="click-hgw-link"]')
            .should("be.visible")
            .should("have.attr", "href", HGW_URL);
    });

    it("VPP login uses the expected Access My Health URL (from config)", () => {
        cy.visit(PATH);

        cy.location("origin").then((origin) => {
            const isLocalhost = origin.includes("localhost");
            const configUrl = isLocalhost
                ? "http://localhost:3025/configuration"
                : "/configuration";

            cy.request(configUrl)
                .its("body")
                .then((body) => {
                    expect(body).to.have.nested.property(
                        "webClient.accessMyHealthUrl",
                        ACCESS_MY_HEALTH_URL
                    );
                });
        });

        cy.get('[data-testid="cancel-button"]').should("be.visible");
    });
});
