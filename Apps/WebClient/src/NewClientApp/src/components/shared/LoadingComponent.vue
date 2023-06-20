<script setup lang="ts">
interface Props {
    isLoading: boolean;
    text?: string;
}
withDefaults(defineProps<Props>(), {
    text: undefined,
});
</script>

<template>
    <v-overlay
        class="align-center justify-center"
        :model-value="isLoading"
        :persistent="true"
    >
        <div class="d-flex flex-column align-center">
            <v-progress-circular
                indeterminate
                color="primary"
                size="64"
                data-testid="loadingSpinner"
            />
            <p class="mt-4">{{ text }}</p>
        </div>
    </v-overlay>
</template>
