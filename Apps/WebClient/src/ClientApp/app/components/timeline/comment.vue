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
      {{ comment.text }}
    </b-row>
    <b-row class="px-3">
      <span> {{ formatDate(comment.createdDateTime) }} </span>
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

   private formatDate(date: Date): string {
    return new Date(Date.parse(date + "Z")).toLocaleString();
  }
}
</script>
