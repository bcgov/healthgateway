const { AuthMethod } = require("../../../../support/constants");

describe("COVID-19 Orders", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
            fixture: "LaboratoryService/covid19Orders.json",
        });
        cy.enableModules("Laboratory");
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card", () => {
        cy.log("Verifying card data");
        cy.get("[data-testid=timelineCard]").first().click();

        cy.get("[data-testid=backBtn]").should("be.visible");
        cy.get("[data-testid=entryCardDetailsTitle]").should("be.visible");
        cy.get("[data-testid=laboratoryHeaderDescription]").should(
            "be.visible"
        );
        cy.get("[data-testid=laboratoryReport]").should("be.visible");
        cy.get("[data-testid=laboratoryReportingLab]").should("be.visible");
        cy.get("[data-testid=laboratoryTestType]").should("be.visible");
        cy.get("[data-testid=laboratoryTestStatus-0]").should("be.visible");

        cy.get("[data-testid=backBtn]").click({ force: true });
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
                cy.get("[data-testid=laboratoryTestStatus-0]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratoryTestStatus-0]", {
                    timeout: 1000,
                }).should(($div) => {
                    expect($div.text().trim()).equal(finalStatus);
                });

                // Validate Report Attachment Icon
                cy.log("Card with reports should have an attachment icon.");
                cy.get("[data-testid=attachmentIcon]").should("exist");

                cy.log("Card with attachment icon should have a report.");
                cy.get("[data-testid=laboratoryReportAvailable]").should(
                    "exist"
                );

                cy.get("[data-testid=backBtn]").click({ force: true });
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
                cy.get(
                    "[data-testid=laboratoryResultDescription-0]"
                ).scrollIntoView();
                cy.get("[data-testid=result-link]").should("be.visible");

                cy.get("[data-testid=laboratoryTestStatus-2]")
                    .scrollIntoView()
                    .should(($div) => {
                        expect($div.text().trim()).equal(otherStatus);
                    });

                cy.get("[data-testid=backBtn]").click({ force: true });
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
                cy.get("[data-testid=laboratoryTestStatus-0]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratoryTestStatus-0]").should(
                    ($div) => {
                        expect($div.text().trim()).equal(correctedStatus);
                    }
                );

                // Validate Report Attachment Icon
                cy.log("Card with reports should have an attachment icon.");
                cy.get("[data-testid=attachmentIcon]").should("exist");

                cy.log("Card with attachment icon should have a report.");
                cy.get("[data-testid=laboratoryReportAvailable]").should(
                    "exist"
                );

                cy.get("[data-testid=backBtn]").click({ force: true });
            });

        cy.get("[data-testid=filterTextInput]").should("be.visible");

        cy.log("Verifying amended state");
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
                cy.get("[data-testid=laboratoryTestStatus-0]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratoryTestStatus-0]").should(
                    ($div) => {
                        expect($div.text().trim()).equal(amendedStatus);
                    }
                );

                // Validate Report Attachment Icon
                cy.log("Card with reports should have an attachment icon.");
                cy.get("[data-testid=attachmentIcon]").should("exist");

                cy.log("Card with attachment icon should have a report.");
                cy.get("[data-testid=laboratoryReportAvailable]").should(
                    "exist"
                );

                cy.get("[data-testid=backBtn]").click({ force: true });
            });
        cy.get("[data-testid=filterTextInput]").should("be.visible");
    });

    it("Validate Download", () => {
        cy.intercept(
            "GET",
            "**/Laboratory/*/Report?hdid=P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A&isCovid19=true",
            {
                fixture: "LaboratoryService/covid19ReportPdf.json",
            }
        );

        cy.contains("[data-testid=entryCardDate]", "2020-Dec-03")
            .first()
            .scrollIntoView()
            .should("be.visible")
            .parents("[data-testid=timelineCard]")
            .click();

        cy.get("[id=entry-details-modal]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=covid-result-download-btn]")
                    .should("be.visible")
                    .click({ force: true });
            });

        cy.get("[data-testid=genericMessageSubmitBtn]")
            .should("be.visible")
            .click({ force: true });

        cy.verifyDownload("COVID_Result_2020_12_03-02_00.pdf");

        cy.get("[data-testid=backBtn]").click({ force: true });
    });
});
