<style lang="scss">
@import "@/assets/scss/_variables.scss";
.text-large {
  font-size: 250%;
}
.modal-header {
  background-color: darkred;
  color: $primary_text;

  .close
  {
      color: $primary_text;
  }
}
.modal-footer {
  justify-content: flex-end;
}
</style>

<template>
  <b-modal
    id="covid-modal"
    title="COVID-19"
    header-class="modal-header"
    footer-class="modal-footer"
    centered
  >
    <b-row>
      <b-col>
        <form @submit.stop.prevent="handleSubmit">
          <b-row>
            <b-col>
              <span>Are you here to view your COVID-19 test results?</span>
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
              <b-button size="lg" variant="outline-primary" @click="handleYes($event)">
                YES
              </b-button>
            </b-col>
            <b-col>
              <b-button size="lg" variant="outline-primary" @click="handleNo($event)">
                NO
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

  private handleYes(bvModalEvt: Event) {
    // Prevent modal from closing
    bvModalEvt.preventDefault();

    // Trigger submit handler
    this.handleSubmit();
  }

  private handleNo(bvModalEvt: Event) {
    // Prevent modal from closing
    bvModalEvt.preventDefault();

    // Trigger cancel handler
    this.cancel();

    // Hide the modal manually
    this.$nextTick(() => {
      this.hideModal();
    });
  }

  private handleSubmit() {
    this.submit();

    // Hide the modal manually
    this.$nextTick(() => {
      this.hideModal();
    });
  }
}
</script>
