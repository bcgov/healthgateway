<script setup lang="ts">
interface Props {
    input?: string;
}
withDefaults(defineProps<Props>(), {
    input: "",
});
</script>

<template>
    <v-sheet class="pa-6" border>
        <!-- eslint-disable vue/no-v-html -->
        <div v-html="input" />
    </v-sheet>
</template>
