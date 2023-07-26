<script setup lang="ts">
interface Props {
    name: string;
    value?: string | null;
    nameClass?: string;
    valueClass?: string;
}
defineProps<Props>();
</script>

<template>
    <p class="text-body-1">
        <span :class="nameClass">{{ name }}: </span>
        <span :class="valueClass" v-bind="$attrs">
            {{ value || "N/A" }}
        </span>
    </p>
</template>
