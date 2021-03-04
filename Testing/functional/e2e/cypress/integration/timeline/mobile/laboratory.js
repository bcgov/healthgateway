const { AuthMethod } = require("../../../support/constants");
describe("Laboratory", () => {
  beforeEach(() => {
    cy.enableModules("Laboratory");
    cy.intercept("GET", "**/v1/api/Laboratory/*", (req) => {
      req.reply((res) => {
        res.send({ fixture: "LaboratoryService/laboratory.json" });
      });
    });
    cy.viewport("iphone-6");
    cy.login(
      Cypress.env("keycloak.username"),
      Cypress.env("keycloak.password"),
      AuthMethod.KeyCloak
    );
    cy.checkTimelineHasLoaded();
  });

  it("Validate Card Details on Mobile", () => {
    cy.get("[data-testid=timelineCard]").first().click();
    cy.get("[data-testid=entryDetailsModal]").then((entryDetailsModal) => {
      entryDetailsModal.get("[data-testid=backBtn]").should("be.visible");
      entryDetailsModal
        .get("[data-testid=entryCardDetailsTitle]")
        .should("be.visible");
      entryDetailsModal
        .get("[data-testid=laboratoryHeaderDescription]")
        .should("be.visible");
      entryDetailsModal
        .get("[data-testid=laboratoryReport]")
        .should("be.visible");
      entryDetailsModal
        .get("[data-testid=laboratoryReportingLab]")
        .should("be.visible");
      entryDetailsModal
        .get("[data-testid=laboratoryTestType]")
        .should("be.visible");
      entryDetailsModal
        .get("[data-testid=laboratoryTestStatus]")
        .should("be.visible");
      entryDetailsModal.get("[data-testid=cardBtn]").should("be.visible");
    });
  });

  it("Validate Card Details on Mobile", () => {
    cy.get("[data-testid=immunizationBtnReady]").first().click();
    cy.get("[data-testid=timelineCard]").first().click();
    cy.get("[data-testid=entryDetailsModal]").then((entryDetailsModal) => {
      entryDetailsModal.get("[data-testid=backBtn]").should("be.visible");
      entryDetailsModal
        .get("[data-testid=entryCardDetailsTitle]")
        .should("be.visible");
      entryDetailsModal.get("[data-testid=entryCardDate]").should("be.visible");
      entryDetailsModal
        .get("[data-testid=immunizationProductTitle]")
        .should("be.visible");
      entryDetailsModal
        .get("[data-testid=immunizationProviderTitle]")
        .should("be.visible");
      entryDetailsModal
        .get("[data-testid=immunizationLotTitle]")
        .should("be.visible");
      entryDetailsModal.get("[data-testid=cardBtn]").should("be.visible");
    });
  });
});
