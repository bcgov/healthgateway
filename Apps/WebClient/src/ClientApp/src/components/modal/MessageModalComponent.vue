<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop } from "vue-property-decorator";

@Component
export default class MessageModalComponent extends Vue {
    @Prop() error!: boolean;
    @Prop({ default: false }) isLoading!: boolean;

    @Prop({ default: "Info" }) private title!: string;
    @Prop({ default: "Message" }) private message!: string;
    @Prop({ default: false }) private okOnly!: boolean;

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
                                @click="handleCancel($event)"
                            >
                                OK
                            </hg-button>
                        </b-col>
                        <b-col v-else>
                            <hg-button
                                data-testid="genericMessageSubmitBtn"
                                class="mr-2"
                                variant="primary"
                                @click="handleSubmit($event)"
                            >
                                Continue
                            </hg-button>
                            <hg-button
                                variant="secondary"
                                @click="handleCancel($event)"
                            >
                                Cancel
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
