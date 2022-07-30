<script lang="ts">
import VueCountdown from "@chenfengyuan/vue-countdown";
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/LoadingComponent.vue";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IUserProfileService } from "@/services/interfaces";

@Component({
    components: {
        LoadingComponent,
        VueCountdown,
        TooManyRequestsComponent,
    },
})
export default class VerifySMSComponent extends Vue {
    @Prop() smsNumber!: string;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Action("setTooManyRequestsError", { namespace: "errorBanner" })
    setTooManyRequestsError!: (params: { key: string }) => void;

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
    private validationError = false;
    private error = false;

    @Watch("smsResendDateTime")
    private onSMSResendDateTimeChanged(): void {
        this.setResendTimeout();
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );
    }

    private mounted(): void {
        this.setResendTimeout();
    }

    public showModal(): void {
        this.isVisible = true;
    }

    public hideModal(): void {
        this.isVisible = false;
    }

    private setResendTimeout(): void {
        if (this.smsResendDateTime === undefined) {
            this.allowRetry = true;
            return;
        }

        const smsTimeWhenEnabled = this.smsResendDateTime.add({
            minutes: this.config.timeouts.resendSMS,
        });

        const now = new DateWrapper();
        this.allowRetry = smsTimeWhenEnabled.isBefore(now);
        if (!this.allowRetry) {
            const millisecondsToExpire =
                smsTimeWhenEnabled.diff(now).milliseconds;
            setTimeout(() => (this.allowRetry = true), millisecondsToExpire);
        }
    }

    @Emit()
    private submit(): void {
        this.isVisible = false;
    }

    @Emit()
    private cancel(): void {
        this.hideModal();
    }

    private handleOk(bvModalEvt: Event): void {
        // Prevent modal from closing
        bvModalEvt.preventDefault();

        // Trigger submit handler
        this.handleSubmit();
    }

    private handleSubmit(): void {
        this.submit();

        // Hide the modal manually
        this.$nextTick(() => this.hideModal());
    }

    private verifySMS(): void {
        this.smsVerificationCode = this.smsVerificationCode.replace(/\D/g, "");
        this.isLoading = true;
        this.userProfileService
            .validateSMS(this.user.hdid, this.smsVerificationCode)
            .then((result) => {
                this.validationError = !result;
                if (!this.validationError) {
                    this.handleSubmit();
                }
            })
            .catch((err: ResultError) => {
                this.logger.error(err);
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "verifySmsModal" });
                } else {
                    this.error = true;
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
            .then(() =>
                setTimeout(() => (this.smsVerificationSent = false), 5000)
            )
            .catch((err) =>
                this.logger.error(`updateSMSNumber with error: ${err}`)
            );
    }

    private getTimeout(): number {
        let resendTime: DateWrapper;
        if (!this.smsResendDateTime) {
            let now = new DateWrapper();
            this.updateSMSResendDateTime({
                hdid: this.user.hdid,
                dateTime: now,
            });
            resendTime = now;
        } else {
            resendTime = this.smsResendDateTime;
        }
        resendTime = resendTime.add(this.config.timeouts.resendSMS * 60 * 1000);
        return resendTime.diff(new DateWrapper()).milliseconds;
    }

    private onVerificationChange(): void {
        if (this.smsVerificationCode.length >= 6) {
            this.verifySMS();
        }
    }

    private formatPhoneNumber(phoneNumber: string): string | null {
        let cleaned = ("" + phoneNumber).replace(/\D/g, "");
        let match = cleaned.match(/^(\d{3})(\d{3})(\d{4})$/);
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
        ok-only
        @show="getTimeout"
    >
        <b-row>
            <b-col>
                <form>
                    <TooManyRequestsComponent location="verifySmsModal" />
                    <b-row v-if="error">
                        <b-col class="text-center">
                            An unexpected error has occurred. Please try
                            refreshing your browser or try again later.
                        </b-col>
                    </b-row>
                    <b-row
                        v-else-if="tooManyRetries"
                        data-testid="verifySMSModalErrorAttemptsText"
                    >
                        <b-col class="text-center">
                            Too many failed attempts.
                        </b-col>
                    </b-row>
                    <b-row v-else data-testid="verifySMSModalText">
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
                                :state="validationError ? false : undefined"
                                max-length="6"
                                :disabled="isLoading"
                                required
                                @update="onVerificationChange"
                            />
                        </b-col>
                    </b-row>
                    <b-row v-if="!error && !tooManyRetries && validationError">
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
        <template v-if="!error" #modal-footer>
            <b-row class="w-100">
                <b-col v-if="!tooManyRetries && allowRetry">
                    Didn't receive a code?
                    <hg-button
                        id="resendSMSVerification"
                        variant="link"
                        class="m-0 p-0"
                        :disabled="smsVerificationSent"
                        @click="sendUserSMSUpdate()"
                    >
                        Resend
                    </hg-button>
                </b-col>
                <b-col v-if="!allowRetry">
                    <VueCountdown :time="getTimeout()">
                        <template slot-scope="props"
                            >Your code has been sent. You can resend after
                            <span data-testid="countdownText"
                                >{{
                                    props.minutes > 0 ? props.minutes + "m" : ""
                                }}
                                {{ props.seconds }}s</span
                            ></template
                        >
                    </VueCountdown>
                </b-col>
                <b-col v-if="tooManyRetries">
                    <hg-button
                        id="resendSMSVerification"
                        variant="link"
                        class="text-center w-100"
                        :disabled="smsVerificationSent"
                        @click="sendUserSMSUpdate()"
                    >
                        Send New Code
                    </hg-button>
                </b-col>
            </b-row>
        </template>
        <LoadingComponent :is-loading="isLoading" />
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
