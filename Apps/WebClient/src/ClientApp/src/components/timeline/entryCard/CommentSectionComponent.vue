<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import AddCommentComponent from "@/components/timeline/entryCard/AddCommentComponent.vue";
import CommentComponent from "@/components/timeline/entryCard/CommentComponent.vue";
import { CommentEntryType } from "@/constants/commentEntryType";
import { entryTypeMap } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry from "@/models/timelineEntry";
import User from "@/models/user";
import { UserComment } from "@/models/userComment";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

@Component({
    components: {
        Comment: CommentComponent,
        AddComment: AddCommentComponent,
    },
})
export default class CommentSectionComponent extends Vue {
    @Prop() parentEntry!: TimelineEntry;
    @Prop({ default: false }) isMobileDetails!: boolean;

    @Getter("user", { namespace: "user" }) user!: User;
    @Action("updateComment", { namespace: "comment" })
    updateComment!: (params: {
        hdid: string;
        comment: UserComment;
    }) => Promise<UserComment>;

    private logger!: ILogger;
    private showComments = false;
    private showInput = false;
    private isLoadingComments = false;

    private newComment: UserComment = {
        id: "",
        text: "",
        parentEntryId: this.parentEntry.id,
        entryTypeCode:
            entryTypeMap.get(this.parentEntry.type)?.commentType ??
            CommentEntryType.None,
        userProfileId: "",
        createdDateTime: new DateWrapper().toISODate(),
        version: 0,
    };

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private mounted(): void {
        // Some comments dont have entry type. This code updates them if they dont.
        let commentsToUpdate: UserComment[] = [];
        if (this.parentEntry.comments !== null) {
            this.parentEntry.comments.forEach((x) => {
                if (x.entryTypeCode === CommentEntryType.None) {
                    x.entryTypeCode =
                        entryTypeMap.get(this.parentEntry.type)?.commentType ??
                        CommentEntryType.None;
                    x.updatedBy = "System_Backfill";
                    commentsToUpdate.push(x);
                }
            });
        }

        commentsToUpdate.forEach((x) => {
            this.logger.info("Updating comment " + x.id);
            this.updateComment({ hdid: this.user.hdid, comment: x });
        });

        let commentsSection = (
            this.$refs["entryComments" + this.parentEntry.id] as Vue
        ).$el;
        commentsSection?.addEventListener(
            "transitionend",
            this.onSectionExpand
        );
    }

    private get hasComments(): boolean {
        return this.parentEntry.comments !== null
            ? this.parentEntry.comments.length > 0
            : false;
    }

    private onAdd(): void {
        this.showComments = true;
    }

    private onSectionExpand(event: Event): void {
        if (this.isMobileDetails && this.showComments) {
            let commentsSection = (
                this.$refs["entryComments" + this.parentEntry.id] as Vue
            ).$el;
            let transitionEvent = event as TransitionEvent;
            if (
                commentsSection !== transitionEvent.target ||
                transitionEvent.propertyName !== "height"
            ) {
                return;
            }
            commentsSection.scrollIntoView({ behavior: "smooth" });
        }
    }
}
</script>

<template>
    <b-row class="pt-2">
        <b-col>
            <b-row class="pt-2">
                <b-col>
                    <div v-if="hasComments" class="d-flex flex-row-reverse">
                        <b-btn
                            variant="link"
                            class="py-2"
                            data-testid="showCommentsBtn"
                            @click="showComments = !showComments"
                        >
                            <span>
                                {{
                                    parentEntry.comments.length > 1
                                        ? parentEntry.comments.length +
                                          " Comments"
                                        : "1 Comment"
                                }}</span
                            >
                        </b-btn>
                    </div>
                </b-col>
            </b-row>
            <b-row class="py-2">
                <b-col>
                    <b-collapse
                        :id="'entryComments-' + parentEntry.id"
                        :ref="'entryComments' + parentEntry.id"
                        v-model="showComments"
                    >
                        <div v-if="!isLoadingComments">
                            <div
                                v-for="comment in parentEntry.comments"
                                :key="comment.id"
                            >
                                <Comment :comment="comment"></Comment>
                            </div>
                        </div>
                        <div v-else>
                            <div class="d-flex align-items-center">
                                <strong>Loading...</strong>
                                <b-spinner class="ml-5"></b-spinner>
                            </div>
                        </div>
                    </b-collapse>
                </b-col>
            </b-row>
            <div
                :class="{
                    push: isMobileDetails,
                }"
            ></div>
            <AddComment
                class="pb-2"
                :class="{
                    'fixed-bottom p-3 comment-input': isMobileDetails,
                }"
                :comment="newComment"
                @on-comment-added="onAdd"
            ></AddComment>
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

.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
    display: none;
}

.comment-input {
    border-top: 1px $primary solid;
    background-color: white;
}

.push {
    height: 60px;
}
</style>
