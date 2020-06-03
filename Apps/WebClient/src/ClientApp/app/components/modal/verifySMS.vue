<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.modal-footer {
  justify-content: flex-start;
  button {
    padding: 5px 20px 5px 20px;
  }
}
</style>

<template>
  <b-modal
    id="verify-sms-modal"
    v-model="isVisible"
    title="Phone Verification"
    size="sm"
    header-bg-variant="primary"
    header-text-variant="light"
    centered
  >
    <b-row>
      <b-col>
        <form>
          <b-row>
            <b-col>
              <label for="verificationCode-input" class="text-center">
                Enter the verification code sent to
                <strong>{{ smsNumber }}</strong>
              </label>
              <b-form-input
                id="verificationCode-input"
                v-model="smsVerificationCode"
                size="lg"
                :autofocus="true"
                class="text-center"
                :state="!error"
                max-length="6"
                :disabled="isLoading"
                required
                @update="onVerificationChange"
              />
            </b-col>
          </b-row>
          <b-row v-if="error">
            <b-col>
              <span class="text-danger"
                >Invalid verification code. Try again.</span
              >
            </b-col>
          </b-row>
        </form>
      </b-col>
    </b-row>
    <template v-slot:modal-footer>
      <b-row>
        <b-col>
          <b-button
            id="resendSMSVerification"
            variant="link"
            class="ml-auto"
            :disabled="smsVerificationSent"
            @click="sendUserSMSUpdate()"
          >
            Didn't receive a code? Resend verification code
          </b-button>
        </b-col>
      </b-row>
    </template>
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
  </b-modal>
</template>

<script lang="ts">
import Vue from "vue";
import LoadingComponent from "@/components/loading.vue";
import { Emit, Prop, Component, Watch } from "vue-property-decorator";
import { Getter } from "vuex-class";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IUserProfileService } from "@/services/interfaces";

@Component({
  components: {
    LoadingComponent
  }
})
export default class VerifySMSComponent extends Vue {
  error: boolean = false;
  @Prop() smsNumber!: string;
  @Getter("user", { namespace: "user" }) user!: User;

  private userProfileService!: IUserProfileService;

  private smsVerificationSent: boolean = false;
  private smsVerificationCode: string = "";
  private isVisible: boolean = false;
  private isLoading: boolean = false;
  private isValid: boolean = false;
  mounted() {
    this.userProfileService = container.get<IUserProfileService>(
      SERVICE_IDENTIFIER.UserProfileService
    );
  }
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

  private verifySMS(): void {
    this.smsVerificationCode = this.smsVerificationCode.replace(/\D/g, "");
    this.isLoading = true;
    this.userProfileService
      .validateSMS(this.smsVerificationCode)
      .then(result => {
        this.error = !result;
        this.smsVerified = result;
        if (!this.error) {
          this.handleSubmit();
        }
      })
      .finally(() => {
        this.smsVerificationCode = "";
        this.isLoading = false;
      });
  }

  private sendUserSMSUpdate(): void {
    this.smsVerificationSent = true;
    this.userProfileService
      .updateSMSNumber(this.user.hdid, this.smsNumber)
      .then(() => {
        setTimeout(() => {
          this.smsVerificationSent = false;
        }, 5000);
      })
      .catch(err => {
        this.hasErrors = true;
        console.log(err);
      });
  }

  private onVerificationChange(): void {
    if (this.smsVerificationCode.length >= 6) {
      this.verifySMS();
    }
  }
}
</script>
