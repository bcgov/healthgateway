<script setup lang="ts">
interface Props {
    title: string;
    subtitle?: string;
}
defineProps<Props>();
</script>

<template>
    <v-row justify="center">
        <v-col cols="12" md="8" lg="6" class="pt-0 text-center">
            <div class="mb-4">
                <h1 class="text-h4 font-weight-bold text-primary mb-2 mt-0">
                    {{ title }}
                </h1>
                <h2 v-if="subtitle" class="text-h6 mt-1">{{ subtitle }}</h2>
            </div>
            <slot />
            <v-divider class="mt-4" />
        </v-col>
    </v-row>
</template>
