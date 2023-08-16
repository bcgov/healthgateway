<script setup lang="ts">
import { computed } from "vue";

interface Props {
    text?: string;
    size?: string;
    maxWidth?: number;
    tooltipTestid?: string;
}
const props = withDefaults(defineProps<Props>(), {
    text: undefined,
    size: "small",
    maxWidth: 300,
    tooltipTestid: undefined,
});

const hasText = computed(() => !!props.text);
</script>

<template>
    <v-tooltip :max-width="maxWidth">
        <template #activator="{ props: slotProps }">
            <v-icon
                v-bind="{ ...slotProps, ...$attrs }"
                icon="info-circle"
                color="primary"
                :size="size"
            />
        </template>
        <span v-if="hasText" :data-testid="tooltipTestid">{{ text }}</span>
        <slot v-else name="default" />
    </v-tooltip>
</template>
