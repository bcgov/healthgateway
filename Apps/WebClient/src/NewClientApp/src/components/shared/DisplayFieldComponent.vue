<script setup lang="ts">
interface Props {
    name: string;
    value: string;
    nameClass?: string;
    valueClass?: string;
}
defineProps<Props>();
</script>

<template>
    <div class="text-body-1">
        <span :class="nameClass">{{ name }}: </span>
        <span :class="valueClass" v-bind="$attrs">
            {{ value }}
        </span>
    </div>
</template>
