<script lang="ts">
import { faLock, IconDefinition } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Emit, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import User from "@/models/user";
import type { UserComment } from "@/models/userComment";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

@Component
export default class AddCommentComponent extends Vue {
    @Prop() comment!: UserComment;

    @Getter("user", { namespace: "user" }) user!: User;

    @Action("createComment", { namespace: "comment" })
    createComment!: (params: {
        hdid: string;
        comment: UserComment;
    }) => Promise<UserComment | undefined>;

    private commentInput = "";

    private logger!: ILogger;

    private isSaving = false;

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private get lockIcon(): IconDefinition {
        return faLock;
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
            .catch((err) => {
                this.logger.error(
                    `Error adding comment on entry ${
                        this.comment.parentEntryId
                    }: ${JSON.stringify(err)}`
                );
            })
            .finally(() => {
                this.isSaving = false;
            });
    }

    @Emit()
    private onCommentAdded(comment: UserComment) {
        return comment;
    }
}
</script>

<template>
    <b-col>
        <div>
            <b-row class="p-2">
                <b-col cols="auto" class="px-0 align-self-center">
                    <div
                        :id="'tooltip-' + comment.parentEntryId"
                        class="tooltip-info"
                    >
                        <font-awesome-icon :icon="lockIcon" size="1x">
                        </font-awesome-icon>
                    </div>
                    <b-tooltip
                        variant="secondary"
                        :target="'tooltip-' + comment.parentEntryId"
                        placement="left"
                        triggers="hover"
                    >
                        Only you can see comments added to your medical records.
                    </b-tooltip>
                </b-col>
                <b-col class="col pl-2 pr-0">
                    <b-form @submit.prevent>
                        <b-form-textarea
                            :id="'comment-input-' + comment.parentEntryId"
                            v-model="commentInput"
                            data-testid="addCommentTextArea"
                            class="comment-input-style"
                            :class="
                                commentInput.length <= 30 ? 'single-line' : ''
                            "
                            rows="2"
                            max-rows="10"
                            no-resize
                            placeholder="Write a comment"
                            maxlength="1000"
                            :disabled="isSaving"
                        ></b-form-textarea>
                    </b-form>
                </b-col>
                <b-col
                    class="pl-2 pr-0 mt-1 mt-md-0 mt-lg-0 col-12 col-md-auto col-lg-auto text-right"
                >
                    <b-button
                        data-testid="postCommentBtn"
                        class="mr-2 px-4"
                        variant="primary"
                        :disabled="commentInput === '' || isSaving"
                        @click="onSubmit"
                    >
                        Post
                    </b-button>
                </b-col>
            </b-row>
        </div>
    </b-col>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.comment-input-style:not(:focus) {
    background-color: $soft_background;
}

.single-line {
    height: 38px !important;
}
</style>
