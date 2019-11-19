<style lang="scss" scoped>
$border-color: #00000070;

.textArea {
  border: 1px solid $border-color;
  box-shadow: inset 0 0 5px $border-color;
}
</style>

<template>
  <div class="textArea p-4" @input="updateHTML"></div>
</template>

<script lang="ts">
import Vue from "vue";
import PageError from "@/models/pageError";
import { Prop, Component } from "vue-property-decorator";

@Component
export default class HtmlTextAreaComponent extends Vue {
  @Prop() input!: string;

  mounted() {
    this.$el.innerHTML = this.input;
  }

  private updateHTML(e) {
    this.$emit("input", e.target.innerHTML);
  }
}
</script>
