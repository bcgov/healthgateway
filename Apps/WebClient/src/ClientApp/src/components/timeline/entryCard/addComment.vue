<script lang="ts">
import {
    faArrowCircleUp,
    faLock,
    IconDefinition,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Emit, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import User from "@/models/user";
import type { UserComment } from "@/models/userComment";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
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

    private get lockIcon(): IconDefinition {
        return faLock;
    }

    private get postIcon(): IconDefinition {
        return faArrowCircleUp;
    }

    private created() {
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
    <b-row>
        <b-col cols="auto" class="pl-0 pr-2 align-self-center">
            <div :id="'tooltip-' + comment.parentEntryId" class="tooltip-info">
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
        <b-col>
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
                    <b-button
                        variant="link"
                        data-testid="postCommentBtn"
                        class="btn-circle"
                        :disabled="commentInput === '' || isSaving"
                        @click="onSubmit"
                        ><font-awesome-icon
                            :icon="postIcon"
                            size="lg"
                            fixed-width
                        ></font-awesome-icon
                    ></b-button>
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

.btn-circle {
    text-align: center;
    align-content: center;
    color: $aquaBlue;
    background-color: white;

    border: 1px solid rgb(206, 212, 218);
    border-left: 0px;
}

.btn-circle:disabled {
    color: grey;
    background-color: $soft-background;
    opacity: 1;
}

.btn-circle:hover:enabled {
    color: $primary;
}
</style>
