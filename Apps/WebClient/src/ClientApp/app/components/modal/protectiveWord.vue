<style lang="scss" scoped>
@import "../../assets/scss/_variables.scss";
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
    ref="protectiveWord-modal"
    title="Restricted PharmaNet Records"
    header-class="modal-header"
    footer-class="modal-footer"
    centered
    @ok="handleOk"
    @close="cancel"
    @hide="reset"
  >
    <b-row>
      <b-col>
        <form ref="form" @submit.stop.prevent="handleSubmit">
          <b-row>
            <b-col cols="8">
              <b-form-group
                :state="state"
                label="Protective Word"
                class="font-weight-bold pt-2"
                label-for="protectiveWord-input"
                invalid-feedback="Protective word is required"
              >
              </b-form-group>
              <b-form-input
                id="protectiveWord-input"
                v-model="protectiveWord"
                type="password"
                :state="state"
                required
              />
            </b-col>
          </b-row>
        </form>
      </b-col>
    </b-row>
    <template v-slot:modal-footer="{ ok, cancel, hide }">
      <b-row>
        <b-col>
          <b-row>
            <b-col>
              <b-button size="lg" variant="primary" @click="ok()">
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
import { Ref, Emit, Component } from "vue-property-decorator";

@Component
export default class ProtectiveWordComponent extends Vue {
  @Ref("protectiveWord-modal") readonly modal!: HTMLElement;
  @Ref("form") readonly form!: HTMLElement;
  private state!: string = "";
  private protectiveWord!: string = "";

  @Emit()
  public showModal() {
    this.modal.show();
  }

  @Emit()
  public submit() {
    return this.protectiveWord;
  }

  @Emit()
  public cancel() {
    return;
  }

  private reset() {
    this.protectiveWord = "";
    this.state = "";
  }

  private checkFormValidity() {
    const valid = this.form.checkValidity();
    this.state = valid ? "valid" : "invalid";
    return valid;
  }

  private handleOk(bvModalEvt) {
    // Prevent modal from closing
    bvModalEvt.preventDefault();

    // Trigger submit handler
    this.handleSubmit();
  }

  private handleSubmit() {
    if (!this.checkFormValidity()) {
      return;
    }

    this.submit();

    // Hide the modal manually
    this.$nextTick(() => {
      this.modal.hide();
    });
  }
}
</script>
