<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class HtmlTextAreaComponent extends Vue {
    @Prop() input!: string;

    private mounted(): void {
        this.$el.innerHTML = this.input;
    }
}
</script>

<template>
    <div class="textArea p-4"></div>
</template>

<style lang="scss" scoped>
$border-color: #00000070;

.textArea {
    border: 1px solid $border-color;
    box-shadow: inset 0 0 5px $border-color;
}
</style>
