<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faArrowCircleUp, faLock } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import UserPreferenceType from "@/constants/userPreferenceType";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import User from "@/models/user";
import type { UserComment } from "@/models/userComment";
import { UserPreference } from "@/models/userPreference";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

library.add(faArrowCircleUp, faLock);

@Component
export default class AddCommentComponent extends Vue {
    @Prop()
    comment!: UserComment;

    @Prop({ default: false })
    visible!: boolean;

    @Prop({ default: false })
    isMobileDetails!: boolean;

    @Action("createComment", { namespace: "comment" })
    createComment!: (params: {
        hdid: string;
        comment: UserComment;
    }) => Promise<UserComment | undefined>;

    @Action("setTooManyRequestsError", { namespace: "errorBanner" })
    setTooManyRequestsError!: (params: { key: string }) => void;

    @Action("setSeenTutorialComment", { namespace: "user" })
    setSeenTutorialComment!: (params: { value: boolean }) => void;

    @Action("setUserPreference", { namespace: "user" })
    setUserPreference!: (params: {
        preference: UserPreference;
    }) => Promise<void>;

    @Getter("seenTutorialComment", { namespace: "user" })
    seenTutorialComment!: boolean;

    @Getter("user", { namespace: "user" })
    user!: User;

    private commentInput = "";
    private logger!: ILogger;
    private isSaving = false;
    private isCommentTutorialHidden = true;

    private get showCommentTutorial(): boolean {
        const preferenceType = UserPreferenceType.TutorialComment;
        return (
            this.visible &&
            this.user.preferences[preferenceType]?.value === "true" &&
            !this.isCommentTutorialHidden
        );
    }

    private get privacyInfoId(): string {
        const id = `privacy-icon-${this.comment.parentEntryId}`;
        return this.isMobileDetails ? id + "-mobile" : id;
    }

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

    @Watch("visible")
    private async onVisibleChanged(): Promise<void> {
        if (
            this.visible &&
            (!this.seenTutorialComment || this.isMobileDetails)
        ) {
            // wait 2 ticks for animations to complete
            await this.$nextTick();
            await this.$nextTick();

            // enable popover
            this.isCommentTutorialHidden = false;
            this.setSeenTutorialComment({ value: true });
        }
    }

    @Emit()
    private onCommentAdded(comment: UserComment): UserComment {
        return comment;
    }

    private dismissCommentTutorial(): void {
        this.logger.debug("Dismissing comment tutorial");
        this.isCommentTutorialHidden = true;

        const preference = {
            ...this.user.preferences[UserPreferenceType.TutorialComment],
            value: "false",
        };
        this.setUserPreference({ preference });
    }
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
        <b-popover
            triggers="manual"
            :show="showCommentTutorial"
            :target="privacyInfoId"
            placement="topright"
            boundary="viewport"
        >
            <div>
                <hg-button
                    class="float-right text-dark p-0 ml-2"
                    variant="icon"
                    @click="dismissCommentTutorial()"
                    >×</hg-button
                >
            </div>
            <div data-testid="comment-tutorial-popover">
                You can add comments to help you keep track of important health
                details. Only you can see them.
            </div>
        </b-popover>
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
