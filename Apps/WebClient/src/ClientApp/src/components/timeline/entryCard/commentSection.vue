<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import AddCommentComponent from "@/components/timeline/entryCard/addComment.vue";
import CommentComponent from "@/components/timeline/entryCard/comment.vue";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry from "@/models/timelineEntry";
import User from "@/models/user";
import {
    CommentEntryType,
    EntryTypeMapper,
    UserComment,
} from "@/models/userComment";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

@Component({
    components: {
        Comment: CommentComponent,
        AddComment: AddCommentComponent,
    },
})
export default class CommentSectionComponent extends Vue {
    @Prop() parentEntry!: TimelineEntry;
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
        entryTypeCode: EntryTypeMapper.toCommentEntryType(
            this.parentEntry.type
        ),
        userProfileId: "",
        createdDateTime: new DateWrapper().toISODate(),
        version: 0,
    };

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private mounted() {
        // Some comments dont have entry type. This code updates them if they dont.
        let commentsToUpdate: UserComment[] = [];
        if (this.parentEntry.comments !== null) {
            this.parentEntry.comments.forEach((x) => {
                if (x.entryTypeCode === CommentEntryType.None) {
                    x.entryTypeCode = EntryTypeMapper.toCommentEntryType(
                        this.parentEntry.type
                    );
                    x.updatedBy = "System_Backfill";
                    commentsToUpdate.push(x);
                }
            });
        }

        commentsToUpdate.forEach((x) => {
            this.logger.info("Updating comment " + x.id);
            this.updateComment({ hdid: this.user.hdid, comment: x });
        });
    }

    private get hasComments(): boolean {
        return this.parentEntry.comments !== null
            ? this.parentEntry.comments.length > 0
            : false;
    }

    private onAdd() {
        this.showComments = true;
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
            <AddComment
                :comment="newComment"
                @on-comment-added="onAdd"
            ></AddComment>
            <b-row class="pt-2">
                <b-col>
                    <b-collapse
                        :id="'entryComments-' + parentEntry.id"
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
</style>
