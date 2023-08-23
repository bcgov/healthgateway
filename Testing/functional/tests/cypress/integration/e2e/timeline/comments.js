const { AuthMethod } = require("../../../support/constants");

describe("Comments Disable", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
    });

    it("Comments Disable", () => {
        cy.get("[data-testid=add-comment-text-area]").should("not.exist");
        cy.get("[data-testid=post-comment-btn]").should("not.exist");
    });
});

describe("Comments Enable", () => {
    beforeEach(() => {
        cy.configureSettings({
            timeline: {
                comment: true,
            },
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Add", () => {
        var testComment = "Test Add Comment";
        cy.get("[data-testid=commentIcon]").should("not.exist");
        cy.get("[data-testid=commentCount]").should("not.exist");

        cy.get('[data-testid="timelineCard"]')
            .first()
            .within(() => {
                cy.get("[data-testid=entryCardDetailsTitle]").click({
                    force: true,
                });

                // Add comment
                cy.get("[data-testid=add-comment-text-area]").type(testComment);
                cy.get("[data-testid=post-comment-btn]").click({ force: true });

                // Verify
                cy.get("[data-testid=commentText]").contains(testComment);
                cy.get("[data-testid=commentIcon]").should("exist");
                cy.get("[data-testid=commentCount]").should("not.exist");
            });
    });

    it("Validate Filter", () => {
        var testComment = "Test Add Comment";

        // Filter by text
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterTextInput]").type(testComment);
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");

        cy.get('[data-testid="timelineCard"]')
            .first()
            .within(() => {
                cy.get("[data-testid=entryCardDetailsTitle]").click({
                    force: true,
                });
                cy.get("[data-testid=showCommentsBtn]").click();

                // Verify
                cy.get("[data-testid=commentText]")
                    .first()
                    .contains(testComment);
                cy.get("[data-testid=commentIcon]").should("exist");
                cy.get("[data-testid=commentCount]").should("not.exist");
            });
    });

    it("Validate Edit", () => {
        var testEditComment = "Test Edit Comment";

        cy.get("[data-testid=timelineCard]")
            .first()
            .within(() => {
                cy.get("[data-testid=entryCardDetailsTitle]").click({
                    force: true,
                });
                cy.get("[data-testid=showCommentsBtn]").click();
                var currentText = cy
                    .get("[data-testid=commentText]")
                    .first()
                    .then((element) => {
                        currentText = element.text();
                        cy.log(currentText);

                        cy.get("[data-testid=commentMenuBtn]")
                            .first()
                            .click({ force: true });
                        cy.document()
                            .find("[data-testid=commentMenuEditBtn]")
                            .click();
                        cy.get("[data-testid=editCommentInput] textarea")
                            .first()
                            .type(testEditComment);
                        cy.get("[data-testid=saveCommentBtn]").click();

                        cy.get("[data-testid=commentText]")
                            .first()
                            .contains(currentText + testEditComment);
                        cy.get("[data-testid=commentIcon]").should("exist");
                        cy.get("[data-testid=commentCount]").should(
                            "not.exist"
                        );
                    });
            });
    });

    it("Validate Delete", () => {
        cy.get("[data-testid=timelineCard]")
            .first()
            .within(() => {
                cy.get("[data-testid=entryCardDetailsTitle]").click({
                    force: true,
                });
                cy.get("[data-testid=showCommentsBtn]").click({ force: true });

                // Delete comment
                cy.get("[data-testid=commentMenuBtn]")
                    .first()
                    .click({ force: true });
                cy.on("window:confirm", (str) => {
                    expect(str).to.eq(
                        "Are you sure you want to delete this comment?"
                    );
                });
                cy.document()
                    .find("[data-testid=commentMenuDeleteBtn]")
                    .first()
                    .click({ force: true });

                // Veriy
                cy.get("[data-testid=commentIcon]").should("not.exist");
                cy.get("[data-testid=commentCount]").should("not.exist");
            });
    });
});
