<style scoped lang="scss">
@import "@/assets/scss/_variables.scss";
</style>
<template>
  <div>
    <b-row class="pt-2">
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
    <b-row>
      <b-col>
        <Comment :comment="newComment" @needs-update="needsUpdate"></Comment>
      </b-col>
    </b-row>
    <b-row>
      <b-col>
        <b-collapse :visible="showComments">
          <div v-if="!isLoadingComments">
            <div v-for="comment in comments" :key="comment.id">
              <Comment :comment="comment" @needs-update="needsUpdate"></Comment>
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
import { Prop, Component } from "vue-property-decorator";
import { IUserCommentService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";

@Component({
  components: {
    Comment: CommentComponent
  }
})
export default class CommentSectionComponent extends Vue {
  @Prop() parentEntry!: MedicationTimelineEntry;
  private commentService!: IUserCommentService;
  private showComments: boolean = false;
  private showInput: boolean = false;
  private isLoadingComments: boolean = false;
  private comments: UserComment[] = [];
  private hasErrors: boolean = false;
  private newComment: UserComment = {
    id: "",
    text: "",
    parentEntryId: this.parentEntry.id,
    userProfileId: "",
    createdDateTime: new Date(),
    version: 0
  };

  private mounted() {
    this.commentService = container.get<IUserCommentService>(
      SERVICE_IDENTIFIER.UserCommentService
    );
    this.getComments();
  }

  private get hasComments(): boolean {
    return this.comments.length > 0;
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

  private needsUpdate(comment: UserComment) {
    this.getComments();
  }
}
</script>
