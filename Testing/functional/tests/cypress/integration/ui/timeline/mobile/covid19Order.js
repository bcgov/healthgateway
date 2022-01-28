const { AuthMethod } = require("../../../../support/constants");

describe("COVID-19 Orders", () => {
    before(() => {
        cy.viewport("iphone-6");
        cy.restoreAuthCookies();
        cy.enableModules("Laboratory");
        cy.intercept("GET", "**/v1/api/Laboratory/Covid19Orders*", {
            fixture: "LaboratoryService/covid19Orders.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
    });

    it("Validate Card", () => {
        cy.log("Verifying card data");
        cy.get("[data-testid=timelineCard]")
            .first()
            .click()
            .then(() => {
                cy.get("[data-testid=backBtn]").should("be.visible");
                cy.get("[data-testid=entryCardDetailsTitle]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratoryHeaderDescription]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratoryReport]").should("be.visible");
                cy.get("[data-testid=laboratoryReportingLab]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratoryTestType]").should("be.visible");
                cy.get("[data-testid=laboratoryTestStatus]").should(
                    "be.visible"
                );
            });

        cy.get("[data-testid=backBtn]").click();
        cy.get("[data-testid=filterTextInput]").should("be.visible");

        cy.log("Verifying final status");
        const negativeSummary = "Result: Negative";
        const finalStatus = "Test Status: Final";
        cy.get("[data-testid=timelineCard]")
            .first()
            .within(() => {
                cy.get("[data-testid=laboratoryHeaderDescription]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratoryHeaderDescription]").should(
                    ($div) => {
                        expect($div.text().trim()).equal(negativeSummary);
                    }
                );
                cy.get("[data-testid=entryCardDetailsTitle]").click();
            });
        cy.get("[id=entry-details-modal]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=laboratoryTestStatus]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratoryTestStatus]", {
                    timeout: 1000,
                }).should(($div) => {
                    expect($div.text().trim()).equal(finalStatus);
                });
                cy.get("[data-testid=backBtn]").click();
            });
        cy.get("[data-testid=filterTextInput]").should("be.visible");

        // Test multiple records with resultLink
        const otherStatus = "Test Status: SomeOtherState";
        cy.log("Verifying not ready state");
        cy.get("[data-testid=timelineCard]")
            .eq(1) // Card Index 1
            .within(() => {
                cy.get("[data-testid=laboratoryHeaderDescription]").should(
                    "not.exist"
                );
                cy.get("[data-testid=entryCardDetailsTitle]").click();
            });
        cy.get("[id=entry-details-modal]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=laboratoryTestStatus]")
                    .first()
                    .should("be.visible");
                cy.get("[data-testid=laboratoryTestStatus]")
                    .first()
                    .should(($div) => {
                        expect($div.text().trim()).equal(otherStatus);
                    });
                cy.get("[data-testid=laboratoryResultDescription]")
                    .eq(2)
                    .find("p")
                    .last()
                    .find("a")
                    .should("have.attr", "href");

                cy.get("[data-testid=backBtn]").click();
            });
        cy.get("[data-testid=filterTextInput]").should("be.visible");

        const positiveSummary = "Result: Positive";
        cy.log("Verifying Corrected state");
        cy.get("[data-testid=timelineCard]")
            .eq(2) // Card Index 2
            .within(() => {
                cy.get("[data-testid=laboratoryHeaderDescription]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratoryHeaderDescription]", {
                    timeout: 1000,
                }).should(($div) => {
                    expect($div.text().trim()).equal(positiveSummary);
                });
                cy.get("[data-testid=entryCardDetailsTitle]").click();
            });

        const correctedStatus = "Test Status: Corrected";
        cy.get("[id=entry-details-modal]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=laboratoryTestStatus]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratoryTestStatus]").should(($div) => {
                    expect($div.text().trim()).equal(correctedStatus);
                });
                cy.get("[data-testid=backBtn]").click();
            });
        cy.get("[data-testid=filterTextInput]").should("be.visible");

        cy.log("Verifying ammended state");
        cy.get("[data-testid=timelineCard]")
            .eq(3)
            .within(() => {
                cy.get("[data-testid=laboratoryHeaderDescription]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratoryHeaderDescription]").should(
                    ($div) => {
                        expect($div.text().trim()).equal(positiveSummary);
                    }
                );
                cy.get("[data-testid=entryCardDetailsTitle]").click();
            });

        const amendedStatus = "Test Status: Amended";
        cy.get("[id=entry-details-modal]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=laboratoryTestStatus]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratoryTestStatus]").should(($div) => {
                    expect($div.text().trim()).equal(amendedStatus);
                });
            });
    });

    it("Validate Report Attachment Icons", () => {
        cy.log("All cards with reports should have attachment icons.");
        cy.get("[data-testid=laboratoryReportAvailable]")
            .closest("[data-testid=timelineCard]")
            .each((card) => {
                cy.wrap(card)
                    .find("[data-testid=attachmentIcon]")
                    .should("exist");
            });

        cy.log("All cards with attachment icons should have reports.");
        cy.get("[data-testid=attachmentIcon]")
            .closest("[data-testid=timelineCard]")
            .each((card) => {
                cy.wrap(card)
                    .find("[data-testid=laboratoryReportAvailable]")
                    .should("exist");
            });
    });
});
