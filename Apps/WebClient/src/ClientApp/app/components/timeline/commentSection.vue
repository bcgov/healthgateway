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
    <b-row v-show="showInput" class="py-2">
      <b-col>
        <b-form @submit.prevent="onSubmit">
          <b-form-input
            ref="commentInput"
            v-model="commentInput"
            type="text"
            autofocus
            class="form-control commentInput"
            placeholder="Enter a comment"
            maxlength="1000"
          ></b-form-input>
        </b-form>
      </b-col>
    </b-row>
    <b-row>
      <b-col>
        <b-collapse :visible="showComments">
          <div v-if="!isLoadingComments">
            <div v-for="comment in comments" :key="comment.id">
              <Comment
                :comment="comment"
                :editing="editing"
                @on-comment-deleted="deleteComment"
                @on-edit-started="onEdit"
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
  IconDefinition
} from "@fortawesome/free-solid-svg-icons";
import { Getter } from "vuex-class";

@Component({
  components: {
    Comment: CommentComponent
  }
})
export default class CommentSectionComponent extends Vue {
  @Getter("user", { namespace: "user" }) user!: User;
  @Prop() parentEntry!: MedicationTimelineEntry;
  private commentService!: IUserCommentService;
  private showComments: boolean = false;
  private showInput: boolean = false;
  private isLoadingComments: boolean = false;
  private isEditMode: boolean = false;
  private editing: UserComment = {
    id: "",
    userProfileId: "",
    parentEntryId: "",
    text: "",
    createdDateTime: new Date(),
    version: 0
  };

  private commentInput: string = "";
  private comments: UserComment[] = [];
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

  private setFocus(): void {
    this.$nextTick(() => {
      (this.$refs.commentInput as HTMLBodyElement).focus();
    });
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

  private toggleCommentInput(): void {
    this.showInput = !this.showInput;
    this.commentInput = "";
    this.setFocus();
  }

  private onSubmit(): void {
    if (this.isEditMode) {
      this.updateComment();
    } else {
      this.addComment();
    }
  }

  private addComment(): void {
    this.isLoadingComments = true;
    let commentPromise = this.commentService
      .createComment({
        text: this.commentInput,
        parentEntryId: this.parentEntry.id,
        userProfileId: this.user.hdid
      })
      .then(() => {
        this.commentInput = "";
        this.getComments();
      })
      .catch(err => {
        console.log("Error adding comment on entry " + this.parentEntry.id);
        console.log(err);
        this.hasErrors = true;
      })
      .finally(() => {
        this.isLoadingComments = false;
      });
  }

  private getComments() {
    this.isLoadingComments = true;
    const parentEntryId = this.parentEntry.id;
    let commentPromise = this.commentService
      .getCommentsForEntry(parentEntryId)
      .then(result => {
        if (result) {
          this.comments = result.resourcePayload;
          this.sortComments();
        }
      })
      .catch(err => {
        console.log("Error loading comments for entry " + this.parentEntry.id);
        console.log(err);
        this.hasErrors = true;
      })
      .finally(() => {
        this.isLoadingComments = false;
      });
  }

  private updateComment(): void {
    this.isLoadingComments = true;
    let commentPromise = this.commentService
      .updateComment({
        id: this.editing.id,
        text: this.commentInput,
        userProfileId: this.editing.userProfileId,
        parentEntryId: this.editing.parentEntryId,
        createdDateTime: this.editing.createdDateTime,
        version: this.editing.version
      })
      .then(result => {
        this.getComments();
        this.commentInput = "";
      })
      .catch(err => {
        console.log(err);
        this.hasErrors = true;
      })
      .finally(() => {
        this.isLoadingComments = false;
        this.showInput = false;
        this.isEditMode = false;
      });
  }

  private deleteComment(comment: UserComment): void {
    if (confirm("Are you sure you want to delete this comment?")) {
      let commentPromise = this.commentService
        .deleteComment(comment)
        .then(result => {
          this.commentInput = "";
          this.getComments();
        })
        .catch(err => {
          console.log(err);
        });
    }
  }

  private onEdit(comment: UserComment) {
    this.showInput = true;
    this.commentInput = comment.text;
    this.editing = comment;
    this.isEditMode = true;
    this.setFocus();
  }
}
</script>
