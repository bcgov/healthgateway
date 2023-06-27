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
        <div v-html="input" />
    </v-sheet>
</template>
