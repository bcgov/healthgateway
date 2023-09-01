<script setup lang="ts">
import { computed, ref } from "vue";

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
import { useAppStore } from "@/stores/app";
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
const appStore = useAppStore();

const showComments = ref(false);
const isLoadingComments = ref(false);
const commentList = ref();

const commentButtonText = computed(() =>
    commentCount.value === 1 ? "1 Comment" : `${commentCount.value} Comments`
);
const commentCount = computed(() => props.parentEntry.comments?.length ?? 0);

// Update comments with None CommentEntryType.
for (const c of props.parentEntry.comments ?? []) {
    if (c.entryTypeCode === CommentEntryType.None) {
        c.entryTypeCode =
            entryTypeMap.get(props.parentEntry.type)?.commentType ??
            CommentEntryType.None;
        c.updatedBy = "System_Backfill";
        logger.info("Updating comment " + c.id);
        commentStore.updateComment(userStore.hdid, c);
    }
}

function handleCommentExpandChange(): void {
    if (appStore.isMobile) {
        commentList.value?.$el.scrollIntoView({
            behavior: "smooth",
        });
    }
}
</script>

<template>
    <div class="mt-6" :class="{ 'mobile-padding': isMobileDetails }">
        <div v-if="commentCount > 0" class="text-right">
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
            ref="commentList"
            @transitionend="handleCommentExpandChange"
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
        <div
            :class="
                isMobileDetails
                    ? ['fixed-bottom', 'pa-4', 'bg-white']
                    : ['mt-4']
            "
        >
            <AddCommentComponent
                :comment="newComment"
                :is-mobile-details="isMobileDetails"
                :visible="visible"
                @on-comment-added="showComments = true"
            />
        </div>
    </div>
</template>

<style scoped lang="scss">
.fixed-bottom {
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    z-index: 2;
}

.mobile-padding {
    // height required to ensure that the last comment is not hidden by the add comment component
    padding-bottom: 65px;
}
</style>
