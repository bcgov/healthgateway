import { DateTime } from "luxon";
import { useState } from "react";
import { ActivityIndicator, Pressable, View } from "react-native";

import { HgTextField, Icon, InfoTooltip } from "@/components/common";
import type { CommentEntryType } from "@/constants/commentEntryType";
import { TooManyRequestsLocation } from "@/constants/errorType";
import { useThemeColors } from "@/hooks/useTheme";
import { isSessionEnded, ResultError } from "@/models/errors";
import type { UserComment } from "@/models/userComment";
import { describeError, logger } from "@/services/logger";
import { useCommentStore } from "@/stores/comment.store";
import { useErrorStore } from "@/stores/error.store";
import { useUserStore } from "@/stores/user.store";
import { cn } from "@/utils/cn";

/** Port of timeline/comment/AddCommentComponent.vue */
export function AddComment({
    parentEntryId,
    entryTypeCode,
    onCommentAdded,
    onError,
}: {
    parentEntryId: string;
    entryTypeCode: CommentEntryType;
    onCommentAdded?: (comment: UserComment) => void;
    onError?: () => void;
}) {
    const colors = useThemeColors();
    const hdid = useUserStore((s) => s.user.hdid);
    const createComment = useCommentStore((s) => s.createComment);
    const [isSaving, setIsSaving] = useState(false);
    const [commentInput, setCommentInput] = useState("");

    function addComment() {
        setIsSaving(true);
        createComment(hdid, {
            id: "00000000-0000-0000-0000-000000000000",
            text: commentInput,
            parentEntryId,
            userProfileId: hdid,
            entryTypeCode,
            version: 0,
            createdDateTime: DateTime.now().toISO() ?? "",
        })
            .then((newComment) => {
                if (newComment !== undefined) {
                    setCommentInput("");
                    onCommentAdded?.(newComment);
                }
            })
            .catch((error: unknown) => {
                if (isSessionEnded(error)) return;
                logger.error(`Error adding comment: ${describeError(error)}`);
                if (error instanceof ResultError && error.statusCode === 429)
                    useErrorStore
                        .getState()
                        .setTooManyRequestsError(TooManyRequestsLocation.Page);
                onError?.();
            })
            .finally(() => setIsSaving(false));
    }

    return (
        <View className="flex-row items-center p-1">
            {/* #prepend: the lock with its tooltip */}
            <View className="mr-2">
                <InfoTooltip
                    accessibilityLabel="Who can see comments"
                    icon="lock"
                    color={colors["bc-black"]}
                    size={16}
                    text="Only you can see comments added to your medical records."
                />
            </View>
            <View className="flex-1">
                <HgTextField
                    testID="add-comment-text-area"
                    accessibilityLabel="Write a comment"
                    value={commentInput}
                    onChangeText={setCommentInput}
                    placeholder="Write a comment"
                    multiline
                    maxLength={1000}
                    className="mb-0"
                    appendInner={
                        <Pressable
                            testID="post-comment-btn"
                            accessibilityRole="button"
                            accessibilityLabel="Post comment"
                            disabled={commentInput === "" || isSaving}
                            onPress={addComment}
                            className={cn(
                                "w-7 h-7 rounded-full items-center justify-center border border-secondary",
                                commentInput === "" && "opacity-40"
                            )}
                        >
                            {isSaving ? (
                                <ActivityIndicator
                                    size="small"
                                    color={colors.secondary}
                                />
                            ) : (
                                <Icon
                                    name="arrow-up"
                                    size={12}
                                    color={colors.secondary}
                                />
                            )}
                        </Pressable>
                    }
                />
            </View>
        </View>
    );
}
