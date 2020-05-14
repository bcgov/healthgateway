<style lang="scss" scoped>
.modal-footer {
  justify-content: flex-end;
}
</style>

<template>
  <b-modal
    id="covid-modal"
    title="COVID-19"
    header-bg-variant="danger"
    header-text-variant="light"
    footer-class="modal-footer"
    :no-close-on-backdrop="true"
    centered
  >
    <b-row>
      <b-col>
        <form @submit.stop.prevent="handleSubmit">
          <b-row>
            <b-col>
              <span
                >Check the status of your COVID-19 test and view your result
                when it is available</span
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
              <b-button variant="outline-primary" @click="handleSubmit($event)">
                View Result
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
export default class CovidModalComponent extends Vue {
  @Prop() error!: boolean;
  @Prop({ default: false }) isLoading!: boolean;

  private isShowing: boolean = false;
  private readonly modalId: string = "covid-modal";

  public showModal() {
    this.isShowing = true;
  }

  public hideModal() {
    this.isShowing = false;
    this.$bvModal.hide(this.modalId);
  }

  @Watch("isLoading")
  private onIsLoading() {
    if (!this.isLoading && this.isShowing) {
      this.$bvModal.show(this.modalId);
    }
  }

  @Emit()
  private submit() {
    this.isShowing = false;
    return;
  }

  @Emit()
  private cancel() {
    this.isShowing = false;
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
