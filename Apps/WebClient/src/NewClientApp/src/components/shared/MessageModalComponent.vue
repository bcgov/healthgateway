<script setup lang="ts">
import { ref } from "vue";
import HgButtonComponent from "@/components/shared/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/shared/HgIconButtonComponent.vue";

interface Props {
    title: string;
    message: string;
    isLoading?: boolean;
    okOnly?: boolean;
}
withDefaults(defineProps<Props>(), {
    isLoading: false,
    okOnly: false,
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
        <v-row justify="center">
            <v-card :loading="isLoading" max-width="665px">
                <template v-slot:loader="{ isActive }">
                    <v-progress-linear
                        :active="isActive"
                        color="primary"
                        heigh="4"
                        indeterminate
                    />
                </template>
                <v-card-title class="bg-primary">
                    <v-toolbar :title="title" density="compact" color="primary">
                        <HgIconButtonComponent
                            data-testid="messageModalCloseButton"
                            icon="close"
                            @click="handleCancel"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text class="text-body-1 pa-4">
                    <p data-testid="genericMessageText">
                        {{ message }}
                    </p>
                </v-card-text>
                <v-card-actions class="justify-end border-t-sm pa-4">
                    <HgButtonComponent
                        data-testid="genericMessageOkBtn"
                        class="mr-2"
                        :variant="okOnly ? 'primary' : 'secondary'"
                        @click.prevent="handleCancel"
                        :text="okOnly ? 'OK' : 'Cancel'"
                    />
                    <HgButtonComponent
                        data-testid="genericMessageSubmitBtn"
                        variant="primary"
                        @click.prevent="handleSubmit"
                        text="Continue"
                    />
                </v-card-actions>
            </v-card>
        </v-row>
    </v-dialog>
</template>
