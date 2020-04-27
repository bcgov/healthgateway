<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.comment-body {
  background-color: $lightGrey;
  border-radius: 10px;
}
</style>
<template>
  <b-col>
    <b-row class="comment-body p-3 my-2">
      {{ this.comment.text }}
    </b-row>
    <b-row class="px-3">
      <span> {{ formatDate(this.comment.createdDateTime) }} </span>
    </b-row>
  </b-col>
</template>
<script lang="ts">
import Vue from "vue";
import UserComment from "@/models/userComment";
import { Prop, Component } from "vue-property-decorator";

@Component
export default class CommentComponent extends Vue {
  @Prop({ default: "Default" }) comment!: UserComment;

  private created() {
    console.log("COMMENT: ", this.comment);
  }

  private formatDate(date: Date) {
    let split = date.toString().split("T");
    const formatted = split[0] + " " + split[1].split(".")[0];
    return formatted;
  }
}
</script>
