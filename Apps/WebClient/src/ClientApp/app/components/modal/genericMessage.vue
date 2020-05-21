<style lang="scss" scoped>
.modal-footer {
  justify-content: flex-end;
}
</style>

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
    <b-row>
      <b-col>
        <form @submit.stop.prevent="handleSubmit">
          <b-row>
            <b-col>
              <span
                >{{ message }}e</span
              >
            </b-col>
          </b-row>
        </form>
      </b-col>
    </b-row>
    <template v-slot:modal-footer>
      <b-row>
        <b-col>
          <b-row>
            <b-col>
              <b-button variant="primary" @click="handleSubmit($event)">
                Continue
              </b-button>
            </b-col>
          </b-row>
        </b-col>
      </b-row>
    </template>
  </b-modal>
</template>

<script lang="ts">
import Vue from "vue";
import { Emit, Prop, Component, Watch } from "vue-property-decorator";

@Component
export default class MessageModalComponent extends Vue {
  @Prop() error!: boolean;
  @Prop({ default: false }) isLoading!: boolean;

  @Prop({ default: 'Info' }) private title!: string;
  @Prop({ default: 'Message' }) private message!: string;
  private isVisible: boolean = false;

  public showModal() {
    this.isVisible = true;
  }

  public hideModal() {
    this.isVisible = false;
  }

  @Emit()
  private submit() {
    this.isVisible = false;
    return;
  }

  @Emit()
  private cancel() {
    this.hideModal();
    return;
  }

  private handleSubmit(bvModalEvt: Event) {
    // Prevent modal from closing
    bvModalEvt.preventDefault();

    // Trigger submit handler
    this.submit();

    // Hide the modal manually
    this.$nextTick(() => {
      this.hideModal();
    });
  }

  private handleCancel(bvModalEvt: Event) {
    // Prevent modal from closing
    bvModalEvt.preventDefault();

    // Trigger cancel handler
    this.cancel();

    // Hide the modal manually
    this.$nextTick(() => {
      this.hideModal();
    });
  }
}
</script>
