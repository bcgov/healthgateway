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
                    <b-row v-if="tooManyRetries">
                        <b-col class="text-center">
                            Too many failed attempts.
                        </b-col>
                    </b-row>
                    <b-row v-if="!tooManyRetries">
                        <b-col>
                            <label
                                for="verificationCode-input"
                                class="text-center w-100"
                            >
                                Enter the verification code sent to <br />
                                <strong>{{
                                    formatPhoneNumber(smsNumber)
                                }}</strong>
                            </label>
                            <b-form-input
                                id="verificationCode-input"
                                v-model="smsVerificationCode"
                                size="lg"
                                :autofocus="true"
                                class="text-center"
                                :state="error ? false : undefined"
                                max-length="6"
                                :disabled="isLoading"
                                required
                                @update="onVerificationChange"
                            />
                        </b-col>
                    </b-row>
                    <b-row v-if="error && !tooManyRetries">
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
            <b-row class="w-100">
                <b-col v-if="!tooManyRetries && allowRetry">
                    Didn't receive a code?
                    <b-button
                        id="resendSMSVerification"
                        variant="link"
                        class="ml-0 pl-0"
                        :disabled="smsVerificationSent"
                        @click="sendUserSMSUpdate()"
                    >
                        Resend
                    </b-button>
                </b-col>
                <b-col v-if="!allowRetry">
                    Your code has been sent. You can resend after
                    {{ config.timeouts.resendSMS }}
                    {{ config.timeouts.resendSMS > 1 ? "minutes" : "minute" }}.
                </b-col>
                <b-col v-if="tooManyRetries">
                    <b-button
                        id="resendSMSVerification"
                        variant="link"
                        class="text-center w-100"
                        :disabled="smsVerificationSent"
                        @click="sendUserSMSUpdate()"
                    >
                        Send new code
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
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger, IUserProfileService } from "@/services/interfaces";
import UserSMSInvite from "@/models/userSMSInvite";
import { WebClientConfiguration } from "@/models/configData";
import moment from "moment";

@Component({
    components: {
        LoadingComponent,
    },
})
export default class VerifySMSComponent extends Vue {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    @Prop() smsNumber!: string;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Action("getUserSMS", { namespace: "user" })
    getUserSMS!: (params: { hdid: string }) => Promise<UserSMSInvite>;

    @Action("updateSMSResendDateTime", { namespace: "user" })
    updateSMSResendDateTime!: ({
        hdid,
        dateTime,
    }: {
        hdid: string;
        dateTime: Date;
    }) => void;

    @Getter("SMSResendDateTime", { namespace: "user" })
    SMSResendDateTime!: Date;

    private userProfileService!: IUserProfileService;

    private tooManyRetries: boolean = false;
    private allowRetry: boolean = false;
    private smsVerificationSent: boolean = false;
    private smsVerificationCode: string = "";
    private isVisible: boolean = false;
    private isLoading: boolean = false;
    private isValid: boolean = false;
    public error: boolean = false;

    private mounted() {
        this.userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );
        this.getVerification();
        this.setResendTimeout();
    }

    private getVerification() {
        this.getUserSMS({ hdid: this.user.hdid }).then((result) => {
            this.tooManyRetries = result && result.tooManyFailedAttempts;
            if (this.tooManyRetries) {
                this.error = false;
            }
            if (result.expired) {
                this.sendUserSMSUpdate();
            }
        });
    }


    @Watch("SMSResendDateTime")
    private onSMSResendDateTimeChanged() {
        this.setResendTimeout();
    }

    private setResendTimeout(): void {
        let smsTimeoutMilliseconds = moment(this.SMSResendDateTime)
            .add(this.config.timeouts!.resendSMS, "minutes")
            .diff(moment(), "milliseconds");

        this.allowRetry = smsTimeoutMilliseconds <= 0;
        if (!this.allowRetry) {
            setTimeout(() => {
                this.allowRetry = true;
            }, smsTimeoutMilliseconds);
        }
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
            .validateSMS(this.user.hdid, this.smsVerificationCode)
            .then((result) => {
                this.error = !result;
                this.getVerification();
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
        this.updateSMSResendDateTime({
            hdid: this.user.hdid,
            dateTime: new Date(),
        });
        this.userProfileService
            .updateSMSNumber(this.user.hdid, this.smsNumber)
            .then(() => {
                this.getVerification();
                setTimeout(() => {
                    this.smsVerificationSent = false;
                }, 5000);
            })
            .catch((err) => {
                this.logger.error(`updateSMSNumber with error: ${err}`);
            });
    }

    private onVerificationChange(): void {
        if (this.smsVerificationCode.length >= 6) {
            this.verifySMS();
        }
    }

    private formatPhoneNumber(phoneNumber: string) {
        var cleaned = ("" + phoneNumber).replace(/\D/g, "");
        var match = cleaned.match(/^(\d{3})(\d{3})(\d{4})$/);
        if (match) {
            return "(" + match[1] + ") " + match[2] + "-" + match[3];
        }
        return null;
    }
}
</script>
