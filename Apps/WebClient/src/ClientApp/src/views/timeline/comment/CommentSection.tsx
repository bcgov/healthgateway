import { useEffect, useState } from "react";
import { View } from "react-native";

import { useScrollToTop } from "@/components/site/PageScrollContext";
import { HgButton } from "@/components/ui";
import { CommentEntryType } from "@/constants/commentEntryType";
import { getEntryTypeDetails } from "@/constants/entryType";
import type { TimelineEntry } from "@/models/timeline/timelineEntry";
import { useCommentStore } from "@/stores/comment.store";
import { useUserStore } from "@/stores/user.store";
import { cn } from "@/utils/cn";

import { AddComment } from "./AddComment";
import { CommentItem } from "./CommentItem";

/**
 * Port of timeline/comment/CommentSectionComponent.vue: "N Comments" toggles the list; the add-comment field
 * follows. (The web client pins the field to the bottom of the full-screen details; here it ends the details.)
 */
export function CommentSection({
    parentEntry,
    isMobileDetails = false,
}: {
    parentEntry: TimelineEntry;
    isMobileDetails?: boolean;
}) {
    const hdid = useUserStore((s) => s.user.hdid);
    const updateComment = useCommentStore((s) => s.updateComment);
    const scrollToTop = useScrollToTop();
    const [showComments, setShowComments] = useState(false);
    const entryTypeCode =
        getEntryTypeDetails(parentEntry.type)?.commentType ??
        CommentEntryType.None;
    const comments = parentEntry.comments ?? [];
    const commentCount = comments.length;
    const commentButtonText =
        commentCount === 1 ? "1 Comment" : `${commentCount} Comments`;

    // backfill comments saved without an entry type
    useEffect(() => {
        for (const c of parentEntry.comments ?? [])
            if (c.entryTypeCode === CommentEntryType.None) {
                updateComment(hdid, {
                    ...c,
                    entryTypeCode,
                    updatedBy: "System_Backfill",
                }).catch(() => undefined);
            }
        // eslint-disable-next-line react-hooks/exhaustive-deps -- once, when the section is created
    }, []);

    return (
        <View className={cn("mt-6", isMobileDetails && "pb-4")}>
            {commentCount > 0 ? (
                <View className="items-end">
                    <HgButton
                        variant="link"
                        testID="showCommentsBtn"
                        className="self-end"
                        text={commentButtonText}
                        onPress={() => setShowComments((v) => !v)}
                    />
                </View>
            ) : null}
            {showComments ? (
                <View nativeID={`entryComments-${parentEntry.id}`}>
                    {comments.map((comment) => (
                        <CommentItem
                            key={comment.id}
                            comment={comment}
                            onError={scrollToTop}
                        />
                    ))}
                </View>
            ) : null}
            <View className={isMobileDetails ? "p-4" : "mt-4"}>
                <AddComment
                    parentEntryId={parentEntry.id}
                    entryTypeCode={entryTypeCode}
                    onCommentAdded={() => setShowComments(true)}
                    onError={scrollToTop}
                />
            </View>
        </View>
    );
}
