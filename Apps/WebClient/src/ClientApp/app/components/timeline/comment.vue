<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.comment-body {
  background-color: $lightGrey;
  border-radius: 10px;
}

.editing {
  background-color: lightyellow;
}

.commnet-menu {
  color: $soft_text;
}

</style>
<template>
  <b-col>
    <b-row
      class="comment-body p-2 my-1"
      align-v="center"
      v-if="mode === 'text'"
    >
      <b-col>
        {{ comment.text }}
      </b-col>
      <b-col>
        <div class="d-flex flex-row-reverse">
          <b-navbar-nav>
            <b-nav-item-dropdown dropright text="" :no-caret="true">
              <template slot="button-content">
                <font-awesome-icon
                  class="comment-menu"
                  :icon="menuIcon"
                  size="1x"
                ></font-awesome-icon>
              </template>
              <b-dropdown-item class="menuItem" @click="editComment()">
                Edit
              </b-dropdown-item>
              <b-dropdown-item class="menuItem" @click="deleteComment()">
                Delete
              </b-dropdown-item>
            </b-nav-item-dropdown>
          </b-navbar-nav>
        </div>
      </b-col>
    </b-row>
    <b-row
      v-if="mode === 'edit'"
      class="comment-body p-2 my-1"
      align-v="center"
    >
      <b-form @submit.prevent="editComment">
        <b-form-input
          ref="commentInput"
          v-model="commentInput"
          type="text"
          autofocus
          class="form-control"
          placeholder="Editing comment"
          maxlength="1000"
        ></b-form-input>
      </b-form>
    </b-row>
    <b-row v-if="mode === 'add'" class="comment-body p-2 my-1" align-v="center">
      <b-form @submit.prevent="onSubmit">
        <b-form-input
          ref="commentInput"
          v-model="commentInput"
          type="text"
          autofocus
          class="form-control"
          placeholder="Enter a comment"
          maxlength="1000"
        ></b-form-input>
      </b-form>
    </b-row>
    <b-row class="px-3">
      <span> {{ formatDate(comment.createdDateTime) }} </span>
    </b-row>
  </b-col>
</template>
<script lang="ts">
import Vue from "vue";
import UserComment from "@/models/userComment";
import { Prop, Component, Emit, Watch } from "vue-property-decorator";
import { faEllipsisV, IconDefinition } from "@fortawesome/free-solid-svg-icons";
import { IUserCommentService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";

@Component
export default class CommentComponent extends Vue {
  @Prop() comment!: UserComment;

  private commentInput: string = "";

  private commentService!: IUserCommentService;

  private mode: string = "text";
  private hasErrors: boolean = false;

  private mounted() {
    this.commentService = container.get<IUserCommentService>(
      SERVICE_IDENTIFIER.UserCommentService
    );
  }

  private formatDate(date: Date): string {
    return new Date(Date.parse(date + "Z")).toLocaleString();
  }

  private get menuIcon(): IconDefinition {
    return faEllipsisV;
  }

  private onSubmit(): void {
    if (this.isEditMode) {
      console.log("Edit mode");
    } else {
      this.addComment();
    }
  }

  private addComment(): void {
    let commentPromise = this.commentService
      .createComment({
        text: this.commentInput,
        parentEntryId: this.parentEntry.id,
        userProfileId: this.user.hdid,
      })
      .then(() => {
        this.commentInput = "";
        this.getComments();
      })
      .catch((err) => {
        console.log(
          "Error adding comment on entry " + this.comment.parentEntryId
        );
        console.log(err);
        this.hasErrors = true;
      })
      .finally(() => {});
  }

  private editComment(): void {
    this.isEditMode = true;
  }

  private updateComment(): void {
    let commentPromise = this.commentService
      .updateComment({
        id: this.editing.id,
        text: this.commentInput,
        userProfileId: this.editing.userProfileId,
        parentEntryId: this.editing.parentEntryId,
        createdDateTime: this.editing.createdDateTime,
        version: this.editing.version,
      })
      .then((result) => {
        this.onEditStarted(this.comment);
      })
      .catch((err) => {
        console.log(err);
        this.hasErrors = true;
      })
      .finally(() => {
        this.isEditMode = false;
      });
  }

  private deleteComment(comment: UserComment): void {
    if (confirm("Are you sure you want to delete this comment?")) {
      let commentPromise = this.commentService
        .deleteComment(comment)
        .then((result) => {
          this.onCommentDeleted(comment);
        })
        .catch((err) => {
          console.log(err);
        });
    }
  }

  @Emit()
  onCommentDeleted(comment: UserComment) {
    return comment;
  }

  @Emit()
  onEditStarted(comment: UserComment) {
    return comment;
  }
}
</script>
