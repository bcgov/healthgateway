<script setup lang="ts">
import { ref } from "vue";

import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import type { UserComment } from "@/models/userComment";
import { ILogger } from "@/services/interfaces";
import { useCommentStore } from "@/stores/comment";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";

interface Props {
    comment: UserComment;
    visible?: boolean;
    isMobileDetails?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    visible: false,
    isMobileDetails: false,
});

const emit = defineEmits<{
    (e: "on-comment-added", newValue: UserComment): UserComment;
}>();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const userStore = useUserStore();
const errorStore = useErrorStore();
const commentStore = useCommentStore();

const isSaving = ref(false);
const commentInput = ref("");

function addComment(): void {
    isSaving.value = true;
    commentStore
        .createComment(userStore.user.hdid, {
            id: "00000000-0000-0000-0000-000000000000",
            text: commentInput.value,
            parentEntryId: props.comment.parentEntryId,
            userProfileId: userStore.user.hdid,
            entryTypeCode: props.comment.entryTypeCode,
            version: 0,
            createdDateTime: DateWrapper.now().toISO(),
        })
        .then((newComment) => {
            if (newComment !== undefined) {
                commentInput.value = "";
                emit("on-comment-added", newComment);
            }
        })
        .catch((err: ResultError) => {
            logger.error(
                `Error adding comment on entry ${
                    props.comment.parentEntryId
                }: ${JSON.stringify(err)}`
            );
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsError("page");
            }
            window.scrollTo({ top: 0, behavior: "smooth" });
        })
        .finally(() => (isSaving.value = false));
}
</script>

<template>
    <v-textarea
        v-model="commentInput"
        data-testid="add-comment-text-area"
        max-rows="10"
        maxlength="1000"
        auto-grow
        rows="1"
        :loading="isSaving"
        placeholder="Write a comment"
        style="overflow-y: auto"
        hide-details
        class="pa-1"
    >
        <template #prepend>
            <v-tooltip
                text="Only you can see comments added to your medical records."
            >
                <template #activator="{ props: slotProps }">
                    <v-icon v-bind="slotProps" icon="lock" size="small" />
                </template>
            </v-tooltip>
        </template>
        <template #append-inner>
            <HgIconButtonComponent
                data-testid="post-comment-btn"
                variant="secondary"
                icon="arrow-up"
                size="x-small"
                :disabled="commentInput === ''"
                :loading="isSaving"
                @click="addComment"
            />
        </template>
    </v-textarea>
</template>
