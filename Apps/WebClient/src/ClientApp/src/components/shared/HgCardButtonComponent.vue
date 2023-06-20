<script setup lang="ts">
import { computed, useAttrs } from "vue";

interface Props {
    title: string;
    dense?: boolean;
}
withDefaults(defineProps<Props>(), {
    dense: false,
});

const attrs = useAttrs();

const hasClickListener = computed<boolean>(() => Boolean(attrs.onClick));
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
            :is-interactable="hasClickListener"
        >
            <template #icon>
                <slot name="icon" />
            </template>
            <template #menu>
                <slot name="menu" />
            </template>
            <template #action-icon>
                <slot name="action-icon" />
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
