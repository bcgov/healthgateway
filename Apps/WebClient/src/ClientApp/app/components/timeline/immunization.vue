<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

$radius: 15px;

.timelineCard {
  border-radius: $radius $radius $radius $radius;
  border-color: $soft_background;
  border-style: solid;
  border-width: 2px;
}

.entryTitle {
  background-color: $soft_background;
  color: $primary;
  padding: 13px 15px;
  font-weight: bold;
  margin-right: -1px;
  border-radius: 0px $radius 0px 0px;
}

.icon {
  background-color: $primary;
  color: white;
  text-align: center;
  padding: 10px 0;
  border-radius: $radius 0px 0px 0px;
}

.leftPane {
  width: 60px;
  max-width: 60px;
}

.detailsButton {
  padding: 0px;
}

.detailSection {
  margin-top: 15px;
}

.commentButton {
  border-radius: $radius;
}

.newComment {
  border-radius: $radius;
}

.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
  display: none;
}
</style>

<template>
  <b-col class="timelineCard">
    <b-row class="entryHeading">
      <b-col class="icon leftPane">
        <font-awesome-icon :icon="entryIcon" size="2x"></font-awesome-icon>
      </b-col>
      <b-col class="entryTitle">
        {{ entry.immunization.name }}
      </b-col>
    </b-row>
    <b-row class="my-2">
      <b-col class="leftPane"></b-col>
      <b-col>
        <b-row>
          <b-col>
            {{ entry.immunization.agents }}
          </b-col>
        </b-row>
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
                <span v-if="this.hasComments">{{
                  this.comments.length > 1
                    ? this.comments.length + " comments"
                    : "1 comment"
                }}</span>
              </b-btn>
            </div>
          </b-col>
        </b-row>
        <b-row class="py-2" v-if="commentInputVisible">
          <b-col>
            <b-collapse :visible="commentInputVisible">
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
            <b-collapse :visible="commentsVisible">
              <div v-if="!this.isLoadingComments">
                <div v-for="comment in this.comments" :key="comment.id">
                  <Comment :comment="comment"></Comment>
                </div>
              </div>
            </b-collapse>
          </b-col>
        </b-row>
      </b-col>
    </b-row>
  </b-col>
</template>

<script lang="ts">
import Vue from "vue";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import { Prop, Component } from "vue-property-decorator";
import { IUserCommentService } from "@/services/interfaces";
import { Getter } from "vuex-class";
import CommentComponent from "@/components/timeline/comment.vue";
import UserComment from "@/models/userComment";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";

import {
  faSyringe,
  IconDefinition,
  faCommentAlt,
} from "@fortawesome/free-solid-svg-icons";

@Component({
  components: {
    Comment: CommentComponent,
  },
})
export default class ImmunizationTimelineComponent extends Vue {
  @Prop() entry!: ImmunizationTimelineEntry;
  @Prop() index!: number;
  @Prop() datekey!: string;
  @Getter("user", { namespace: "user" }) user!: User;

  private isLoadingComments: boolean = false;
  private hasErrors: boolean = false;

  private commentService!: IUserCommentService;
  private isCommentsVisible: boolean = false;
  private isCommentInputVisible: boolean = false;
  private isCommentSaving: boolean = false;
  private newComment: string = "";
  private comments: UserComment[] = [];
  private numComments = 0;

  private mounted() {
    this.commentService = container.get<IUserCommentService>(
      SERVICE_IDENTIFIER.UserCommentService
    );
    this.getComments();
  }

  private get isLoading(): boolean {
    return this.isLoadingComments;
  }

  private get commentsVisible(): boolean {
    return this.isCommentsVisible;
  }

  private get commentInputVisible(): boolean {
    return this.isCommentInputVisible;
  }

  private get hasComments(): boolean {
    return this.comments.length > 0;
  }

  private get commentsLoaded(): boolean {
    return this.commentsLoaded;
  }

  private get entryIcon(): IconDefinition {
    return faSyringe;
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
    this.isCommentsVisible = !this.commentsVisible;
    if (this.isCommentInputVisible && !this.isCommentsVisible) {
      this.toggleCommentInput();
    }
  }

  private toggleCommentInput(): void {
    this.isCommentInputVisible = !this.isCommentInputVisible;
    if (this.isCommentInputVisible && !this.isCommentsVisible) {
      this.toggleComments();
    }
  }

  private addComment(): void {
    this.isCommentSaving = true;
    let commentPromise = this.commentService
      .createComment({
        text: this.newComment,
        parentEntryId: this.entry.id,
        userProfileId: this.user.hdid,
      })
      .then(() => {
        this.newComment = "";
        this.getComments();
        this.isCommentsVisible = true;
      })
      .catch((err) => {
        console.log("Error adding comment on entry " + this.entry.id);
        console.log(err);
        this.hasErrors = true;
      })
      .finally(() => {
        this.isCommentSaving = false;
      });
  }

  private getComments() {
    const referenceId = this.entry.id;
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
        console.log("Error loading comments for entry " + this.entry.id);
        console.log(err);
        this.hasErrors = true;
        this.isLoadingComments = false;
      });
  }
}
</script>
