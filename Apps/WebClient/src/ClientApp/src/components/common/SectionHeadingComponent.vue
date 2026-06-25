<script setup lang="ts">
interface Props {
    title: string;
    includeDivider?: boolean;
}
withDefaults(defineProps<Props>(), {
    includeDivider: false,
});
</script>

<template>
    <v-divider v-if="includeDivider" class="my-4" />
    <h3 v-bind="$attrs" class="text-h6 font-weight-bold my-4">{{ title }}</h3>
</template>
