import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";
import { useFocusedInputHandler } from "react-native-keyboard-controller";

import { CommentEntryType } from "@/constants/commentEntryType";
import { SpecialAuthorityRequestTimelineEntry } from "@/models/timeline/specialAuthorityRequestTimelineEntry";
import type { UserComment } from "@/models/userComment";
import { services } from "@/services/container";
import { entryCommentsGetter, useCommentStore } from "@/stores/comment.store";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import {
    changeText,
    fireLayout,
    focus,
    inAct,
    isWithin,
    press,
    render,
    screen,
    textOf,
    waitFor,
} from "@/test/ui";
import { confirmAction } from "@/utils/confirm";
import { SpecialAuthorityRequestEntry } from "@/views/private/timeline/entries/SpecialAuthorityRequestEntry";
import { EntryDetailsLayout } from "@/views/private/timeline/EntryDetailsLayout";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);
jest.mock("@/utils/confirm", () => ({
    confirmAction: jest.fn(async () => true),
}));

const existing: UserComment = {
    id: "c1",
    userProfileId: "HDID1",
    parentEntryId: "SA-1",
    text: "Called the pharmacy",
    createdDateTime: "2024-01-12T10:00:00",
    version: 1,
    entryTypeCode: CommentEntryType.SpecialAuthorityRequest,
};
const request = {
    drugName: "OZEMPIC",
    requestStatus: "Approved",
    requestedDate: "2024-01-10",
    referenceNumber: "SA-1",
};

/** The card renders with the store's comments, as the timeline builds it */
function Card({ isMobileDetails = false }: { isMobileDetails?: boolean }) {
    const comments = useCommentStore((s) => s.comments);
    return (
        <SpecialAuthorityRequestEntry
            entry={
                new SpecialAuthorityRequestTimelineEntry(
                    request,
                    entryCommentsGetter(comments)
                )
            }
            hdid="HDID1"
            commentsAreEnabled
            isMobileDetails={isMobileDetails}
        />
    );
}

