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
      :class="{ editing: isEditMode }"
      align-v="center"
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

@Component
export default class CommentComponent extends Vue {
  @Prop() comment!: UserComment;
  @Prop() editing!: UserComment;

  private isEditMode: boolean = false;
  private hasErrors: boolean = false;

  private formatDate(date: Date): string {
    return new Date(Date.parse(date + "Z")).toLocaleString();
  }

  private get menuIcon(): IconDefinition {
    return faEllipsisV;
  }

  @Watch("editing")
  private onEditMode(): void {
    this.editing.id === this.comment.id
      ? (this.isEditMode = true)
      : (this.isEditMode = false);
  }

  private deleteComment(): void {
    this.onCommentDeleted(this.comment);
  }

  private editComment(): void {
    this.onEditStarted(this.comment);
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
