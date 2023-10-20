<script setup lang="ts">
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import type { UserComment } from "@/models/userComment";
import { ILogger } from "@/services/interfaces";
import { useAppStore } from "@/stores/app";
import { useCommentStore } from "@/stores/comment";
import { useUserStore } from "@/stores/user";

interface Props {
    comment: UserComment;
}
const props = defineProps<Props>();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const userStore = useUserStore();
const commentStore = useCommentStore();
const appStore = useAppStore();

const commentInput = ref("");
const isEditMode = ref(false);
const isUpdating = ref(false);

const isMobile = computed(() => appStore.isMobile);

function formatDate(date: string): string {
    return DateWrapper.fromIso(date).format("yyyy-MMM-dd, t");
}

function onCancel(): void {
    isEditMode.value = false;
}

function editComment(): void {
    commentInput.value = props.comment.text;
    isEditMode.value = true;
}

function handleCommentError(err: Error): void {
    logger.error(JSON.stringify(err));
    window.scrollTo({ top: 0, behavior: "smooth" });
}

function onSubmit(): void {
    isUpdating.value = true;
    const comment: UserComment = {
        id: props.comment.id,
        text: commentInput.value,
        userProfileId: props.comment.userProfileId,
        parentEntryId: props.comment.parentEntryId,
        createdDateTime: props.comment.createdDateTime,
        entryTypeCode: props.comment.entryTypeCode,
        version: props.comment.version,
    };
    commentStore
        .updateComment(userStore.user.hdid, comment)
        .then(() => {
            logger.info("Comment Updated");
            isEditMode.value = false;
        })
        .catch(handleCommentError)
        .finally(() => (isUpdating.value = false));
}

function removeComment(): void {
    if (confirm("Are you sure you want to delete this comment?")) {
        isUpdating.value = true;
        commentStore
            .deleteComment(userStore.user.hdid, props.comment)
            .then(() => logger.info("Comment removed"))
            .catch(handleCommentError)
            .finally(() => (isUpdating.value = false));
    }
}
</script>

<template>
    <v-sheet class="px-4 py-2 mt-2" color="grey-lighten-5">
        <v-row v-if="!isEditMode">
            <v-col>
                <p class="text-body-1">
                    <span data-testid="commentText">{{ comment.text }}</span>
                    <br />
                    <span class="text-caption text-medium-emphasis">
                        {{ formatDate(comment.createdDateTime) }}
                    </span>
                </p>
            </v-col>
            <v-col cols="auto">
                <v-menu>
                    <template #activator="{ props: slotProps }">
                        <HgIconButtonComponent
                            icon="ellipsis-v"
                            size="small"
                            v-bind="slotProps"
                            data-testid="commentMenuBtn"
                        />
                    </template>
                    <v-list>
                        <v-list-item
                            data-testid="commentMenuEditBtn"
                            title="Edit"
                            @click="editComment()"
                        />
                        <v-list-item
                            data-testid="commentMenuDeleteBtn"
                            title="Delete"
                            :loading="isUpdating"
                            @click="removeComment()"
                        />
                    </v-list>
                </v-menu>
            </v-col>
        </v-row>
        <v-row v-else :class="{ 'flex-column': isMobile }">
            <v-col>
                <v-textarea
                    id="comment-input"
                    v-model="commentInput"
                    data-testid="editCommentInput"
                    max-rows="10"
                    rows="1"
                    variant="underlined"
                    auto-grow
                    placeholder="Editing a comment"
                    maxlength="1000"
                    :loading="isUpdating"
                    hide-details
                />
            </v-col>
            <v-col cols="auto" class="d-flex align-center">
                <HgButtonComponent
                    data-testid="saveCommentBtn"
                    class="mr-2"
                    variant="primary"
                    :disabled="commentInput === ''"
                    :loading="isUpdating"
                    text="Save"
                    @click="onSubmit"
                />
                <HgButtonComponent
                    variant="secondary"
                    text="Cancel"
                    @click="onCancel"
                />
            </v-col>
        </v-row>
    </v-sheet>
</template>
