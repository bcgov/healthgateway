import { useState } from "react";
import { Platform, Pressable, View } from "react-native";

import {
    AppText,
    HgButton,
    HgMenu,
    HgTextField,
    Icon,
} from "@/components/common";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { useThemeColors } from "@/hooks/useTheme";
import { isSessionEnded } from "@/models/errors";
import type { UserComment } from "@/models/userComment";
import { describeError, logger } from "@/services/logger";
import { useCommentStore } from "@/stores/comment.store";
import { useUserStore } from "@/stores/user.store";
import { cn } from "@/utils/cn";
import { confirmAction } from "@/utils/confirm";
import { formatDate, fromIso, isPacificTime } from "@/utils/dateUtil";

import { useKeyboardClearanceBelowField } from "../EntryDetailsLayout";

const formatCommentDate = (date: string) =>
    formatDate(
        fromIso(date),
        isPacificTime() ? "yyyy-MMM-dd, t" : "yyyy-MMM-dd, t ZZZZ"
    );

/** Port of timeline/comment/CommentComponent.vue */
export function CommentItem({
    comment,
    onError,
}: {
    comment: UserComment;
    onError?: () => void;
}) {
    const colors = useThemeColors();
    const { isMobile } = useBreakpoint();
    const hdid = useUserStore((s) => s.user.hdid);
    const updateComment = useCommentStore((s) => s.updateComment);
    const deleteComment = useCommentStore((s) => s.deleteComment);
    const [commentInput, setCommentInput] = useState("");
    const [isEditMode, setIsEditMode] = useState(false);
    const [isUpdating, setIsUpdating] = useState(false);
    // While editing in full screen, everything from the field's top to this box's bottom (Save and Cancel included) stays
    // above the keyboard in the native apps. The offsets are measured, so padding and margins here can change freely.
    const [boxHeight, setBoxHeight] = useState(0);
    const [editRowTop, setEditRowTop] = useState(0);
    const [fieldTop, setFieldTop] = useState(0);
    const keepEditRowInSight = useKeyboardClearanceBelowField(
        boxHeight - editRowTop - fieldTop,
        isEditMode
    );

    function handleCommentError(error: unknown) {
        if (isSessionEnded(error)) return;
        logger.error(describeError(error));
        onError?.();
    }

    function onSubmit() {
        setIsUpdating(true);
        updateComment(hdid, {
            id: comment.id,
            text: commentInput,
            userProfileId: comment.userProfileId,
            parentEntryId: comment.parentEntryId,
            createdDateTime: comment.createdDateTime,
            entryTypeCode: comment.entryTypeCode,
            version: comment.version,
        })
            .then(() => {
                logger.info("Comment Updated");
                setIsEditMode(false);
            })
            .catch(handleCommentError)
            .finally(() => setIsUpdating(false));
    }

    async function removeComment() {
        if (
            !(await confirmAction(
                "Are you sure you want to delete this comment?"
            ))
        )
            return;
        setIsUpdating(true);
        deleteComment(hdid, comment)
            .then(() => logger.info("Comment removed"))
            .catch(handleCommentError)
            .finally(() => setIsUpdating(false));
    }

    return (
        // v-sheet grey-lighten-5
        <View
            testID="commentBox"
            className="px-4 py-2 mt-3 bg-table-header rounded"
            onLayout={(e) => setBoxHeight(e.nativeEvent.layout.height)}
        >
            {!isEditMode ? (
                <View className="flex-row items-start">
                    <View className="flex-1 py-3">
                        <AppText testID="commentText">{comment.text}</AppText>
                        <AppText className="text-body-small text-bc-grey">
                            {formatCommentDate(comment.createdDateTime)}
                        </AppText>
                    </View>
                    <HgMenu
                        width={140}
                        align="end"
                        items={[
                            {
                                title: "Edit",
                                testID: "commentMenuEditBtn",
                                onPress: () => {
                                    setCommentInput(comment.text);
                                    setIsEditMode(true);
                                },
                            },
                            {
                                title: "Delete",
                                testID: "commentMenuDeleteBtn",
                                onPress: () => void removeComment(),
                            },
                        ]}
                    >
                        {(open, isOpen) => (
                            <Pressable
                                testID="commentMenuBtn"
                                accessibilityRole="button"
                                accessibilityLabel="Options for comment"
                                aria-expanded={isOpen}
                                {...(Platform.OS === "web"
                                    ? ({ "aria-haspopup": "menu" } as object)
                                    : {})}
                                onPress={open}
                                className="p-2 mt-1"
                            >
                                <Icon
                                    name="ellipsis-vertical"
                                    size={16}
                                    color={colors["bc-black"]}
                                />
                            </Pressable>
                        )}
                    </HgMenu>
                </View>
            ) : (
                <View
                    className={cn(
                        "py-2",
                        isMobile ? "" : "flex-row items-center"
                    )}
                    onLayout={(e) => setEditRowTop(e.nativeEvent.layout.y)}
                >
                    <View
                        testID="editCommentFieldArea"
                        // at mobile sizes, the input takes up the whole width; otherwise the buttons appear to the right
                        className={cn(!isMobile && "flex-1")}
                        // the text box starts at this wrapper's top, so anything added above it will skew the measurement
                        onLayout={(e) => setFieldTop(e.nativeEvent.layout.y)}
                    >
                        <HgTextField
                            testID="editCommentInput"
                            nativeID="comment-input"
                            accessibilityLabel="Edit comment"
                            value={commentInput}
                            onChangeText={setCommentInput}
                            placeholder="Editing a comment"
                            multiline
                            autoGrow
                            // With the keyboard up on a small phone, a taller field won't fit above it with Save and
                            // Cancel visible, so the field would scroll partly off screen
                            maxLines={6}
                            maxLength={1000}
                            editable={!isUpdating}
                            {...keepEditRowInSight}
                        />
                    </View>
                    <View
                        className={cn(
                            "flex-row items-center",
                            // Under the field: mt-2 plus the field's mb-2 gives 16px
                            isMobile ? "mt-2" : "ml-3"
                        )}
                    >
                        <HgButton
                            testID="saveCommentBtn"
                            className="mr-2"
                            text="Save"
                            disabled={commentInput === ""}
                            loading={isUpdating}
                            onPress={onSubmit}
                        />
                        <HgButton
                            variant="secondary"
                            text="Cancel"
                            onPress={() => setIsEditMode(false)}
                        />
                    </View>
                </View>
            )}
        </View>
    );
}
