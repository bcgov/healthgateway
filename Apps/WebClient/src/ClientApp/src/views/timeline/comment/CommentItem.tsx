import { useState } from "react";
import { Platform, Pressable, View } from "react-native";

import { AppText, HgButton, HgMenu, HgTextField, Icon } from "@/components/ui";
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
        <View className="px-4 py-2 mt-2 bg-table-header rounded">
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
                <View className={cn(isMobile ? "" : "flex-row items-center")}>
                    <View className="flex-1 py-2">
                        <HgTextField
                            testID="editCommentInput"
                            nativeID="comment-input"
                            accessibilityLabel="Edit comment"
                            value={commentInput}
                            onChangeText={setCommentInput}
                            placeholder="Editing a comment"
                            multiline
                            maxLength={1000}
                            editable={!isUpdating}
                        />
                    </View>
                    <View
                        className={cn(
                            "flex-row items-center",
                            !isMobile && "ml-3"
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
