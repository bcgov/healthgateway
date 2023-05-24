<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faEllipsisV } from "@fortawesome/free-solid-svg-icons";
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import { DateWrapper } from "@/models/dateWrapper";
import User from "@/models/user";
import type { UserComment } from "@/models/userComment";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

library.add(faEllipsisV);

interface Props {
    comment: UserComment;
}
const props = defineProps<Props>();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const store = useStore();

const commentInput = ref("");
const isEditMode = ref(false);
const isLoading = ref(false);

const user = computed<User>(() => store.getters["user/user"]);

function updateComment(
    hdid: string,
    comment: UserComment
): Promise<UserComment> {
    return store.dispatch("comment/updateComment", { hdid, comment });
}

function deleteComment(hdid: string, comment: UserComment): Promise<void> {
    return store.dispatch("comment/deleteComment", { hdid, comment });
}

function formatDate(date: string): string {
    return new DateWrapper(date, { isUtc: true }).format("yyyy-MMM-dd, t");
}

function onSubmit(): void {
    confirmUpdate();
}

function onCancel(): void {
    isEditMode.value = false;
}

function editComment(): void {
    commentInput.value = props.comment.text;
    isEditMode.value = true;
}

function confirmUpdate(): void {
    isLoading.value = true;
    const comment: UserComment = {
        id: props.comment.id,
        text: commentInput.value,
        userProfileId: props.comment.userProfileId,
        parentEntryId: props.comment.parentEntryId,
        createdDateTime: props.comment.createdDateTime,
        entryTypeCode: props.comment.entryTypeCode,
        version: props.comment.version,
    };
    updateComment(user.value.hdid, comment)
        .then(() => {
            logger.info("Comment Updated");
            isEditMode.value = false;
        })
        .catch((err) => {
            logger.error(JSON.stringify(err));
            window.scrollTo({ top: 0, behavior: "smooth" });
        })
        .finally(() => (isLoading.value = false));
}

function removeComment(): void {
    if (confirm("Are you sure you want to delete this comment?")) {
        isLoading.value = true;
        deleteComment(user.value.hdid, props.comment)
            .then(() => logger.info("Comment removed"))
            .catch((err) => {
                logger.error(JSON.stringify(err));
                window.scrollTo({ top: 0, behavior: "smooth" });
            })
            .finally(() => (isLoading.value = false));
    }
}
</script>

<template>
    <b-row>
        <b-col>
            <div v-show="!isLoading">
                <b-row
                    v-if="!isEditMode"
                    class="comment-body py-2 mr-0 ml-3 my-1"
                    align-v="center"
                >
                    <b-col
                        data-testid="commentWrapper"
                        class="comment-text pl-3"
                    >
                        <span data-testid="commentText">{{
                            comment.text
                        }}</span>
                        <p class="m-0 timestamp text-nowrap">
                            {{ formatDate(comment.createdDateTime) }}
                        </p>
                    </b-col>
                    <div class="d-flex flex-row-reverse">
                        <b-dropdown
                            data-testid="commentMenuBtn"
                            dropright
                            text=""
                            :no-caret="true"
                            variant="link"
                        >
                            <template slot="button-content">
                                <hg-icon
                                    icon="ellipsis-v"
                                    size="small"
                                    class="comment-menu"
                                />
                            </template>
                            <b-dropdown-item
                                class="menuItem"
                                data-testid="commentMenuEditBtn"
                                @click="editComment()"
                            >
                                Edit
                            </b-dropdown-item>
                            <b-dropdown-item
                                class="menuItem"
                                data-testid="commentMenuDeleteBtn"
                                @click="removeComment()"
                            >
                                Delete
                            </b-dropdown-item>
                        </b-dropdown>
                    </div>
                </b-row>
                <b-row
                    v-if="isEditMode"
                    class="comment-body py-2 mr-0 ml-3 my-1"
                >
                    <b-col class="pl-2 pr-0">
                        <b-form @submit.prevent>
                            <b-form-textarea
                                id="comment-input"
                                v-model="commentInput"
                                data-testid="editCommentInput"
                                :class="
                                    commentInput.length <= 30
                                        ? 'single-line'
                                        : ''
                                "
                                max-rows="10"
                                no-resize
                                placeholder="Editing a comment"
                                maxlength="1000"
                            ></b-form-textarea>
                        </b-form>
                    </b-col>
                    <b-col
                        class="px-2 mt-1 mt-md-0 mt-lg-0 col-12 col-md-auto col-lg-auto text-right"
                    >
                        <b-button
                            data-testid="saveCommentBtn"
                            class="mr-2"
                            variant="primary"
                            :disabled="commentInput === ''"
                            @click="onSubmit"
                        >
                            Save
                        </b-button>
                        <b-button variant="secondary" @click="onCancel">
                            Cancel
                        </b-button>
                    </b-col>
                </b-row>
            </div>
            <div v-show="isLoading">
                <div class="d-flex align-items-center">
                    <strong>Loading...</strong>
                    <b-spinner class="ml-5"></b-spinner>
                </div>
            </div>
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

.comment-body {
    background-color: $soft_background;
}

.editing {
    background-color: lightyellow;
}

.comment-menu {
    color: $soft_text;
}

.comment-text {
    white-space: pre-line;
}

.timestamp {
    color: $soft_text;
    font-size: 0.7em;
}

.single-line {
    height: 38px !important;
}
</style>
