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

.dropdown {
  color: $primary;
  text-decoration: none;
}

.dropdown:hover {
  color: inherit;
}
</style>
<template>
  <b-col>
    <b-row
      v-if="mode === 'text'"
      class="comment-body p-2 my-1"
      align-v="center"
    >
      <b-col class="comment-text">{{ comment.text }}</b-col>
      <div class="d-flex flex-row-reverse">
        <b-dropdown
          dropright
          text=""
          :no-caret="true"
          class="dropdown"
          variant="link"
        >
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
        </b-dropdown>
      </div>
    </b-row>
    <b-row
      v-if="mode !== 'text'"
      class="comment-body py-2 my-1"
      align-v="center"
    >
      <div v-if="mode === 'add'">
        <div
          :id="'tooltip-' + comment.parentEntryId"
          class="tooltip-info d-flex pl-2"
        >
          <font-awesome-icon :icon="lockIcon" size="1x"> </font-awesome-icon>
        </div>
        <b-tooltip
          variant="secondary"
          :target="'tooltip-' + comment.parentEntryId"
          placement="left"
          triggers="hover"
        >
          Only you can see comments added to your medical records.
        </b-tooltip>
      </div>
      <div class="comment-input pl-2">
        <b-form @submit.prevent>
          <b-form-textarea
            ref="commentInput"
            v-model="commentInput"
            rows="1"
            no-resize
            :placeholder="placeholder"
            maxlength="1000"
          ></b-form-textarea>
        </b-form>
      </div>
      <div class="d-flex comment-button px-3 flex-row">
        <b-button
          variant="primary"
          :disabled="commentInput === ''"
          class="d-flex"
          @click="onSubmit"
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
    <b-row v-if="mode !== 'add'" class="px-3">
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
  faLock
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

  private get lockIcon(): IconDefinition {
    return faLock;
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
        userProfileId: this.user.hdid
      })
      .then(() => {
        this.commentInput = "";
        this.needsUpdate(this.comment);
      })
      .catch(err => {
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
        version: this.comment.version
      })
      .then(result => {
        this.needsUpdate(this.comment);
      })
      .catch(err => {
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
        .then(result => {
          this.needsUpdate(this.comment);
        })
        .catch(err => {
          console.log(err);
        });
    }
  }

  @Emit()
  needsUpdate(comment: UserComment) {
    return comment;
  }
}
</script>
