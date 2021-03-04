const { AuthMethod } = require("../../../support/constants");
before(() => {
  cy.restoreAuthCookies();
  cy.enableModules("Laboratory");
  cy.intercept("GET", "**/v1/api/Laboratory*", {
    fixture: "LaboratoryService/laboratory.json",
  });
  cy.login(
    Cypress.env("keycloak.username"),
    Cypress.env("keycloak.password"),
    AuthMethod.KeyCloak
  );
});
describe("Laboratory", () => {
  it("Validate Card Details on Mobile", () => {
    cy.viewport("iphone-6");
    cy.get("[data-testid=timelineCard]")
      .first()
      .click()
      .then(() => {
        cy.get("[data-testid=backBtn]").should("be.visible");
        cy.get("[data-testid=entryCardDetailsTitle]").should("be.visible");
        cy.get("[data-testid=laboratoryHeaderDescription]").should(
          "be.visible"
        );
        cy.get("[data-testid=laboratoryReport]").should("be.visible");
        cy.get("[data-testid=laboratoryReportingLab]").should("be.visible");
        cy.get("[data-testid=laboratoryTestType]").should("be.visible");
        cy.get("[data-testid=laboratoryTestStatus]").should("be.visible");
      });
  });

  it("Validate Final status", () => {
    cy.viewport("iphone-6");
    const summaryText = "Result: Negative";
    cy.get("[data-testid=timelineCard]")
      .first()
      .within(() => {
        cy.get("[data-testid=laboratoryHeaderDescription]").should(
          "be.visible"
        );
        cy.get("[data-testid=laboratoryHeaderDescription]", {
          timeout: 1000,
        }).should(($div) => {
          expect($div.text()).equal(summaryText);
        });
        cy.get("[data-testid=entryCardDetailsTitle]")
          .click()
          .then(() => {
            const testStatus = "Test Status: Final";
            cy.get("[data-testid=laboratoryTestStatus]", {
              timeout: 1000,
            }).should(($div) => {
              expect($div.text()).equal(testStatus);
            });
          });
      });
  });

  it("Validate Amended status", () => {
    cy.viewport("iphone-6");
    const summaryText = "Result: Positive";
    cy.get("[data-testid=timelineCard]")
      .eq(1)
      .within(() => {
        cy.get("[data-testid=laboratoryHeaderDescription]").should(
          "be.visible"
        );
        cy.get("[data-testid=laboratoryHeaderDescription]", {
          timeout: 1000,
        }).should(($div) => {
          expect($div.text()).equal(summaryText);
        });
        cy.get("[data-testid=entryCardDetailsTitle]")
          .click()
          .then(() => {
            const testStatus = "Test Status: Amended";
            cy.get("[data-testid=laboratoryTestStatus]", {
              timeout: 1000,
            }).should(($div) => {
              expect($div.text()).equal(testStatus);
            });
          });
      });
  });

  it("Validate Corrected status", () => {
    cy.viewport("iphone-6");
    const summaryText = "Result: Positive";
    cy.get("[data-testid=timelineCard]")
      .eq(2)
      .within(() => {
        cy.get("[data-testid=laboratoryHeaderDescription]").should(
          "be.visible"
        );
        cy.get("[data-testid=laboratoryHeaderDescription]").should(($div) => {
          expect($div.text()).equal(summaryText);
        });
        cy.get("[data-testid=entryCardDetailsTitle]")
          .click()
          .then(() => {
            const testStatus = "Test Status: Corrected";
            cy.get("[data-testid=laboratoryTestStatus]").should(($div) => {
              expect($div.text()).equal(testStatus);
            });
          });
      });
  });

  it("Validate Other statuses", () => {
    cy.viewport("iphone-6");
    cy.get("[data-testid=timelineCard]")
      .eq(3)
      .within(() => {
        cy.get("[data-testid=laboratoryHeaderDescription]").should(
          "note.be.visible"
        );
        cy.get("[data-testid=entryCardDetailsTitle]")
          .click()
          .then(() => {
            const testStatus = "Test Status: SomeOtherState";
            cy.get("[data-testid=laboratoryTestStatus]").should(($div) => {
              expect($div.text()).equal(testStatus);
            });
          });
      });
  });
});
