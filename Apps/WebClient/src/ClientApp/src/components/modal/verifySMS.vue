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
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import VueCountdown from "@chenfengyuan/vue-countdown";

@Component({
    components: {
        LoadingComponent,
        countdown: VueCountdown,
    },
})
export default class VerifySMSComponent extends Vue {
    @Prop() smsNumber!: string;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Action("getUserSMS", { namespace: "user" })
    getUserSMS!: (params: { hdid: string }) => Promise<UserSMSInvite | null>;

    @Action("updateSMSResendDateTime", { namespace: "user" })
    updateSMSResendDateTime!: ({
        hdid,
        dateTime,
    }: {
        hdid: string;
        dateTime: DateWrapper;
    }) => void;

    @Getter("smsResendDateTime", { namespace: "user" })
    smsResendDateTime?: DateWrapper;

    private logger!: ILogger;
    private userProfileService!: IUserProfileService;

    private tooManyRetries = false;
    private allowRetry = false;
    private smsVerificationSent = false;
    private smsVerificationCode = "";
    private isVisible = false;
    private isLoading = false;
    private isValid = false;
    public error = false;

    public showModal(): void {
        this.isVisible = true;
    }

    public hideModal(): void {
        this.isVisible = false;
    }

    @Watch("smsResendDateTime")
    private onSMSResendDateTimeChanged() {
        this.setResendTimeout();
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );
        this.getVerification();
        this.setResendTimeout();
    }

    private getVerification() {
        this.getUserSMS({ hdid: this.user.hdid }).then((result) => {
            this.tooManyRetries = result ? result.tooManyFailedAttempts : false;
            if (this.tooManyRetries) {
                this.error = false;
            }
            if (result !== null && result.expired) {
                this.sendUserSMSUpdate();
            }
        });
    }

    private setResendTimeout(): void {
        if (this.smsResendDateTime === undefined) {
            this.allowRetry = true;
            return;
        }

        let smsTimeWhenEnabled: DateWrapper = this.smsResendDateTime.add({
            minutes: this.config.timeouts.resendSMS,
        });

        let now = new DateWrapper();
        this.allowRetry = smsTimeWhenEnabled.isBefore(now);
        if (!this.allowRetry) {
            let millisecondsToExpire = smsTimeWhenEnabled.diff(now);
            setTimeout(() => {
                this.allowRetry = true;
            }, millisecondsToExpire);
        }
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
            dateTime: new DateWrapper(),
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

    private getTimeout(): number {
        if (!this.smsResendDateTime) {
            this.updateSMSResendDateTime({
                hdid: this.user.hdid,
                dateTime: new DateWrapper(),
            });
        }
        let resendTime = new Date();
        resendTime.setTime(
            this.smsResendDateTime!.toJSDate().getTime() +
                this.config.timeouts!.resendSMS * 60 * 1000
        );
        return resendTime.getTime() - new Date().getTime();
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

<template>
    <b-modal
        id="verify-sms-modal"
        v-model="isVisible"
        data-testid="verifySMSModal"
        title="Phone Verification"
        size="sm"
        header-bg-variant="primary"
        header-text-variant="light"
        centered
        @show="getTimeout"
    >
        <b-row>
            <b-col>
                <form>
                    <b-row
                        v-if="tooManyRetries"
                        data-testid="verifySMSModalErrorAttemptsText"
                    >
                        <b-col class="text-center">
                            Too many failed attempts.
                        </b-col>
                    </b-row>
                    <b-row
                        v-if="!tooManyRetries"
                        data-testid="verifySMSModalText"
                    >
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
                                data-testid="verifySMSModalCodeInput"
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
                            <span
                                class="text-danger"
                                data-testid="verifySMSModalErrorInvalidText"
                                >Invalid verification code. Try again.</span
                            >
                        </b-col>
                    </b-row>
                </form>
            </b-col>
        </b-row>
        <template #modal-footer>
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
                    <countdown :time="getTimeout()">
                        <template slot-scope="props"
                            >Your code has been sent. You can resend after
                            {{ props.minutes > 0 ? props.minutes + "m" : "" }}
                            {{ props.seconds }}s</template
                        >
                    </countdown>
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

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.modal-footer {
    justify-content: flex-start;
    button {
        padding: 5px 20px 5px 20px;
    }
}
</style>
