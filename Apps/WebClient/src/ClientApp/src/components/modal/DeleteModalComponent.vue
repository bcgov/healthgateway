<script setup lang="ts">
import { ref } from "vue";

interface Props {
    title?: string;
    message?: string;
    confirm?: string;
}
withDefaults(defineProps<Props>(), {
    title: "Info",
    message: "Message",
    confirm: "Yes, I'm Sure",
});

const emit = defineEmits<{
    (e: "submit"): void;
    (e: "cancel"): void;
}>();

defineExpose({
    showModal,
    hideModal,
});

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
        id="delete-confirmation"
        v-model="isVisible"
        data-testid="deleteConfirmationModal"
        :title="title"
        header-bg-variant="danger"
        header-text-variant="light"
        footer-class="modal-footer"
        centered
    >
        <b-row>
            <b-col>
                <form @submit.stop.prevent="handleSubmit">
                    <b-row>
                        <b-col>
                            <span>{{ message }}</span>
                        </b-col>
                    </b-row>
                </form>
            </b-col>
        </b-row>
        <template #modal-footer>
            <b-row>
                <b-col>
                    <b-row>
                        <b-col>
                            <hg-button
                                data-testid="cancelDeleteBtn"
                                class="mr-2"
                                variant="secondary"
                                @click.prevent="handleCancel"
                            >
                                Cancel
                            </hg-button>
                            <hg-button
                                data-testid="confirmDeleteBtn"
                                variant="primary"
                                @click.prevent="handleSubmit"
                            >
                                {{ confirm }}
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
