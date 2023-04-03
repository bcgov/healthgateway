<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class HgCardButtonComponent extends Vue {
    @Prop({ required: true })
    title!: string;

    @Prop({ required: false, default: false })
    dense!: boolean;

    @Prop({ required: false, default: false })
    hasChevron!: boolean;

    private get hasClickListener(): boolean {
        return this.$listeners && Boolean(this.$listeners.click);
    }

    private get hasIconSlot(): boolean {
        return this.$slots.icon !== undefined;
    }

    private get hasMenuSlot(): boolean {
        return this.$slots.menu !== undefined;
    }

    private get hasDefaultSlot(): boolean {
        return this.$slots.default !== undefined;
    }
}
</script>

<template>
    <b-button
        class="hg-card-button h-100 w-100 p-0 shadow"
        v-bind="$attrs"
        v-on="$listeners"
    >
        <hg-card
            :title="title"
            :dense="dense"
            :has-chevron="hasChevron"
            :is-interactable="hasClickListener"
        >
            <template #icon>
                <slot name="icon" />
            </template>
            <template #menu>
                <slot name="menu" />
            </template>
            <slot />
        </hg-card>
    </b-button>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.hg-card-button {
    background-color: transparent;
    border: none;
}
</style>
