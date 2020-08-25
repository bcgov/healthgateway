<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#comment-input:not(:focus) {
    background-color: $soft_background;
}

.single-line {
    height: 38px !important;
}
</style>
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
                            id="comment-input"
                            v-model="commentInput"
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
<script lang="ts">
import Vue from "vue";
import UserComment from "@/models/userComment";
import User from "@/models/user";
import { Getter } from "vuex-class";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import {
    IconDefinition,
    faEllipsisV,
    faLock,
} from "@fortawesome/free-solid-svg-icons";
import { ILogger, IUserCommentService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";

@Component
export default class AddCommentComponent extends Vue {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    @Getter("user", { namespace: "user" }) user!: User;
    @Prop() comment!: UserComment;
    private commentInput: string = "";
    private commentService!: IUserCommentService;
    private hasErrors: boolean = false;
    private isSaving: boolean = false;

    private mounted() {
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
                createdDateTime: new Date(),
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
                this.hasErrors = true;
            })
            .finally(() => {
                this.isSaving = false;
            });
    }

    @Emit()
    onCommentAdded(comment: UserComment) {
        return comment;
    }
}
</script>
