<script setup lang="ts">
interface Props {
    title: string;
    margin?: string;
}
withDefaults(defineProps<Props>(), {
    margin: "mb-4",
});
</script>

<template>
    <h4
        class="text-subtitle-1 font-weight-bold d-flex align-center"
        :class="[margin]"
    >
        <span>{{ title }}</span>
        <slot name="append" />
    </h4>
</template>
