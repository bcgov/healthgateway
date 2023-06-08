<script setup lang="ts">
import { computed } from "vue";

interface Props {
    status: string;
    variant?: string;
    heading?: string;
}
const props = withDefaults(defineProps<Props>(), {
    variant: "",
    heading: "Status",
});

const classes = computed(() => {
    switch (props.variant) {
        case "success":
            return ["text-success"];
        case "danger":
            return ["text-danger"];
        default:
            return [];
    }
});
</script>

<template>
    <div class="hg-status-label">
        <span class="text-muted">{{ heading }}: </span>
        <span v-bind="$attrs" :class="classes">{{ status }}</span>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.hg-status-label {
    color: $hg-text-primary !important;

    .text-muted {
        color: $hg-text-secondary !important;
    }

    .text-success {
        color: $hg-state-success !important;
    }

    .text-danger {
        color: $hg-state-danger !important;
    }
}
</style>
