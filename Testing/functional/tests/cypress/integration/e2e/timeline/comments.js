const { AuthMethod } = require("../../../support/constants");
const { waitForTimelineCard } = require("../../../support/functions/timeline");

const commentSelector = "[data-testid=commentText]";

function getSpecialAuthorityCard() {
    return cy
        .get("[data-testid=specialauthorityrequestTitle]")
        .first()
        .closest('[data-testid="timelineCard"]');
}

function removeTestCommentsIfPresent(
    commentTexts,
    matchPrefixes = false,
    collapseWhenDone = false
) {
    getSpecialAuthorityCard().then(($card) => {
        // A failed assertion can leave the card either expanded or collapsed.
        if (!$card.find("[data-testid=add-comment-text-area]:visible").length) {
            getSpecialAuthorityCard()
                .find("[data-testid=specialauthorityrequestTitle]")
                .click({ force: true });
        }

        getSpecialAuthorityCard().then(($expandedCard) => {
            const $showCommentsButton = $expandedCard
                .find("[data-testid=showCommentsBtn]:visible")
                .filter((_, element) => element.textContent.includes("Show"));

            if ($showCommentsButton.length) {
                cy.wrap($showCommentsButton).click({ force: true });
            }
        });

        getSpecialAuthorityCard().then(($updatedCard) => {
            const matchingComment = [
                ...$updatedCard.find(commentSelector),
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
                    $updatedCard.find(
                        "[data-testid=add-comment-text-area]:visible"
                    ).length
                ) {
                    getSpecialAuthorityCard()
                        .find("[data-testid=specialauthorityrequestTitle]")
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
                    name: "specialAuthorityRequest",
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
                    name: "specialAuthorityRequest",
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

        getSpecialAuthorityCard().within(() => {
            cy.get("[data-testid=commentIcon]").should("not.exist");
            cy.get("[data-testid=commentCount]").should("not.exist");
        });

        getSpecialAuthorityCard().within(() => {
            cy.get("[data-testid=specialauthorityrequestTitle]").click({
                force: true,
            });
        });

        // Re-query the Special Authority card and target its editable textarea.
        getSpecialAuthorityCard()
            .find("[data-testid=add-comment-text-area] textarea")
            .filter(":visible")
            .should("have.length", 1)
            .and("be.visible")
            .type(testComment);

        getSpecialAuthorityCard()
            .find("[data-testid=post-comment-btn]")
            .filter(":visible")
            .should("be.visible")
            .and("not.be.disabled")
            .click();

        cy.wait("@postComment");

        // Verify
        getSpecialAuthorityCard().find(commentSelector).contains(testComment);
        getSpecialAuthorityCard()
            .find("[data-testid=commentIcon]")
            .should("exist");
        getSpecialAuthorityCard()
            .find("[data-testid=commentCount]")
            .should("not.exist");

        // Edit while the card is in its normal, unfiltered state. Applying a
        // text filter re-renders the card and closes Vuetify's teleported menu.
        getSpecialAuthorityCard()
            .find("[data-testid=commentMenuBtn]")
            .first()
            .click({ force: true });
        cy.document()
            .find("[data-testid=commentMenuEditBtn]")
            .click({ force: true });
        cy.get("[data-testid=editCommentInput] textarea")
            .filter(":visible")
            .clear()
            .type(testEditComment);
        cy.get("[data-testid=saveCommentBtn]").filter(":visible").click();
        cy.wait("@updateComment");

        getSpecialAuthorityCard().within(() => {
            cy.contains("[data-testid=commentText]", testEditComment);
            cy.get("[data-testid=commentIcon]").should("exist");
            cy.get("[data-testid=commentCount]").should("not.exist");
        });

        // Filtering causes the card to render again in its collapsed state.
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterTextInput]").type(testEditComment);
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");

        getSpecialAuthorityCard().within(() => {
            cy.get("[data-testid=specialauthorityrequestTitle]").click({
                force: true,
            });
            cy.get("[data-testid=showCommentsBtn]")
                .should("be.visible")
                .click({ waitForAnimations: false });

            // Verify the generic timeline text filter searches comments.
            cy.contains("[data-testid=commentText]", testEditComment);
            cy.get("[data-testid=commentIcon]").should("exist");
            cy.get("[data-testid=commentCount]").should("not.exist");
        });

        // Return to the normal timeline before using the comment menu again.
        cy.get("[data-testid=clear-filters-button]").click();
        getSpecialAuthorityCard().within(() => {
            cy.get("[data-testid=specialauthorityrequestTitle]").click({
                force: true,
            });
            cy.get("[data-testid=showCommentsBtn]")
                .should("be.visible")
                .click({ waitForAnimations: false });

            cy.contains("[data-testid=commentText]", testEditComment);
        });

        getSpecialAuthorityCard()
            .find("[data-testid=commentMenuBtn]")
            .first()
            .click({ force: true });
        cy.on("window:confirm", (str) => {
            expect(str).to.eq("Are you sure you want to delete this comment?");
        });
        cy.document()
            .find("[data-testid=commentMenuDeleteBtn]")
            .first()
            .click({ force: true });
        cy.wait("@deleteComment");

        getSpecialAuthorityCard().within(() => {
            // Verify
            cy.contains("[data-testid=commentText]", testEditComment).should(
                "not.exist"
            );
            cy.get("[data-testid=commentIcon]").should("not.exist");
            cy.get("[data-testid=commentCount]").should("not.exist");
        });
    });
});
