<script lang="ts">
import Vue from "vue";
import type { UserComment } from "@/models/userComment";
import CommentComponent from "@/components/timeline/comment.vue";
import AddCommentComponent from "@/components/timeline/addComment.vue";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import { Component, Prop } from "vue-property-decorator";
import { ILogger } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { DateWrapper } from "@/models/dateWrapper";
import { Getter } from "vuex-class";
import User from "@/models/user";

@Component({
    components: {
        Comment: CommentComponent,
        AddComment: AddCommentComponent,
    },
})
export default class CommentSectionComponent extends Vue {
    @Prop() parentEntry!: MedicationTimelineEntry;
    @Getter("user", { namespace: "user" }) user!: User;
    @Getter("getEntryComments", { namespace: "comment" })
    entryComments!: (entyId: string) => UserComment[];

    private logger!: ILogger;
    private showComments = false;
    private showInput = false;
    private isLoadingComments = false;

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
    }

    private get comments(): UserComment[] {
        return this.entryComments(this.parentEntry.id) || [];
    }

    private get hasComments(): boolean {
        return this.comments !== undefined ? this.comments.length > 0 : false;
    }

    private onAdd() {
        this.showComments = true;
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
                                variant="link"
                                class="px-3 py-2"
                                @click="showComments = !showComments"
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
