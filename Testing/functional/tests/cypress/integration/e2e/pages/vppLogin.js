const PATH = "/vppLogin";
const ACCESS_MY_HEALTH_URL =
    "https://dev.vpp.patientportal.ca-1.healtheintent.com/";

describe("VPP Login View", () => {
    it('Sign in button navigates to "/login" (same origin)', () => {
        cy.visit(PATH);

        cy.get('[data-testid="sign-in-button"]').should("be.visible").click();
        cy.location("pathname", { timeout: 10000 }).should("eq", "/login");
    });

    it("VPP login uses the expected Access My Health URL (from config)", () => {
        cy.visit(PATH);

        cy.request("/configuration")
            .its("body")
            .then((body) => {
                expect(body).to.have.nested.property(
                    "webClient.accessMyHealthUrl",
                    ACCESS_MY_HEALTH_URL
                );
            });

        cy.get('[data-testid="do-not-sign-in-button"]').should("be.visible");
    });
});
