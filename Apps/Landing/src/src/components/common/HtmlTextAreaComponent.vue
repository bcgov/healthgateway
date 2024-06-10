<script setup lang="ts">
interface Props {
    input?: string;
}
withDefaults(defineProps<Props>(), {
    input: "",
});
</script>

<template>
    <v-sheet class="document pa-6" border>
        <!-- eslint-disable vue/no-v-html -->
        <div v-html="input" />
        <!-- eslint-enable vue/no-v-html -->
    </v-sheet>
</template>

<style lang="scss">
.document a {
    color: rgb(var(--v-theme-link));
}
</style>
