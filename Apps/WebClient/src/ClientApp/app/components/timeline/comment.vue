<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.comment-body {
  background-color: $lightGrey;
  border-radius: 10px;
}

.editing {
  background-color: $light-background;
}

.commnet-menu {
  color: $soft_text;
}
</style>
<template>
  <b-col>
    <b-row class="comment-body p-2 my-1" :class="{ editing: isEditing, }" align-v="center">
      <b-col>
        {{ this.comment.text }}
      </b-col>
      <b-col>
        <div class="d-flex flex-row-reverse">
          <!-- Right aligned nav items -->
          <b-navbar-nav>
            <b-nav-item-dropdown dropright text="" :no-caret="true">
              <!-- Using 'button-content' slot -->
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
    <b-row class="px-3">
      <span> {{ formatDate(comment.createdDateTime) }} </span>
    </b-row>
  </b-col>
</template>
<script lang="ts">
import Vue from "vue";
import UserComment from "@/models/userComment";
import { Prop, Component, Emit } from "vue-property-decorator";
import { faEllipsisV, IconDefinition } from "@fortawesome/free-solid-svg-icons";
import { IUserCommentService } from "../../services/interfaces";

@Component
export default class CommentComponent extends Vue {
  @Prop({ default: "Default" }) comment!: UserComment;
  @Prop() service!: IUserCommentService;

  private isEditing: boolean = false;
  private isSaving: boolean = false;
  private text: string = "";
  private parentEntryId: number = 0;
  private createdDateTime: Date = new Date();
  private hasErrors: boolean = false;

  private formatDate(date: Date): string {
    return new Date(Date.parse(date + "Z")).toLocaleString();
  }

  private get menuIcon(): IconDefinition {
    return faEllipsisV;
  }

  deleteComment(): void {
    this.onCommentDeleted(this.comment);
  }

  private editComment(): void {
    this.isEditing = true;
    this.text = this.comment.text;
    this.createdDateTime = this.comment.createdDateTime;
    this.parentEntryId = this.comment.parentEntryId;
    this.onEditStarted(this.comment);
  }

  private onReset(): void {
    this.isEditing = false;
    this.onEditClose(this.comment);
  }

  @Emit()
  onCommentUpdated(comment: UserComment) {
    return comment;
  }

  @Emit()
  onCommentDeleted(comment: UserComment) {
    return comment;
  }

  @Emit()
  onEditStarted(comment: UserComment) {
    return comment;
  }

  @Emit()
  onEditClose(comment: UserComment) {
    return comment;
  }
}
</script>
