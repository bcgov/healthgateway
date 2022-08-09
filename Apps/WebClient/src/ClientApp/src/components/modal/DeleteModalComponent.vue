<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop } from "vue-property-decorator";

@Component
export default class DeleteModalComponent extends Vue {
    @Prop() error!: boolean;
    @Prop({ default: false }) isLoading!: boolean;

    @Prop({ default: "Info" }) private title!: string;
    @Prop({ default: "Message" }) private message!: string;
    private isVisible = false;

    public showModal(): void {
        this.isVisible = true;
    }

    public hideModal(): void {
        this.isVisible = false;
    }

    @Emit()
    private submit(): void {
        this.isVisible = false;
    }

    @Emit()
    private cancel(): void {
        this.hideModal();
    }

    private handleSubmit(bvModalEvt: Event): void {
        // Prevent modal from closing
        bvModalEvt.preventDefault();

        // Trigger submit handler
        this.submit();

        // Hide the modal manually
        this.$nextTick(() => this.hideModal());
    }

    private handleCancel(bvModalEvt: Event): void {
        // Prevent modal from closing
        bvModalEvt.preventDefault();

        // Trigger cancel handler
        this.cancel();

        // Hide the modal manually
        this.$nextTick(() => this.hideModal());
    }
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
                                variant="secondary"
                                @click="handleCancel($event)"
                            >
                                Cancel
                            </hg-button>
                            <hg-button
                                data-testid="confirmDeleteBtn"
                                class="mr-2"
                                variant="primary"
                                @click="handleSubmit($event)"
                            >
                                Yes, I'm Sure
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
