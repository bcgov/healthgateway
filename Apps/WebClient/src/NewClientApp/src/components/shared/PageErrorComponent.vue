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
            <div class="mb-4">
                <h1 class="text-h3 font-weight-bold text-primary">
                    {{ title }}
                </h1>
                <h2 class="text-h5 mt-2" v-if="subtitle">{{ subtitle }}</h2>
            </div>
            <slot />
            <v-divider class="mt-4"></v-divider>
        </v-col>
    </v-row>
</template>
