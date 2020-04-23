<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.text-large {
  font-size: 250%;
}
.modal-header {
  background-color: $primary;
  color: $primary_text;

  button,
  button:hover {
    color: #fff;
  }
}
.modal-footer {
  justify-content: flex-start;
  button {
    padding: 5px 20px 5px 20px;
  }
}
</style>

<template>
  <b-modal
    id="protective-word-modal"
    title="Restricted PharmaNet Records"
    header-class="modal-header"
    footer-class="modal-footer"
    centered
  >
    <b-row>
      <b-col>
        <form @submit.stop.prevent="handleSubmit">
          <b-row>
            <b-col cols="8">
              <label for="protectiveWord-input">Protective Word </label>
              <b-form-input
                id="protectiveWord-input"
                v-model="protectiveWord"
                type="password"
                required
              />
            </b-col>
          </b-row>
          <b-row v-if="error">
            <b-col>
              <span class="text-danger"
                >Invalid protective word. Try again.</span
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
              <b-button
                size="lg"
                variant="primary"
                :disabled="!protectiveWord"
                @click="handleOk($event)"
              >
                Continue
              </b-button>
            </b-col>
          </b-row>
          <br />
          <b-row>
            <b-col>
              <small>
                Please enter the protective word required to access these
                restricted PharmaNet records.
              </small>
            </b-col>
          </b-row>
          <b-row>
            <b-col>
              <small>
                For more information visit
                <a
                  href="https://www2.gov.bc.ca/gov/content/health/health-drug-coverage/pharmacare-for-bc-residents/pharmanet/protective-word-for-a-pharmanet-record"
                  >protective-word-for-a-pharmanet-record</a
                >
              </small>
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
export default class ProtectiveWordComponent extends Vue {
  @Prop() error!: boolean;
  @Prop({ default: false }) isLoading!: boolean;

  private isShowing: boolean = false;
  private protectiveWord: string = "";
  private readonly modalId: string = "protective-word-modal";

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
    return this.protectiveWord;
  }

  @Emit()
  private cancel() {
    this.isShowing = false;
    return;
  }

  private handleOk(bvModalEvt: Event) {
    // Prevent modal from closing
    bvModalEvt.preventDefault();

    // Trigger submit handler
    this.handleSubmit();
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
