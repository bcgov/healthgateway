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
}

const props = withDefaults(defineProps<Props>(), {
    type: "info",
    title: undefined,
    text: undefined,
    closable: false,
    class: "d-print-none mb-4",
    border: false,
    variant: "flat",
});

const backgroundClass = computed(() => {
    switch (props.type) {
        case "info":
            return "hg-alert-info";
        case "success":
            return "hg-alert-success";
        case "warning":
            return "hg-alert-warning";
        case "error":
            return "hg-alert-error";
        default:
            return "";
    }
});
</script>

<template>
    <v-alert
        :class="[backgroundClass, props.class]"
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
.hg-alert-info {
    background-color: rgb(var(--v-theme-infoBackground)) !important;
    color: rgb(var(--v-theme-infoText)) !important;
}

.hg-alert-success {
    background-color: #e6f4ea !important; // placeholder color
    color: #1b4332 !important; // placeholder text color
}

.hg-alert-warning {
    background-color: #e6f4ea !important; // placeholder color
    color: #1b4332 !important; // placeholder text color
}

.hg-alert-error {
    background-color: #fdecea !important; // placeholder color
    color: #611a15 !important; // placeholder text color
}
</style>
