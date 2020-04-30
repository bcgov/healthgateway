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
              comments.length > 1
                ? comments.length + " comments"
                : "1 comment"
            }}</span>
          </b-btn>
        </div>
      </b-col>
    </b-row>
    <b-row class="py-2" v-if="showInput">
      <b-col>
        <b-collapse :visible="inputVisible">
          <b-form @submit.prevent="addComment">
            <b-form-input
              type="text"
              autofocus
              class="newComment"
              v-model="newComment"
              placeholder="Enter a comment"
              maxlength="1000"
            ></b-form-input>
          </b-form>
        </b-collapse>
      </b-col>
    </b-row>
    <b-row>
      <b-col>
        <b-collapse :visible="showComments">
          <div v-if="!this.isLoadingComments">
            <div v-for="comment in this.comments" :key="comment.id">
              <Comment :comment="comment"></Comment>
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
  private showComments: boolean = false;
  private showInput: boolean = false;
  private isCommentSaving: boolean = false;
  private isLoadingComments: boolean = false;
  private newComment: string = "";
  private comments: UserComment[] = [];
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
    this.showComments = !this.showComments;
  }

  private toggleCommentInput(): void {
    this.showInput = !this.showInput;
    this.newComment = "";
  }

  private addComment(): void {
    this.isCommentSaving = true;
    let commentPromise = this.commentService
      .createComment({
        text: this.newComment,
        parentEntryId: this.parentEntry.id,
        userProfileId: this.user.hdid,
      })
      .then(() => {
        this.newComment = "";
        this.getComments();
        this.showComments = true;
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
}
</script>