describe("comments on a timeline entry (CommentSection / Comment / AddComment ports)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        signInRegistered();
        updateStores(() =>
            useCommentStore.setState({ comments: { "SA-1": [existing] } })
        );
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
        jest.clearAllMocks();
    });

    it("the header shows the comment icon; '1 Comment' toggles the list; posting adds a comment", async () => {
        jest.spyOn(services.userComment, "createComment").mockImplementation(
            async (_h, c) => ({
                ...c,
                id: "c2",
                createdDateTime: "2024-01-13T10:00:00",
            })
        );
        await render(<Card />);
        expect(screen.getByTestId("commentIcon")).toBeTruthy();
        expect(screen.queryByTestId("commentCount")).toBeNull();
        await press(screen.getByLabelText(/OZEMPIC/));
        expect(textOf(screen.getByTestId("showCommentsBtn"))).toBe("1 Comment");
        await press(screen.getByTestId("showCommentsBtn"));
        expect(textOf(screen.getByTestId("commentText"))).toBe(
            "Called the pharmacy"
        );
        await changeText(
            screen.getByTestId("add-comment-text-area"),
            "Approved for a year"
        );
        await press(screen.getByTestId("post-comment-btn"));
        await waitFor(() =>
            expect(screen.getAllByTestId("commentText")).toHaveLength(2)
        );
        expect(textOf(screen.getByTestId("commentCount"))).toBe("2");
        expect(services.userComment.createComment).toHaveBeenCalledWith(
            "HDID1",
            expect.objectContaining({
                text: "Approved for a year",
                parentEntryId: "SA-1",
                entryTypeCode: "SAR",
            })
        );
    });

    it("full screen: the field is pinned under the scrolling entry, and posting there opens the list", async () => {
        jest.spyOn(services.userComment, "createComment").mockImplementation(
            async (_h, c) => ({
                ...c,
                id: "c2",
                createdDateTime: "2024-01-13T10:00:00",
            })
        );
        setBreakpoint(390);
        await render(
            <EntryDetailsLayout testID="entryDetailsScroll">
                <Card isMobileDetails />
            </EntryDetailsLayout>
        );
        const field = screen.getByTestId("add-comment-text-area");
        expect(isWithin(field, screen.getByTestId("entryDetailsFooter"))).toBe(
            true
        );
        expect(isWithin(field, screen.getByTestId("entryDetailsScroll"))).toBe(
            false
        );
        await changeText(field, "Approved for a year");
        await press(screen.getByTestId("post-comment-btn"));
        await waitFor(() =>
            expect(screen.getAllByTestId("commentText")).toHaveLength(2)
        );
    });

    it("full screen: while an edited comment's field has focus, all of the comment's box under the field is kept above the keyboard", async () => {
        // Web: the browser, not the keyboard library, keeps the field in sight
        if (Platform.OS === "web") return;
        setBreakpoint(390);
        await render(
            <EntryDetailsLayout testID="entryDetailsScroll">
                <Card isMobileDetails />
            </EntryDetailsLayout>
        );
        const bottomOffset = () =>
            screen.getByTestId("entryDetailsScroll").props
                .bottomOffset as number;
        await press(screen.getByTestId("showCommentsBtn"));
        await press(screen.getByTestId("commentMenuBtn"));
        await press(screen.getByTestId("commentMenuEditBtn"));
        expect(
            String(screen.getByTestId("editCommentFieldArea").props.className)
        ).not.toMatch(/\bflex-1\b/);
        // Caret not yet known: all of the box under the field's top is kept
        await fireLayout(screen.getByTestId("commentBox"), { height: 120 });
        const withoutButtons = bottomOffset();
        await focus(screen.getByTestId("editCommentInput"));
        expect(bottomOffset()).toBe(withoutButtons + 120);
        await press(screen.getByText("Cancel"));
        expect(bottomOffset()).toBe(withoutButtons);
    });

    it("full screen: the edited field's caret, reported as it gains focus and before its onFocus, reduces the space kept under it", async () => {
        if (Platform.OS === "web") return;
        setBreakpoint(390);
        await render(
            <EntryDetailsLayout testID="entryDetailsScroll">
                <Card isMobileDetails />
            </EntryDetailsLayout>
        );
        const bottomOffset = () =>
            screen.getByTestId("entryDetailsScroll").props
                .bottomOffset as number;
        await press(screen.getByTestId("showCommentsBtn"));
        await press(screen.getByTestId("commentMenuBtn"));
        await press(screen.getByTestId("commentMenuEditBtn"));
        await fireLayout(screen.getByTestId("commentBox"), { height: 120 });
        const withoutButtons = bottomOffset();
        const caret = { x: 0, y: 48, position: 0 };
        await inAct(() =>
            jest
                .mocked(useFocusedInputHandler)
                .mock.calls.at(-1)?.[0]
                .onSelectionChange?.({
                    target: 1,
                    selection: { start: caret, end: caret },
                })
        );
        await focus(screen.getByTestId("editCommentInput"));
        expect(bottomOffset()).toBe(withoutButtons + 120 - 48);
    });

    it("Edit saves the new text; Delete asks first", async () => {
        jest.spyOn(services.userComment, "updateComment").mockImplementation(
            async (_h, c) => c
        );
        jest.spyOn(services.userComment, "deleteComment").mockResolvedValue(
            undefined
        );
        await render(<Card />);
        await press(screen.getByLabelText(/OZEMPIC/));
        await press(screen.getByTestId("showCommentsBtn"));
        await press(screen.getByTestId("commentMenuBtn"));
        await press(screen.getByTestId("commentMenuEditBtn"));
        await changeText(screen.getByTestId("editCommentInput"), "Edited");
        await press(screen.getByTestId("saveCommentBtn"));
        await waitFor(() =>
            expect(textOf(screen.getByTestId("commentText"))).toBe("Edited")
        );
        await press(screen.getByTestId("commentMenuBtn"));
        await press(screen.getByTestId("commentMenuDeleteBtn"));
        await waitFor(() =>
            expect(services.userComment.deleteComment).toHaveBeenCalled()
        );
        expect(confirmAction).toHaveBeenCalledWith(
            "Are you sure you want to delete this comment?"
        );
    });
});
