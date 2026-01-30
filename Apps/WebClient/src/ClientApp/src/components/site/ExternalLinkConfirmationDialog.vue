<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";

const props = withDefaults(
    defineProps<{
        modelValue: boolean;
        title?: string;
        body?: string[];
        confirmLabel?: string;
        cancelLabel?: string;
    }>(),
    {
        title: "",
        body: () => [],
        confirmLabel: "Proceed",
        cancelLabel: "Cancel",
    }
);

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
    (e: "confirm"): void;
    (e: "cancel"): void;
}>();

function close(): void {
    emit("update:modelValue", false);
}

function cancel(): void {
    emit("cancel");
    close();
}

function confirm(): void {
    emit("confirm");
    close();
}
</script>

<template>
    <v-dialog
        :model-value="modelValue"
        data-testid="external-link-confirmation-dialog"
        persistent
        no-click-animation
        @update:model-value="emit('update:modelValue', $event)"
    >
        <div class="d-flex justify-center">
            <v-card max-width="700">
                <v-card-title class="bg-primary px-0">
                    <v-toolbar
                        :title="props.title"
                        density="compact"
                        color="primary"
                    >
                        <HgIconButtonComponent
                            icon="close"
                            aria-label="Close"
                            @click="cancel"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text class="pa-4">
                    <div v-if="props.body?.length">
                        <div
                            v-for="(line, i) in props.body"
                            :key="i"
                            class="text-body-1"
                            :class="{ 'mb-1': i < props.body.length - 1 }"
                            :data-testid="`external-link-confirmation-dialog-body-${i}`"
                        >
                            {{ line }}
                        </div>
                    </div>
                </v-card-text>
                <v-card-actions class="justify-end border-t-sm px-4 py-3">
                    <HgButtonComponent
                        variant="secondary"
                        :uppercase="false"
                        :text="props.cancelLabel"
                        data-testid="external-link-confirmation-dialog-cancel-button"
                        @click="cancel"
                    />
                    <HgButtonComponent
                        variant="primary"
                        :uppercase="false"
                        :text="props.confirmLabel"
                        data-testid="external-link-confirmation-dialog-proceed-button"
                        @click="confirm"
                    />
                </v-card-actions>
            </v-card>
        </div>
    </v-dialog>
</template>
