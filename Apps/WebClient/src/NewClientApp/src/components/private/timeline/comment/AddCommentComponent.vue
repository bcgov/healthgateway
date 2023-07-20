<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faArrowCircleUp, faLock } from "@fortawesome/free-solid-svg-icons";
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

const userStore = useUserStore();
const errorStore = useErrorStore();
const commentStore = useCommentStore();

library.add(faArrowCircleUp, faLock);

interface Props {
    comment: UserComment;
    visible?: boolean;
    isMobileDetails?: boolean;
}
const addCommentProps = withDefaults(defineProps<Props>(), {
    visible: false,
    isMobileDetails: false,
});

const emit = defineEmits<{
    (e: "on-comment-added", newValue: UserComment): UserComment;
}>();

const commentInput = ref("");
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const isSaving = ref(false);

function addComment(): void {
    isSaving.value = true;
    commentStore
        .createComment(userStore.user.hdid, {
            id: "00000000-0000-0000-0000-000000000000",
            text: commentInput.value,
            parentEntryId: addCommentProps.comment.parentEntryId,
            userProfileId: userStore.user.hdid,
            entryTypeCode: addCommentProps.comment.entryTypeCode,
            version: 0,
            createdDateTime: new DateWrapper().toISO(),
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
                    addCommentProps.comment.parentEntryId
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
        data-testid="addCommentTextArea"
        max-rows="10"
        maxlength="1000"
        auto-grow
        rows="1"
        variant="outlined"
        :loading="isSaving"
        placeholder="Write a comment"
        style="overflow-y: auto"
        class="pa-2"
    >
        <template #prepend>
            <v-tooltip
                variant="secondary"
                location="left"
                text="Only you can see comments added to your medical records."
            >
                <template #activator="{ props }">
                    <v-icon v-bind="props" icon="lock" size="small" />
                </template>
            </v-tooltip>
        </template>
        <template #append>
            <HgIconButtonComponent
                color="primary"
                variant="icon-input"
                data-testid="postCommentBtn"
                icon="arrow-circle-up"
                :disabled="commentInput === '' || isSaving"
                @click="addComment"
            />
        </template>
    </v-textarea>
</template>
