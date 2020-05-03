<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.comment-body {
  background-color: $lightGrey;
  border-radius: 10px;
  display: flex;
}

.editing {
  background-color: lightyellow;
}

.commnet-menu {
  color: $soft_text;
}

.comment-text {
  white-space: pre-line;
}

.comment-input {
  flex: 1 1 auto;
}

.comment-button {
  flex: 0 0 auto;
  flex-direction: row;
}
</style>
<template>
  <b-col>
    <b-row
      class="comment-body p-2 my-1"
      align-v="center"
      v-if="mode === 'text'"
    >
      <b-col class="comment-text">{{ comment.text }}</b-col>
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
      v-if="mode !== 'text'"
      class="comment-body py-2 my-1"
      align-v="center"
    >
      <div class="comment-input pl-2">
        <b-form @submit.prevent>
          <b-form-textarea
            rows="1"
            no-resize
            ref="commentInput"
            v-model="commentInput"
            :placeholder="placeholder"
            maxlength="1000"
          ></b-form-textarea>
        </b-form>
      </div>
      <div class="d-flex comment-button px-3 flex-row">
        <b-button
          variant="primary"
          @click="onSubmit"
          :disabled="commentInput === ''"
          class="d-flex"
        >
          Save
        </b-button>
        <div v-if="mode === 'edit'" class="d-flex pl-2">
          <b-button variant="secondary" @click="onCancel">
            Cancel
          </b-button>
        </div>
      </div>
    </b-row>
    <b-row class="px-3" v-if="mode !== 'add'">
      <span> {{ formatDate(comment.createdDateTime) }} </span>
    </b-row>
  </b-col>
</template>
<script lang="ts">
import Vue from "vue";
import UserComment from "@/models/userComment";
import User from "@/models/user";
import { Getter } from "vuex-class";
import { Prop, Component, Emit, Watch } from "vue-property-decorator";
import {
  faEllipsisV,
  IconDefinition,
  faCommentAlt,
} from "@fortawesome/free-solid-svg-icons";
import { IUserCommentService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";

@Component
export default class CommentComponent extends Vue {
  @Getter("user", { namespace: "user" }) user!: User;
  @Prop() comment!: UserComment;
  private commentInput: string = "";
  private commentService!: IUserCommentService;
  private mode: string = "text";
  private hasErrors: boolean = false;

  private mounted() {
    if (this.comment.id === "") {
      this.mode = "add";
    }
    this.commentService = container.get<IUserCommentService>(
      SERVICE_IDENTIFIER.UserCommentService
    );
  }

  private get commentIcon(): IconDefinition {
    return faCommentAlt;
  }

  private get placeholder(): string {
    if (this.mode === "edit") {
      return "Editing a comment";
    } else {
      return "Add a private comment";
    }
  }

  private formatDate(date: Date): string {
    return new Date(Date.parse(date + "Z")).toLocaleString();
  }

  private get menuIcon(): IconDefinition {
    return faEllipsisV;
  }

  private onSubmit(): void {
    if (this.mode === "edit") {
      this.updateComment();
    } else {
      this.addComment();
    }
  }

  private onCancel(): void {
    this.mode = "text";
  }

  private addComment(): void {
    let commentPromise = this.commentService
      .createComment({
        text: this.commentInput,
        parentEntryId: this.comment.parentEntryId,
        userProfileId: this.user.hdid,
      })
      .then(() => {
        this.commentInput = "";
        this.onCommentCreated(this.comment);
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
    this.commentInput = this.comment.text;
    this.mode = "edit";
  }

  private updateComment(): void {
    let commentPromise = this.commentService
      .updateComment({
        id: this.comment.id,
        text: this.commentInput,
        userProfileId: this.comment.userProfileId,
        parentEntryId: this.comment.parentEntryId,
        createdDateTime: this.comment.createdDateTime,
        version: this.comment.version,
      })
      .then((result) => {
        this.onCommentUpdated(this.comment);
      })
      .catch((err) => {
        console.log(err);
        this.hasErrors = true;
      })
      .finally(() => {
        this.mode = "text";
      });
  }

  private deleteComment(): void {
    if (confirm("Are you sure you want to delete this comment?")) {
      let commentPromise = this.commentService
        .deleteComment(this.comment)
        .then((result) => {
          this.onCommentDeleted(this.comment);
        })
        .catch((err) => {
          console.log(err);
        });
    }
  }

  @Emit()
  onCommentCreated(comment: UserComment) {
    return comment;
  }

  @Emit()
  onCommentDeleted(comment: UserComment) {
    return comment;
  }

  @Emit()
  onCommentUpdated(comment: UserComment) {
    return comment;
  }
}
</script>
