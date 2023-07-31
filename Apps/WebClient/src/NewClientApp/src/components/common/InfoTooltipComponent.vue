<script setup lang="ts">
interface Props {
    text?: string;
    size?: string;
    maxWidth?: number;
    tooltipTestid?: string;
}
withDefaults(defineProps<Props>(), {
    text: undefined,
    size: "small",
    maxWidth: 300,
    tooltipTestid: undefined,
});
</script>

<template>
    <v-tooltip :text="text" :max-width="maxWidth" :data-testid="tooltipTestid">
        <template #activator="{ props: slotProps }">
            <v-icon
                v-bind="{ ...slotProps, ...$attrs }"
                icon="info-circle"
                color="primary"
                :size="size"
            />
        </template>
        <slot name="default" />
    </v-tooltip>
</template>
