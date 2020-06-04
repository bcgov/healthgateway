<template>
  <div>
    <ErrorComponent :error="errorDescription" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import PageErrorComponent from "@/components/error.vue";
import PageError from "@/models/pageError";

@Component({
  components: {
    ErrorComponent: PageErrorComponent
  }
})
export default class UnauthorizedComponent extends Vue {
  public errorDescription: PageError = new PageError(
    "401",
    "Unauthorized",
    "You do not have permission to view this page."
  );
}
</script>
