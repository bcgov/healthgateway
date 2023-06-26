<script setup lang="ts">
interface Props {
    title: string;
    subtitle?: string;
}
defineProps<Props>();
</script>

<template>
    <v-row justify="center">
        <v-col cols="6" class="pt-12">
            <h1 class="text-h3 font-weight-bold text-primary">{{ title }}</h1>
            <h2 class="text-h5 pb-4" v-if="subtitle">{{ subtitle }}</h2>
            <slot />
            <v-divider class="mt-4"></v-divider>
        </v-col>
    </v-row>
</template>
