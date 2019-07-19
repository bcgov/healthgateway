<template>
  <div>
    <ErrorComponent :error="errorDescription" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import PageErrorComponent, { PageError } from "@/components/error.vue";

@Component({
  components: {
    ErrorComponent: PageErrorComponent
  }
})
export default class UnauthorizedError extends Vue {
  private errorDescription: PageError = new PageError (
    "401",
    "Unauthorized",
    "You do not have permission to view this page."
  );
}
</script>


