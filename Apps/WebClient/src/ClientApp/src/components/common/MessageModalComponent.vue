<script setup lang="ts">
import { ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";

interface Props {
    title: string;
    message: string;
    isLoading?: boolean;
    okOnly?: boolean;
    submitText?: string;
}
withDefaults(defineProps<Props>(), {
    isLoading: false,
    okOnly: false,
    submitText: "Continue",
});

const emit = defineEmits<{
    (e: "submit"): void;
    (e: "cancel"): void;
}>();

defineExpose({ showModal, hideModal });

const isVisible = ref(false);

function showModal(): void {
    isVisible.value = true;
}

function hideModal(): void {
    isVisible.value = false;
}

function handleSubmit(): void {
    hideModal();
    emit("submit");
}

function handleCancel(): void {
    hideModal();
    emit("cancel");
}
</script>

<template>
    <v-dialog
        id="genericMessage"
        v-model="isVisible"
        persistent
        no-click-animation
    >
        <div class="d-flex justify-center">
            <v-card
                :loading="isLoading"
                max-width="700px"
                data-testid="generic-message-modal"
            >
                <template #loader="{ isActive }">
                    <v-progress-linear
                        :active="isActive"
                        color="accent"
                        indeterminate
                    />
                </template>
                <v-card-title class="bg-primary px-0">
                    <v-toolbar :title="title" density="compact" color="primary">
                        <HgIconButtonComponent
                            data-testid="message-modal-close-button"
                            icon="close"
                            aria-label="Close"
                            @click="handleCancel"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text class="text-body-1 pa-4">
                    <p data-testid="generic-message-text">
                        {{ message }}
                    </p>
                </v-card-text>
                <v-card-actions class="justify-end border-t-sm pa-4">
                    <HgButtonComponent
                        data-testid="generic-message-ok-btn"
                        :variant="okOnly ? 'primary' : 'secondary'"
                        :text="okOnly ? 'OK' : 'Cancel'"
                        @click.prevent="handleCancel"
                    />
                    <HgButtonComponent
                        v-if="!okOnly"
                        data-testid="generic-message-submit-btn"
                        class="ml-2"
                        variant="primary"
                        :text="submitText"
                        @click.prevent="handleSubmit"
                    />
                </v-card-actions>
            </v-card>
        </div>
    </v-dialog>
</template>
