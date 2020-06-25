<template>
  <div class="vld-parent">
    <loading :active.sync="isLoading" :is-full-page="true"></loading>
  </div>
</template>

<script lang="ts">
// Import component
import Loading from "vue-loading-overlay";
// Import stylesheet
import "vue-loading-overlay/dist/vue-loading.css";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component({
  components: {
    Loading,
  },
})
export default class LoadingComponent extends Vue {
  @Prop() isLoading!: boolean;
}
</script>
