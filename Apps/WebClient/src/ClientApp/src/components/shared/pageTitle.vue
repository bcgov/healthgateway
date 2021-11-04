<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class PageTitleComponent extends Vue {
    @Prop({ required: true }) title!: string;

    private get hasSlot() {
        return this.$slots.default !== undefined;
    }
}
</script>

<template>
    <div id="pageTitle">
        <b-row no-gutters class="justify-content-end">
            <b-col cols="12" sm>
                <h1 id="subject">{{ title }}</h1>
            </b-col>
            <b-col v-if="hasSlot" cols="auto" class="mb-2"><slot /></b-col>
        </b-row>
        <hr />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#pageTitle {
    color: $primary;
}

#pageTitle hr {
    border-top: 2px solid $primary;
}
</style>
