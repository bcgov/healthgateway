import { useEffect, useState } from "react";
import { View } from "react-native";

import { HgButton } from "@/components/common";
import { useScrollToTop } from "@/components/site/PageScrollContext";
import { CommentEntryType } from "@/constants/commentEntryType";
import { getEntryTypeDetails } from "@/constants/entryType";
import type { TimelineEntry } from "@/models/timeline/timelineEntry";
import { selectEntryComments, useCommentStore } from "@/stores/comment.store";
import { useUserStore } from "@/stores/user.store";

import { usePinnedFooter } from "../EntryDetailsLayout";
import { AddComment } from "./AddComment";
import { CommentItem } from "./CommentItem";

/** Port of timeline/comment/CommentSectionComponent.vue */
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
    const comments = useCommentStore(selectEntryComments(parentEntry.id)) ?? [];
    const commentCount = comments.length;
    const commentButtonText =
        commentCount === 1 ? "1 Comment" : `${commentCount} Comments`;

    // backfill comments saved without an entry type
    useEffect(() => {
        for (const c of comments)
            if (c.entryTypeCode === CommentEntryType.None) {
                updateComment(hdid, {
                    ...c,
                    entryTypeCode,
                    updatedBy: "System_Backfill",
                }).catch(() => undefined);
            }
        // eslint-disable-next-line react-hooks/exhaustive-deps -- once, when the section is created
    }, []);

    const addComment = (
        <AddComment
            parentEntryId={parentEntry.id}
            entryTypeCode={entryTypeCode}
            onCommentAdded={() => setShowComments(true)}
            onError={scrollToTop}
        />
    );
    const pinned = usePinnedFooter(isMobileDetails ? addComment : undefined);

    return (
        <View>
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
            {pinned ? null : (
                <View className={isMobileDetails ? "p-4" : "mt-3 py-3"}>
                    {addComment}
                </View>
            )}
        </View>
    );
}
