<script setup lang="ts">
import { computed, onMounted, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import AddCommentComponent from "@/components/private/timeline/comment/AddCommentComponent.vue";
import CommentComponent from "@/components/private/timeline/comment/CommentComponent.vue";
import { CommentEntryType } from "@/constants/commentEntryType";
import { entryTypeMap } from "@/constants/entryType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry from "@/models/timeline/timelineEntry";
import { UserComment } from "@/models/userComment";
import { ILogger } from "@/services/interfaces";
import { useCommentStore } from "@/stores/comment";
import { useUserStore } from "@/stores/user";

interface Props {
    parentEntry: TimelineEntry;
    isMobileDetails?: boolean;
    visible?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    isMobileDetails: false,
    visible: false,
});

const newComment: UserComment = {
    id: "",
    text: "",
    parentEntryId: props.parentEntry.id,
    entryTypeCode:
        entryTypeMap.get(props.parentEntry.type)?.commentType ??
        CommentEntryType.None,
    userProfileId: "",
    createdDateTime: new DateWrapper().toISODate(),
    version: 0,
};

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const userStore = useUserStore();
const commentStore = useCommentStore();

const showComments = ref(false);
const isLoadingComments = ref(false);

const commentButtonText = computed(() =>
    commentCount.value === 1 ? "1 Comment" : `${commentCount.value} Comments`
);
const commentCount = computed(() => props.parentEntry.comments?.length ?? 0);

onMounted(() => {
    // Some comments don't have entry type. This code updates them if they don't.
    const commentsToUpdate: UserComment[] = [];
    if (props.parentEntry.comments !== null) {
        props.parentEntry.comments.forEach((x) => {
            if (x.entryTypeCode === CommentEntryType.None) {
                x.entryTypeCode =
                    entryTypeMap.get(props.parentEntry.type)?.commentType ??
                    CommentEntryType.None;
                x.updatedBy = "System_Backfill";
                commentsToUpdate.push(x);
            }
        });
    }

    commentsToUpdate.forEach((x) => {
        logger.info("Updating comment " + x.id);
        commentStore.updateComment(userStore.user.hdid, x);
    });
});
</script>

<template>
    <div class="my-4">
        <div v-if="commentCount > 0" class="text-right pb-2">
            <HgButtonComponent
                variant="link"
                data-testid="showCommentsBtn"
                :text="commentButtonText"
                @click="showComments = !showComments"
            />
        </div>
        <v-expand-transition
            v-show="showComments"
            :id="'entryComments-' + parentEntry.id"
        >
            <v-skeleton-loader
                v-if="isLoadingComments"
                type="list-item-two-line, list-item-two-line"
                color="grey-lighten-5"
            />
            <div v-else>
                <CommentComponent
                    v-for="comment in parentEntry.comments"
                    :key="comment.id"
                    :comment="comment"
                />
            </div>
        </v-expand-transition>
        <AddCommentComponent
            :comment="newComment"
            :is-mobile-details="isMobileDetails"
            :visible="visible"
            @on-comment-added="showComments = true"
        />
    </div>
</template>
