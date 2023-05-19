<script setup lang="ts">
import { BCollapse } from "bootstrap-vue";
import { ComponentPublicInstance, computed, onMounted, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import AddCommentComponent from "@/components/timeline/entryCard/AddCommentComponent.vue";
import CommentComponent from "@/components/timeline/entryCard/CommentComponent.vue";
import { CommentEntryType } from "@/constants/commentEntryType";
import { entryTypeMap } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry from "@/models/timelineEntry";
import User from "@/models/user";
import { UserComment } from "@/models/userComment";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

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
const store = useStore();

const showComments = ref(false);
const isLoadingComments = ref(false);

const collapseComponent = ref<ComponentPublicInstance | null>();

const user = computed<User>(() => store.getters["user/user"]);
const commentCount = computed(() => props.parentEntry.comments?.length ?? 0);
const collapseElement = computed<Element | undefined>(
    () => collapseComponent?.value?.$el
);

function updateComment(
    hdid: string,
    comment: UserComment
): Promise<UserComment> {
    return store.dispatch("comment/updateComment", { hdid, comment });
}

function onSectionExpand(event: Event): void {
    if (props.isMobileDetails && showComments.value) {
        const transitionEvent = event as TransitionEvent;
        if (
            collapseElement.value !== transitionEvent.target ||
            transitionEvent.propertyName !== "height"
        ) {
            return;
        }
        collapseElement.value?.scrollIntoView({ behavior: "smooth" });
    }
}

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
        updateComment(user.value.hdid, x);
    });

    collapseElement.value?.addEventListener("transitionend", onSectionExpand);
});
</script>

<template>
    <b-row class="pt-2">
        <b-col>
            <b-row class="pt-2">
                <b-col>
                    <div
                        v-if="commentCount > 0"
                        class="d-flex flex-row-reverse"
                    >
                        <b-btn
                            variant="link"
                            class="py-2"
                            data-testid="showCommentsBtn"
                            @click="showComments = !showComments"
                        >
                            <span>
                                {{
                                    commentCount == 1
                                        ? "1 Comment"
                                        : `${commentCount} Comments"`
                                }}</span
                            >
                        </b-btn>
                    </div>
                </b-col>
            </b-row>
            <b-row class="py-2">
                <b-col>
                    <b-collapse
                        :id="'entryComments-' + parentEntry.id"
                        ref="collapseComponent"
                        v-model="showComments"
                    >
                        <div v-if="!isLoadingComments">
                            <div
                                v-for="comment in parentEntry.comments"
                                :key="comment.id"
                            >
                                <CommentComponent :comment="comment" />
                            </div>
                        </div>
                        <div v-else>
                            <div class="d-flex align-items-center">
                                <strong>Loading...</strong>
                                <b-spinner class="ml-5"></b-spinner>
                            </div>
                        </div>
                    </b-collapse>
                </b-col>
            </b-row>
            <div :class="{ push: isMobileDetails }"></div>
            <AddCommentComponent
                class="pb-2"
                :class="{
                    'fixed-bottom p-3 comment-input': isMobileDetails,
                }"
                :comment="newComment"
                :is-mobile-details="isMobileDetails"
                :visible="visible"
                @on-comment-added="showComments = true"
            />
        </b-col>
    </b-row>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.col {
    padding: 0px;
    margin: 0px;
}

.row {
    padding: 0;
    margin: 0px;
}

.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
    display: none;
}

.comment-input {
    border-top: 1px $primary solid;
    background-color: white;
}

.push {
    height: 60px;
}
</style>
