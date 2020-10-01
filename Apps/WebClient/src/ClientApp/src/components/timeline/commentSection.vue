<script lang="ts">
import Vue from "vue";
import UserComment from "@/models/userComment";
import CommentComponent from "@/components/timeline/comment.vue";
import AddCommentComponent from "@/components/timeline/addComment.vue";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import { Component, Prop } from "vue-property-decorator";
import { ILogger, IUserCommentService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { DateWrapper } from "@/models/dateWrapper";

@Component({
    components: {
        Comment: CommentComponent,
        AddComment: AddCommentComponent,
    },
})
export default class CommentSectionComponent extends Vue {
    @Prop() parentEntry!: MedicationTimelineEntry;

    private logger!: ILogger;
    private commentService!: IUserCommentService;
    private showComments: boolean = false;
    private showInput: boolean = false;
    private isLoadingComments: boolean = false;
    private comments: UserComment[] = [];
    private newComment: UserComment = {
        id: "",
        text: "",
        parentEntryId: this.parentEntry.id,
        userProfileId: "",
        createdDateTime: new DateWrapper().toISODate(),
        version: 0,
    };

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.commentService = container.get<IUserCommentService>(
            SERVICE_IDENTIFIER.UserCommentService
        );
        this.getComments();
    }

    private get hasComments(): boolean {
        return this.comments.length > 0;
    }

    private sortComments() {
        this.comments.sort((a, b) => {
            if (a.createdDateTime > b.createdDateTime) {
                return -1;
            } else if (a.createdDateTime < b.createdDateTime) {
                return 1;
            } else {
                return 0;
            }
        });
    }

    private toggleComments(): void {
        this.showComments = !this.showComments;
    }

    private getComments() {
        this.isLoadingComments = true;
        const parentEntryId = this.parentEntry.id;
        let commentPromise = this.commentService
            .getCommentsForEntry(parentEntryId)
            .then((result) => {
                if (result) {
                    this.comments = result.resourcePayload;
                    this.sortComments();
                }
            })
            .catch((err) => {
                this.logger.error(
                    `Error loading comments for entry  ${
                        this.parentEntry.id
                    }: ${JSON.stringify(err)}`
                );
            })
            .finally(() => {
                this.isLoadingComments = false;
            });
    }

    private needsUpdate(comment: UserComment) {
        this.getComments();
    }

    private onAdd(comment: UserComment) {
        if (!this.showComments) {
            this.showComments = true;
        }
        this.getComments();
    }
}
</script>

<template>
    <b-row>
        <b-col class="commentSection">
            <div>
                <b-row class="pt-2">
                    <b-col>
                        <div v-if="hasComments" class="d-flex flex-row-reverse">
                            <b-btn
                                v-b-toggle="'entryComments-' + parentEntry.id"
                                variant="link"
                                class="px-3 py-2"
                                @click="toggleComments()"
                            >
                                <span>
                                    {{
                                        comments.length > 1
                                            ? comments.length + " Comments"
                                            : "1 Comment"
                                    }}</span
                                >
                            </b-btn>
                        </div>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <AddComment
                            :comment="newComment"
                            @on-comment-added="onAdd"
                        ></AddComment>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <b-collapse
                            :id="'entryComments-' + parentEntry.id"
                            v-model="showComments"
                        >
                            <div v-if="!isLoadingComments">
                                <div
                                    v-for="comment in comments"
                                    :key="comment.id"
                                >
                                    <Comment
                                        :comment="comment"
                                        @needs-update="needsUpdate"
                                    ></Comment>
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
            </div>
        </b-col>
    </b-row>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
    display: none;
}

.commentSection {
    padding-left: 0px;
    padding-right: 0px;
}
</style>
