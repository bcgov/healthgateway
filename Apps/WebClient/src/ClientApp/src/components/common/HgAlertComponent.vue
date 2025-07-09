<script setup lang="ts">
import { useAttrs } from "vue";
const attrs = useAttrs();

const DEFAULT_ALERT_CLASS = "d-print-none mb-4";

interface Props {
    type?: "info" | "success" | "warning" | "error";
    title?: string;
    text?: string;
    custom?: boolean;
}

withDefaults(defineProps<Props>(), {
    type: "info",
    title: undefined,
    text: undefined,
    custom: false,
});
</script>

<template>
    <v-alert
        v-bind="attrs"
        :class="[
            custom ? 'hg-alert' : undefined,
            DEFAULT_ALERT_CLASS,
            attrs.class,
        ]"
        :type="type"
        :title="title"
        :text="text"
    >
        <template #text>
            <slot name="text" />
        </template>
        <slot />
    </v-alert>
</template>

<style scoped lang="scss">
.hg-alert {
    box-shadow: none !important;
    background-color: #e5edf5 !important;
    color: #1a2e49 !important;
    border: 1px solid #b4c8db !important;
    border-radius: 8px !important;

    &.elevation-0,
    &.elevation-1,
    &.elevation-2,
    &.v-sheet {
        box-shadow: none !important;
    }

    .v-alert__content {
        padding: 0.5rem 0;
    }

    .v-alert__title {
        color: #1a2e49 !important;
    }

    .v-alert__icon {
        background-color: #005fa8 !important;
        color: white !important;
        border-radius: 50%;
        width: 24px;
        height: 24px;
        min-width: 24px;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .hg-alert__link {
        color: #005fa8;
        text-decoration: underline;
    }
}
</style>
