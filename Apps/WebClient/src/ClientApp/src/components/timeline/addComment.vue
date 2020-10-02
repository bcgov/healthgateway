<script lang="ts">
import Vue from "vue";
import UserComment from "@/models/userComment";
import User from "@/models/user";
import { Getter } from "vuex-class";
import { Component, Emit, Prop } from "vue-property-decorator";
import { IconDefinition, faLock } from "@fortawesome/free-solid-svg-icons";
import { ILogger, IUserCommentService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { DateWrapper } from "@/models/dateWrapper";

@Component
export default class AddCommentComponent extends Vue {
    @Getter("user", { namespace: "user" }) user!: User;
    @Prop() comment!: UserComment;
    private commentInput = "";

    private logger!: ILogger;
    private commentService!: IUserCommentService;

    private isSaving = false;

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.commentService = container.get<IUserCommentService>(
            SERVICE_IDENTIFIER.UserCommentService
        );
    }

    private get lockIcon(): IconDefinition {
        return faLock;
    }

    private onSubmit(): void {
        this.addComment();
    }

    private addComment(): void {
        this.isSaving = true;
        this.commentService
            .createComment({
                text: this.commentInput,
                parentEntryId: this.comment.parentEntryId,
                userProfileId: this.user.hdid,
                version: 0,
                createdDateTime: new DateWrapper().toISO(),
            })
            .then(() => {
                this.commentInput = "";
                this.onCommentAdded(this.comment);
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
