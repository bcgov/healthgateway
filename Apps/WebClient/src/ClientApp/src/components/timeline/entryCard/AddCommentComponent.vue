<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faArrowCircleUp, faLock } from "@fortawesome/free-solid-svg-icons";
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import User from "@/models/user";
import type { UserComment } from "@/models/userComment";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

const store = useStore();

library.add(faArrowCircleUp, faLock);

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

const user = computed<User>(() => store.getters["user/user"]);

const commentInput = ref("");
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const isSaving = ref(false);

const privacyInfoId = computed<string>(() => {
    const id = `privacy-icon-${props.comment.parentEntryId}`;
    return props.isMobileDetails ? id + "-mobile" : id;
});

function createComment(params: {
    hdid: string;
    comment: UserComment;
}): Promise<UserComment | undefined> {
    return store.dispatch("comment/createComment", params);
}

function setTooManyRequestsError(params: { key: string }): void {
    store.dispatch("errorBanner/setTooManyRequestsError", params);
}

function addComment(): void {
    isSaving.value = true;
    createComment({
        hdid: user.value.hdid,
        comment: {
            id: "00000000-0000-0000-0000-000000000000",
            text: commentInput.value,
            parentEntryId: props.comment.parentEntryId,
            userProfileId: user.value.hdid,
            entryTypeCode: props.comment.entryTypeCode,
            version: 0,
            createdDateTime: new DateWrapper().toISO(),
        },
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
                setTooManyRequestsError({ key: "page" });
            }
            window.scrollTo({ top: 0, behavior: "smooth" });
        })
        .finally(() => (isSaving.value = false));
}
</script>

<template>
    <b-row>
        <b-col
            :id="privacyInfoId"
            cols="auto"
            class="px-0 py-1 align-self-center"
        >
            <hg-icon icon="lock" size="small" />
        </b-col>
        <b-tooltip
            variant="secondary"
            :target="privacyInfoId"
            placement="left"
            triggers="hover"
        >
            Only you can see comments added to your medical records.
        </b-tooltip>
        <b-col class="ml-2">
            <b-input-group>
                <b-form-textarea
                    :id="'comment-input-' + comment.parentEntryId"
                    v-model="commentInput"
                    data-testid="addCommentTextArea"
                    class="comment-input"
                    :class="[
                        { 'single-line': commentInput.length <= 30 },
                        { faded: commentInput.length === 0 },
                    ]"
                    rows="2"
                    max-rows="10"
                    :no-resize="true"
                    placeholder="Write a comment"
                    maxlength="1000"
                    style="overflow: auto"
                    :disabled="isSaving"
                ></b-form-textarea>
                <b-input-group-append>
                    <hg-button
                        variant="icon-input"
                        data-testid="postCommentBtn"
                        :disabled="commentInput === '' || isSaving"
                        @click="addComment"
                    >
                        <hg-icon
                            icon="arrow-circle-up"
                            size="medium"
                            fixed-width
                        />
                    </hg-button>
                </b-input-group-append>
            </b-input-group>
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

.comment-input {
    border-right: 0px;

    &.faded {
        background-color: $soft-background;
    }

    &.single-line {
        height: 38px !important;
    }
}
</style>
