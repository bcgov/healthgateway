const { AuthMethod } = require("../../../support/constants");
const { waitForTimelineCard } = require("../../../support/functions/timeline");

const commentSelector = "[data-testid=commentText]";

function removeTestCommentsIfPresent(
    commentTexts,
    matchPrefixes = false,
    collapseWhenDone = false
) {
    cy.get("body").then(($body) => {
        const $card = $body.find('[data-testid="timelineCard"]').first();
        if (!$card.length) {
            return;
        }

        // A failed assertion can leave the card either expanded or collapsed.
        if (!$card.find("[data-testid=add-comment-text-area]:visible").length) {
            cy.wrap($card)
                .find("[data-testid=entryCardDetailsTitle]")
                .click({ force: true });
        }

        cy.wrap($card).then(($expandedCard) => {
            const $showCommentsButton = $expandedCard
                .find("[data-testid=showCommentsBtn]:visible")
                .filter((_, element) => element.textContent.includes("Show"));

            if ($showCommentsButton.length) {
                cy.wrap($showCommentsButton).click({ force: true });
            }
        });

        cy.get("body").then(($updatedBody) => {
            const matchingComment = [
                ...$updatedBody.find(commentSelector),
            ].find((element) => {
                const commentText = element.textContent.trim();
                return commentTexts.some((candidate) =>
                    matchPrefixes
                        ? commentText.startsWith(candidate)
                        : commentText === candidate
                );
            });

            if (!matchingComment) {
                if (
                    collapseWhenDone &&
                    $updatedBody.find(
                        '[data-testid="timelineCard"]:first [data-testid=add-comment-text-area]:visible'
                    ).length
                ) {
                    cy.get('[data-testid="timelineCard"]')
                        .first()
                        .find("[data-testid=entryCardDetailsTitle]")
                        .click({ force: true });
                }
                return;
            }

            cy.wrap(matchingComment)
                .closest(".v-sheet")
                .find("[data-testid=commentMenuBtn]")
                .as("cleanupCommentMenu");
            cy.get("@cleanupCommentMenu").click({ force: true });
            cy.document()
                .find("[data-testid=commentMenuDeleteBtn]")
                .first()
                .click({ force: true });
            cy.wait("@deleteComment");

            // Remove both the original and edited forms if a failure happened
            // while the save request was in progress.
            removeTestCommentsIfPresent(
                commentTexts,
                matchPrefixes,
                collapseWhenDone
            );
        });
    });
}

describe("Comments Disable", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "medication",
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
        cy.intercept("POST", "**/UserProfile/*/Comment*").as("postComment");
        cy.intercept("PUT", "**/UserProfile/*/Comment*").as("updateComment");
        cy.intercept("DELETE", "**/UserProfile/*/Comment*").as("deleteComment");
        cy.configureSettings({
            timeline: {
                comment: true,
            },
            datasets: [
                {
                    name: "medication",
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
        waitForTimelineCard();

        // Previous interrupted runs may have left an automation comment on the
        // shared account. Remove only comments created by this spec, then put
        // the card back into the state expected at the start of the workflow.
        cy.on("window:confirm", () => true);
        removeTestCommentsIfPresent(
            ["Test Add Comment", "Test Edit Comment"],
            true,
            true
        );
    });

    it("Validates the comment CRUD workflow", () => {
        // A failed attempt can leave its server-side comment behind. Unique
        // text keeps a retry independent from comments created by that attempt.
        const uniqueId = `${Date.now()}-${Cypress._.random(1000000)}`;
        const testComment = `Test Add Comment ${uniqueId}`;
        const testEditComment = `Test Edit Comment ${uniqueId}`;

        cy.get('[data-testid="timelineCard"]')
            .first()
            .within(() => {
                cy.get("[data-testid=commentIcon]").should("not.exist");
                cy.get("[data-testid=commentCount]").should("not.exist");
            });

        cy.get('[data-testid="timelineCard"]')
            .first()
            .within(() => {
                cy.get("[data-testid=entryCardDetailsTitle]").click({
                    force: true,
                });

                // Add comment
                cy.get("[data-testid=add-comment-text-area]")
                    .should("be.visible")
                    .type(testComment);

                cy.get("[data-testid=post-comment-btn]")
                    .should("be.visible")
                    .and("not.be.disabled")
                    .click();

                cy.wait("@postComment");

                // Verify
                cy.get("[data-testid=commentText]").contains(testComment);
                cy.get("[data-testid=commentIcon]").should("exist");
                cy.get("[data-testid=commentCount]").should("not.exist");
            });

        // Edit while the card is in its normal, unfiltered state. Applying a
        // text filter re-renders the card and closes Vuetify's teleported menu.
        cy.get("[data-testid=commentMenuBtn]").first().click({ force: true });
        cy.document()
            .find("[data-testid=commentMenuEditBtn]")
            .click({ force: true });
        cy.get("[data-testid=editCommentInput] textarea")
            .filter(":visible")
            .clear()
            .type(testEditComment);
        cy.get("[data-testid=saveCommentBtn]").filter(":visible").click();
        cy.wait("@updateComment");

        cy.get('[data-testid="timelineCard"]')
            .first()
            .within(() => {
                cy.contains("[data-testid=commentText]", testEditComment);
                cy.get("[data-testid=commentIcon]").should("exist");
                cy.get("[data-testid=commentCount]").should("not.exist");
            });

        // Filtering causes the card to render again in its collapsed state.
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterTextInput]").type(testEditComment);
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");

        cy.get('[data-testid="timelineCard"]')
            .first()
            .within(() => {
                cy.get("[data-testid=entryCardDetailsTitle]").click({
                    force: true,
                });
                cy.get("[data-testid=showCommentsBtn]").click();

                // Verify the generic timeline text filter searches comments.
                cy.contains("[data-testid=commentText]", testEditComment);
                cy.get("[data-testid=commentIcon]").should("exist");
                cy.get("[data-testid=commentCount]").should("not.exist");
            });

        // Return to the normal timeline before using the comment menu again.
        cy.get("[data-testid=clear-filters-button]").click();
        cy.get('[data-testid="timelineCard"]')
            .first()
            .within(() => {
                cy.get("[data-testid=entryCardDetailsTitle]").click({
                    force: true,
                });
                cy.get("[data-testid=showCommentsBtn]").click();
                cy.contains("[data-testid=commentText]", testEditComment);
            });

        cy.get("[data-testid=commentMenuBtn]").first().click({ force: true });
        cy.on("window:confirm", (str) => {
            expect(str).to.eq("Are you sure you want to delete this comment?");
        });
        cy.document()
            .find("[data-testid=commentMenuDeleteBtn]")
            .first()
            .click({ force: true });
        cy.wait("@deleteComment");

        cy.get('[data-testid="timelineCard"]')
            .first()
            .within(() => {
                // Verify
                cy.contains(
                    "[data-testid=commentText]",
                    testEditComment
                ).should("not.exist");
                cy.get("[data-testid=commentIcon]").should("not.exist");
                cy.get("[data-testid=commentCount]").should("not.exist");
            });
    });
});
