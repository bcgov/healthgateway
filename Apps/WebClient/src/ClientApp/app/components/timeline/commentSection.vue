<style scoped lang="scss">
@import "@/assets/scss/_variables.scss";
</style>
<template>
  <div>
    <b-row class="pt-2">
      <b-col>
        <b-btn
          class="commentButton"
          variant="outline-primary"
          @click="toggleCommentInput()"
        >
          <font-awesome-icon
            :icon="commentIcon"
            size="1x"
            class="pr-1"
          ></font-awesome-icon>
          <span>Comment</span>
        </b-btn>
      </b-col>
      <b-col>
        <div class="d-flex flex-row-reverse">
          <b-btn variant="link" class="px-0 py-2" @click="toggleComments()">
            <span v-if="hasComments">{{
              comments.length > 1 ? comments.length + " comments" : "1 comment"
            }}</span>
          </b-btn>
        </div>
      </b-col>
    </b-row>
    <b-row class="py-2" v-if="commentInputVisible">
      <b-col>
        <b-collapse :visible="commentInputVisible">
          <b-form @submit.prevent="onSubmit" @reset="onClose">
            <b-form-input
              type="text"
              ref="commentInput"
              autofocus
              class="commentInput"
              v-model="commentInput"
              placeholder="Enter a comment"
              maxlength="1000"
            ></b-form-input>
          </b-form>
        </b-collapse>
      </b-col>
    </b-row>
    <b-row>
      <b-col>
        <b-collapse :visible="commentsVisible">
          <div v-for="comment in this.comments" :key="comment.id">
            <Comment
              :comment="comment"
              :service="commentService"
              @on-comment-updated="onChange"
              @on-comment-deleted="onDelete"
              @on-edit-started="onEdit"
            ></Comment>
          </div>
        </b-collapse>
      </b-col>
    </b-row>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import UserComment from "@/models/userComment";
import CommentComponent from "@/components/timeline/comment.vue";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import User from "@/models/user";
import { Prop, Component } from "vue-property-decorator";
import { IUserCommentService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import {
  faCommentAlt,
  IconDefinition,
} from "@fortawesome/free-solid-svg-icons";
import { Getter } from "vuex-class";

@Component({
  components: {
    Comment: CommentComponent,
  },
})
export default class CommentSectionComponent extends Vue {
  @Getter("user", { namespace: "user" }) user!: User;
  @Prop() parentEntry!: MedicationTimelineEntry;
  private commentService!: IUserCommentService;
  private commentsVisible: boolean = false;
  private commentInputVisible: boolean = false;
  private isCommentSaving: boolean = false;
  private isLoadingComments: boolean = false;
  private isEditMode: boolean = false;
  private commentInput: string = "";
  private comments: UserComment[] = [];
  // TODO: improve this
  private editing: UserComment = undefined;
  private numComments = 0;
  private hasErrors: boolean = false;

  private mounted() {
    this.commentService = container.get<IUserCommentService>(
      SERVICE_IDENTIFIER.UserCommentService
    );
    this.getComments();
  }

  private get hasComments(): boolean {
    return this.comments.length > 0;
  }

  private get commentIcon(): IconDefinition {
    return faCommentAlt;
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
    this.commentsVisible = !this.commentsVisible;
    if (this.commentInputVisible && !this.commentsVisible) {
      this.commentInputVisible = false;
    }
  }

  private toggleCommentInput(): void {
    this.commentInputVisible = !this.commentInputVisible;
    if (this.commentInputVisible && !this.commentsVisible) {
      this.commentsVisible = true;
    }
  }

  private onSubmit(): void {
    if (this.isEditMode) {
      this.updateComment(this.editing);
    } else {
      this.addComment();
    }
  }

  private addComment(): void {
    this.isCommentSaving = true;
    let commentPromise = this.commentService
      .createComment({
        text: this.commentInput,
        parentEntryId: this.parentEntry.id,
        userProfileId: this.user.hdid,
      })
      .then(() => {
        this.commentInput = "";
        this.getComments();
        this.commentsVisible = true;
      })
      .catch((err) => {
        console.log("Error adding comment on entry " + this.parentEntry.id);
        console.log(err);
        this.hasErrors = true;
      })
      .finally(() => {
        this.isCommentSaving = false;
      });
  }

  private getComments() {
    const referenceId = this.parentEntry.id;
    this.isLoadingComments = true;
    let commentPromise = this.commentService
      .getCommentsForEntry(referenceId)
      .then((result) => {
        if (result) {
          this.comments = result.resourcePayload;
          this.sortComments();
          this.isLoadingComments = false;
        }
      })
      .catch((err) => {
        console.log("Error loading comments for entry " + this.parentEntry.id);
        console.log(err);
        this.hasErrors = true;
        this.isLoadingComments = false;
      });
  }

  private updateComment(comment: UserComment): void {
    this.isCommentSaving = true;
    let commentPromise = this.commentService
      .updateComment({
        id: comment.id,
        text: this.commentInput,
        userProfileId: comment.userProfileId,
        parentEntryId: comment.parentEntryId,
        createdDateTime: comment.createdDateTime,
        version: comment.version,
      })
      .then((result) => {
        this.getComments();
        this.commentInput = "";
      })
      .catch((err) => {
        console.log(err);
        this.hasErrors = true;
      })
      .finally(() => {
        this.isCommentSaving = false;
        this.isEditMode = false;
      });
  }

  private deleteComment(comment: UserComment): void {
    let commentPromise = this.commentService
      .deleteComment(comment)
      .then((result) => {
        console.log(result);
        this.getComments();
      })
      .catch((err) => {
        console.log(err);
      });
  }

  private onChange(comment: UserComment) {
    console.log("test1 ", comment);
  }

  private onDelete(comment: UserComment) {
    this.deleteComment(comment);
  }

  private onEdit(comment: UserComment) {
    this.editing = comment;
    this.commentInputVisible = true;
    this.isEditMode = true;
    this.commentInput = comment.text;
  }

  // TODO: Change this
  onClose() {
    delete this.editing;
  }
}
</script>
