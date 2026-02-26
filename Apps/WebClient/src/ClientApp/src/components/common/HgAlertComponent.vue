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

const backgroundClass = computed(() => {
    switch (props.type) {
        case "info":
            return "hg-alert-outlined-info";
        case "success":
            return "";
        case "warning":
            if (props.variant === "outlined") {
                return "hg-alert-outlined-warning";
            } else if (props.variant === "text") {
                return "hg-alert-text-warning";
            }
            return "";
        case "error":
            return "hg-alert-outlined-error";
        default:
            return "";
    }
});
</script>

<template>
    <v-alert
        :class="[
            props.variant === 'outlined' ||
            (props.variant === 'text' && props.type === 'warning')
                ? backgroundClass
                : '',
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
.hg-alert-outlined-info {
    background-color: rgb(var(--v-theme-infoBackground)) !important;
    color: rgb(var(--v-theme-infoText)) !important;
}

// Warning
.v-alert.hg-alert-outlined-warning {
    background-color: #fff4dc !important; // Soft yellow background
}

.hg-alert-outlined-warning {
    color: #5c3a00 !important; // Dark brown for text
    border-color: #f9b23d !important; // Golden border
}

.hg-alert-text-warning {
    color: #7a4f00 !important;
}

.hg-alert-text-warning .v-alert__prepend i {
    color: #7a4f00 !important;
}

// Error
.v-alert.hg-alert-outlined-error {
    background-color: #fbd5d8 !important; // Darker pink background
}

.hg-alert-outlined-error {
    color: #d8292f !important; // Strong red for text
    border-color: #b00020 !important; // Deeper red border
}

.hg-alert-outlined-error .v-alert__prepend i {
    color: #d8292f !important; // Strong red for icon
}

.hg-alert-outlined-error .v-alert-title {
    font-weight: bold;
    color: #b00020 !important; // Deeper red border
}

// Support for inner expansion panels in error alert
.hg-alert-outlined-error::v-deep(.v-expansion-panels),
.hg-alert-outlined-error::v-deep(.v-expansion-panel) {
    background-color: #fde7e9 !important; // Light pink
    border: 1px solid #e5a9ad; // Soft pinkish red
    border-radius: 6px;
}

.hg-alert-outlined-error::v-deep(.v-expansion-panel-text) {
    background-color: #fde7e9 !important;
    border-top: 1px solid #e5a9ad;
    padding-top: 0.5rem;
    padding-bottom: 0 !important;
    margin-top: 0.5rem;
}

.hg-alert-outlined-error::v-deep(button.v-expansion-panel-title) {
    font-weight: bold !important;
    color: #b00020 !important;
    padding-bottom: 0.25rem !important;
}

.hg-alert-outlined-error::v-deep(.v-expansion-panel-title__overlay) {
    padding-top: 0 !important;
    padding-bottom: 0 !important;
}

.hg-alert-outlined-error::v-deep(.v-expansion-panel-title) {
    margin-top: 0 !important;
    margin-bottom: 0.5rem !important;
    min-height: auto !important;
    height: auto !important;
}

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
