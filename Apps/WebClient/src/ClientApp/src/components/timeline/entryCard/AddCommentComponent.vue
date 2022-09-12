<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faArrowCircleUp, faLock } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Emit, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import User from "@/models/user";
import type { UserComment } from "@/models/userComment";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

library.add(faArrowCircleUp, faLock);

@Component
export default class AddCommentComponent extends Vue {
    @Prop()
    comment!: UserComment;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Action("createComment", { namespace: "comment" })
    createComment!: (params: {
        hdid: string;
        comment: UserComment;
    }) => Promise<UserComment | undefined>;

    @Action("setTooManyRequestsError", { namespace: "errorBanner" })
    setTooManyRequestsError!: (params: { key: string }) => void;

    private commentInput = "";
    private logger!: ILogger;
    private isSaving = false;

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private onSubmit(): void {
        this.addComment();
    }

    private addComment(): void {
        this.isSaving = true;
        this.createComment({
            hdid: this.user.hdid,
            comment: {
                id: "00000000-0000-0000-0000-000000000000",
                text: this.commentInput,
                parentEntryId: this.comment.parentEntryId,
                userProfileId: this.user.hdid,
                entryTypeCode: this.comment.entryTypeCode,
                version: 0,
                createdDateTime: new DateWrapper().toISO(),
            },
        })
            .then((newComment) => {
                if (newComment !== undefined) {
                    this.commentInput = "";
                    this.onCommentAdded(newComment);
                }
            })
            .catch((err: ResultError) => {
                this.logger.error(
                    `Error adding comment on entry ${
                        this.comment.parentEntryId
                    }: ${JSON.stringify(err)}`
                );
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                    window.scrollTo(0, 0);
                }
            })
            .finally(() => {
                this.isSaving = false;
            });
    }

    @Emit()
    private onCommentAdded(comment: UserComment): UserComment {
        return comment;
    }
}
</script>

<template>
    <b-row>
        <b-col
            :id="'tooltip-' + comment.parentEntryId"
            cols="auto"
            class="px-0 py-1 align-self-center"
        >
            <hg-icon icon="lock" size="small" />
        </b-col>
        <b-tooltip
            variant="secondary"
            :target="'tooltip-' + comment.parentEntryId"
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
                        @click="onSubmit"
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
