const { AuthMethod } = require("../../../support/constants");

describe("Laboratory", () => {
    before(() => {
        cy.enableModules(["CovidLabResults", "Laboratory", "Dependent"]);
        cy.intercept("GET", "**/v1/api/Laboratory*", {
            fixture: "LaboratoryService/laboratory.json",
        });
        cy.intercept("GET", "**/v1/api/UserProfile/*/Dependent", {
            fixture: "UserProfileService/dependent.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
    });

    it("Validate Covid Tab with Results", () => {
        let sensitiveDocMessage =
            " The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off. ";
        // Validate the tab and elements are present
        cy.get("[data-testid=covid19TabTitle]").last().parent().click();
        cy.get("[data-testid=dependentCovidTestDate]")
            .first()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);
        cy.get("[data-testid=dependentCovidReportDownloadBtn]").first().click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageText]").should(
            "have.text",
            sensitiveDocMessage
        );
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=genericMessageModal]").should("not.exist");

        cy.get("[data-testid=covid19TabTitle]").last().parent().click();
        cy.get("[data-testid=dependentCovidTestDate]")
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);
        expect(cy.get("[data-testid=dependentCovidTestStatus]").last()).not.to
            .be.empty;
        expect(cy.get("[data-testid=dependentCovidTestLabResult]").last()).not
            .to.be.empty;
        cy.get("[data-testid=dependentCovidReportDownloadBtn]").last().click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageText]").should(
            "have.text",
            sensitiveDocMessage
        );
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });

    it("Validate Covid with multiple results", () => {
        cy.get("[data-testid=dependentCovidTestDate]")
            .contains("td", "2020-Oct-06")
            .siblings("[data-testid=dependentCovidTestLabResult]")
            .contains("Negative")
            .next()
            .find("[data-testid=dependent-covid-test-info-button]")
            .click();
        cy.get("[data-testid=dependent-covid-test-info-popover]").should(
            "be.visible"
        );
        cy.get("[data-testid=dependentCovidTestDate]")
            .contains("td", "2020-Oct-04")
            .siblings("[data-testid=dependentCovidTestLabResult]")
            .contains("Negative");
        cy.get("[data-testid=dependentCovidTestDate]")
            .contains("td", "2020-Oct-03")
            .siblings("[data-testid=dependentCovidTestStatus]")
            .contains("SomeOtherState");
        cy.get("[data-testid=dependentCovidTestDate]")
            .contains("td", "2020-Oct-02")
            .siblings("[data-testid=dependentCovidTestStatus]")
            .contains("SomeOtherState");
    });
});
