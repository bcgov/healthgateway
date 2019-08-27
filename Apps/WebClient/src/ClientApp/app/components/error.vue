<style lang="scss" scoped>
.error-template {
  padding: 40px 15px;
}
</style>

<template>
  <div class="row">
    <div class="col d-flex justify-content-center">
      <div class="error-template">
        <h1>{{ error.code }}</h1>
        <h2>{{ error.name }}</h2>
        <p>{{ error.message }}</p>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import PageError from "@/models/pageError";
import { Prop, Component } from "vue-property-decorator";

@Component
export default class PageErrorComponent extends Vue {
  @Prop(PageError) error!: PageError;
}
</script>
