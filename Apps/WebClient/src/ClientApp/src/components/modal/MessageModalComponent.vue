<script setup lang="ts">
import { ref } from "vue";

interface Props {
    title: string;
    message: string;
    error?: boolean;
    isLoading?: boolean;
    okOnly?: boolean;
}
withDefaults(defineProps<Props>(), {
    error: false,
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
    <b-modal
        id="generic-message"
        v-model="isVisible"
        :title="title"
        header-bg-variant="primary"
        header-text-variant="light"
        footer-class="modal-footer"
        centered
    >
        <b-row data-testid="genericMessageModal">
            <b-col>
                <form @submit.stop.prevent="handleSubmit">
                    <b-row>
                        <b-col>
                            <span data-testid="genericMessageText">
                                {{ message }}
                            </span>
                        </b-col>
                    </b-row>
                </form>
            </b-col>
        </b-row>
        <template #modal-footer>
            <b-row>
                <b-col>
                    <b-row>
                        <b-col v-if="okOnly">
                            <hg-button
                                data-testid="genericMessageOkBtn"
                                class="mr-2"
                                variant="primary"
                                @click.prevent="handleCancel"
                            >
                                OK
                            </hg-button>
                        </b-col>
                        <b-col v-else>
                            <hg-button
                                class="mr-2"
                                variant="secondary"
                                @click.prevent="handleCancel"
                            >
                                Cancel
                            </hg-button>
                            <hg-button
                                data-testid="genericMessageSubmitBtn"
                                variant="primary"
                                @click.prevent="handleSubmit"
                            >
                                Continue
                            </hg-button>
                        </b-col>
                    </b-row>
                </b-col>
            </b-row>
        </template>
    </b-modal>
</template>

<style lang="scss" scoped>
.modal-footer {
    justify-content: flex-end;
}
</style>
