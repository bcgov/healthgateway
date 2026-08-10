<script setup lang="ts">
import { computed } from "vue";

interface Props {
    type?: "info" | "success" | "warning" | "error";
    title?: string;
    text?: string;
    closable?: boolean;
    class?: string;
    border?: boolean | "top" | "end" | "bottom" | "start";
    variant?: "flat" | "tonal" | "outlined" | "text" | "plain" | "elevated";
    centerContent?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
    type: "info",
    title: undefined,
    text: undefined,
    closable: false,
    class: "d-print-none mb-4",
    border: false,
    variant: "flat",
    centerContent: false,
});

const outlinedColorClasses = computed(() => {
    switch (props.variant == "outlined" ? props.type : undefined) {
        case "info":
            return "bg-info-background text-info-text border-info-text";
        case "warning":
            return "bg-warning-background text-warning-text border-warning-border";
        case "error":
            return "bg-error-background text-error border-error";
        case "success":
            return undefined;
        default:
            return undefined;
    }
});
</script>

<template>
    <v-alert
        :class="[
            outlinedColorClasses,
            props.centerContent ? 'hg-alert--center-content' : '',
            props.class,
        ]"
        :type="type"
        :title="title"
        :text="text"
        :closable="closable"
        :border="border"
        :variant="variant"
    >
        <template v-if="$slots.title" #title>
            <slot name="title" />
        </template>
        <template v-if="$slots.text" #text>
            <slot name="text" />
        </template>
        <slot />
    </v-alert>
</template>

<style scoped lang="scss">
/*
 * Opt-in: vertically center icon + wrapped text (no-title case can look misaligned).
 * Enabled via the centerContent prop.
 */
.hg-alert--center-content {
    display: flex !important;
    align-items: center !important;
}

.hg-alert--center-content :deep(.v-alert__prepend),
.hg-alert--center-content :deep(.v-alert__content) {
    align-self: center !important;
}

.hg-alert--center-content :deep(.v-alert__content) {
    padding-block: 0 !important;
}

.hg-alert--center-content :deep(.v-alert__prepend svg) {
    display: block !important;
}
</style>
