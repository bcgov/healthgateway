<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.column-wrapper {
    border: 1px;
}

#pageTitle {
    color: $primary;
}

#pageTitle hr {
    border-top: 2px solid $primary;
}
</style>

<template>
    <div id="pageTitle">
        <h1>{{ title }}</h1>
        <hr />
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class PageTitleComponent extends Vue {
    @Prop() title!: string;
}
</script>
