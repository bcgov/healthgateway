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
        cy.configureSettings({
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.get("[data-testid=addCommentTextArea]").should("not.exist");
        cy.get("[data-testid=postCommentBtn]").should("not.exist");
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
                    name: "clinicalDocument",
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

        cy.get("[data-testid=entryCardDetailsTitle]").first().click();

        // Add comment
        cy.get("[data-testid=addCommentTextArea]").first().type(testComment);
        cy.get("[data-testid=postCommentBtn]").first().click();

        // Verify
        cy.get("[data-testid=commentText]").first().contains(testComment);
        cy.get("[data-testid=commentIcon]").should("exist");
        cy.get("[data-testid=commentCount]").should("not.exist");
    });

    it("Validate Filter", () => {
        var testComment = "Test Add Comment";

        // Filter by text
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterTextInput]").type(testComment);
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");

        cy.get("[data-testid=entryCardDetailsTitle]").first().click();
        cy.get("[data-testid=showCommentsBtn]").first().click();

        // Verify
        cy.get("[data-testid=commentText]").first().contains(testComment);
        cy.get("[data-testid=commentIcon]").should("exist");
        cy.get("[data-testid=commentCount]").should("not.exist");
    });

    it("Validate Edit", () => {
        var testEditComment = "Test Edit Comment";

        cy.get("[data-testid=entryCardDetailsTitle]").first().click();
        cy.get("[data-testid=showCommentsBtn]").first().click();
        var currentText = cy
            .get("[data-testid=commentText]")
            .first()
            .then((element) => {
                currentText = element.text();
                cy.log(currentText);

                // Edit comment
                cy.get("[data-testid=commentMenuBtn]").first().click();
                cy.get("[data-testid=commentMenuEditBtn]").first().click();
                cy.get("[data-testid=editCommentInput]").type(testEditComment);
                cy.get("[data-testid=saveCommentBtn]").click();

                // Veriy
                cy.get("[data-testid=commentText]").contains(
                    currentText + testEditComment
                );
                cy.get("[data-testid=commentIcon]").should("exist");
                cy.get("[data-testid=commentCount]").should("not.exist");
            });
    });

    it("Validate Delete", () => {
        cy.get("[data-testid=entryCardDetailsTitle]").first().click();
        cy.get("[data-testid=showCommentsBtn]").first().click();

        // Delete comment
        cy.get("[data-testid=commentMenuBtn]").first().click();
        cy.on("window:confirm", (str) => {
            expect(str).to.eq("Are you sure you want to delete this comment?");
        });
        cy.get("[data-testid=commentMenuDeleteBtn]").first().click();

        // Veriy
        cy.get("[data-testid=commentIcon]").should("not.exist");
        cy.get("[data-testid=commentCount]").should("not.exist");
    });
});
