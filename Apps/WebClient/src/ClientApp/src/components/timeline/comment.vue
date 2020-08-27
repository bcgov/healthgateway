<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

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
<template>
    <b-col>
        <div v-show="!isLoading">
            <b-row
                v-if="!isEditMode"
                class="comment-body py-2 mr-0 ml-3 my-1"
                align-v="center"
            >
                <b-col class="comment-text">
                    {{ comment.text }}
                    <p class="m-0 timestamp">
                        {{ formatDate(comment.createdDateTime) }}
                    </p>
                </b-col>
                <div class="d-flex flex-row-reverse">
                    <b-dropdown
                        dropright
                        text=""
                        :no-caret="true"
                        variant="link"
                    >
                        <template slot="button-content">
                            <font-awesome-icon
                                class="comment-menu"
                                :icon="menuIcon"
                                size="1x"
                            ></font-awesome-icon>
                        </template>
                        <b-dropdown-item
                            class="menuItem"
                            @click="editComment()"
                        >
                            Edit
                        </b-dropdown-item>
                        <b-dropdown-item
                            class="menuItem"
                            @click="deleteComment()"
                        >
                            Delete
                        </b-dropdown-item>
                    </b-dropdown>
                </div>
            </b-row>
            <b-row v-if="isEditMode" class="comment-body py-2 mr-0 ml-3 my-1">
                <b-col class="col pl-2 pr-0">
                    <b-form @submit.prevent>
                        <b-form-textarea
                            id="comment-input"
                            v-model="commentInput"
                            :class="
                                commentInput.length <= 30 ? 'single-line' : ''
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
export default class CommentComponent extends Vue {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    @Getter("user", { namespace: "user" }) user!: User;
    @Prop() comment!: UserComment;
    private commentInput: string = "";
    private commentService!: IUserCommentService;
    private hasErrors: boolean = false;
    private isEditMode: boolean = false;
    private isLoading: boolean = false;

    private mounted() {
        this.commentService = container.get<IUserCommentService>(
            SERVICE_IDENTIFIER.UserCommentService
        );
    }

    private formatDate(date: Date): string {
        return new Date(Date.parse(date + "Z")).toLocaleString([], {
            year: "numeric",
            month: "long",
            day: "numeric",
            hour: "2-digit",
            minute: "2-digit",
        });
    }

    private get menuIcon(): IconDefinition {
        return faEllipsisV;
    }

    private onSubmit(): void {
        this.updateComment();
    }

    private onCancel(): void {
        this.isEditMode = false;
    }

    private editComment(): void {
        this.commentInput = this.comment.text;
        this.isEditMode = true;
    }

    private updateComment(): void {
        this.isLoading = true;
        this.commentService
            .updateComment({
                id: this.comment.id,
                text: this.commentInput,
                userProfileId: this.comment.userProfileId,
                parentEntryId: this.comment.parentEntryId,
                createdDateTime: this.comment.createdDateTime,
                version: this.comment.version,
            })
            .then((result) => {
                this.needsUpdate(this.comment);
            })
            .catch((err) => {
                this.logger.error(err);
                this.hasErrors = true;
            })
            .finally(() => {
                this.isEditMode = false;
                this.isLoading = false;
            });
    }

    private deleteComment(): void {
        if (confirm("Are you sure you want to delete this comment?")) {
            this.isLoading = true;
            this.commentService
                .deleteComment(this.comment)
                .then((result) => {
                    this.needsUpdate(this.comment);
                })
                .catch((err) => {
                    this.logger.error(err);
                })
                .finally(() => {
                    this.isLoading = false;
                });
        }
    }

    @Emit()
    needsUpdate(comment: UserComment) {
        return comment;
    }
}
</script>
