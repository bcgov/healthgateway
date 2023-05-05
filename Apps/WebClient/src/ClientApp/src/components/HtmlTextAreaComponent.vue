<script setup lang="ts">
interface Props {
    input: string;
}
defineProps<Props>();
</script>

<template>
    <div class="textArea p-4" v-html="input" />
</template>

<style lang="scss" scoped>
$border-color: #00000070;

.textArea {
    border: 1px solid $border-color;
    box-shadow: inset 0 0 5px $border-color;
}
</style>
