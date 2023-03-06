<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faExclamationTriangle } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { email, requiredIf, sameAs } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import HtmlTextAreaComponent from "@/components/HtmlTextAreaComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { RegistrationStatus } from "@/constants/registrationStatus";
import ValidationRegEx from "@/constants/validationRegEx";
import type { WebClientConfiguration } from "@/models/configData";
import { ResultError } from "@/models/errors";
import { TermsOfService } from "@/models/termsOfService";
import type { OidcUserInfo } from "@/models/user";
import { CreateUserRequest } from "@/models/userProfile";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IUserProfileService } from "@/services/interfaces";
import PhoneUtil from "@/utility/phoneUtil";

library.add(faExclamationTriangle);

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        LoadingComponent,
        HtmlTextAreaComponent,
    },
};

@Component(options)
export default class RegistrationView extends Vue {
    @Prop()
    inviteKey?: string;

    @Prop()
    inviteEmail?: string;

    @Action("addError", { namespace: "errorBanner" })
    addError!: (params: {
        errorType: ErrorType;
        source: ErrorSourceType;
        traceId: string | undefined;
    }) => void;

    @Action("addCustomError", { namespace: "errorBanner" })
    addCustomError!: (params: {
        title: string;
        source: ErrorSourceType;
        traceId: string | undefined;
    }) => void;

    @Action("setTooManyRequestsError", { namespace: "errorBanner" })
    setTooManyRequestsError!: (params: { key: string }) => void;

    @Action("setTooManyRequestsWarning", { namespace: "errorBanner" })
    setTooManyRequestsWarning!: (params: { key: string }) => void;

    @Action("createProfile", { namespace: "user" })
    createProfile!: (params: { request: CreateUserRequest }) => Promise<void>;

    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;

    @Getter("oidcUserInfo", { namespace: "user" })
    oidcUserInfo!: OidcUserInfo | undefined;

    @Ref("registrationForm")
    form!: HTMLFormElement;

    private accepted = false;
    private email = "";
    private emailConfirmation = "";
    private smsNumber = "";

    private isEmailChecked = true;
    private isSMSNumberChecked = true;

    private userProfileService!: IUserProfileService;
    private submitStatus = "";
    private loadingUserData = true;
    private loadingTermsOfService = true;
    private submittingRegistration = false;
    private clientRegistryError = false;
    private errorMessage = "";

    private logger!: ILogger;
    private isValidAge: boolean | null = null;
    private minimumAge!: number;

    private termsOfService?: TermsOfService;

    private get isLoading(): boolean {
        return (
            this.loadingTermsOfService ||
            this.loadingUserData ||
            this.submittingRegistration
        );
    }

    private get fullName(): string {
        if (this.oidcUserInfo === undefined) {
            return "";
        }
        return `${this.oidcUserInfo.given_name} ${this.oidcUserInfo.family_name}`;
    }

    private get isRegistrationClosed(): boolean {
        return (
            this.webClientConfig.registrationStatus == RegistrationStatus.Closed
        );
    }

    private get isPredefinedEmail(): boolean {
        if (
            this.webClientConfig.registrationStatus != RegistrationStatus.Open
        ) {
            return !!this.inviteEmail;
        }
        return false;
    }

    private get termsOfServiceLoaded(): boolean {
        return !this.isLoading && Boolean(this.termsOfService?.content);
    }

    private mounted(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.minimumAge = this.webClientConfig.minPatientAge;

        if (
            this.webClientConfig.registrationStatus == RegistrationStatus.Open
        ) {
            this.email = "";
            this.emailConfirmation = "";
        } else {
            this.email = this.inviteEmail || "";
            this.emailConfirmation = this.email;
        }

        if (this.oidcUserInfo === undefined) {
            this.addError({
                errorType: ErrorType.Retrieve,
                source: ErrorSourceType.User,
                traceId: undefined,
            });
            return;
        }

        if (this.oidcUserInfo.email !== null) {
            this.email = this.oidcUserInfo.email;
            this.emailConfirmation = this.oidcUserInfo.email;
        }

        this.userProfileService = container.get(
            SERVICE_IDENTIFIER.UserProfileService
        );

        this.loadingUserData = true;
        this.userProfileService
            .validateAge(this.oidcUserInfo.hdid)
            .then((isValid) => {
                this.isValidAge = isValid;
            })
            .catch((err: ResultError) => {
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                } else {
                    this.clientRegistryError = true;
                    this.addCustomError({
                        title:
                            "Unable to validate " +
                            ErrorSourceType.User.toLowerCase(),
                        source: ErrorSourceType.User,
                        traceId: undefined,
                    });
                }
            })
            .finally(() => {
                this.loadingUserData = false;
            });

        this.loadTermsOfService();
    }

    private validations(): unknown {
        const validPhoneNumberFormat = (rawInputSmsNumber: string) => {
            if (!rawInputSmsNumber) {
                return true;
            }
            if (!ValidationRegEx.PhoneNumberMasked.test(rawInputSmsNumber)) {
                return false;
            }
            const phoneNumber = PhoneUtil.stripPhoneMask(rawInputSmsNumber);
            return this.userProfileService.isPhoneNumberValid(phoneNumber);
        };
        return {
            smsNumber: {
                required: requiredIf(() => this.isSMSNumberChecked),
                sms: validPhoneNumberFormat,
            },
            email: {
                required: requiredIf(() => this.isEmailChecked),
                email,
            },
            emailConfirmation: {
                required: requiredIf(() => this.isEmailChecked),
                sameAsEmail: sameAs("email"),
                email,
            },
            accepted: { isChecked: sameAs(() => true) },
        };
    }

    private loadTermsOfService(): void {
        this.loadingTermsOfService = true;
        this.userProfileService
            .getTermsOfService()
            .then((result) => {
                this.logger.debug(
                    `getTermsOfService result: ${JSON.stringify(result)}`
                );
                this.termsOfService = result;
            })
            .catch((err: ResultError) => {
                this.logger.error(err.resultMessage);
                if (err.statusCode === 429) {
                    this.setTooManyRequestsWarning({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Retrieve,
                        source: ErrorSourceType.TermsOfService,
                        traceId: undefined,
                    });
                }
            })
            .finally(() => {
                this.loadingTermsOfService = false;
            });
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    private async onSubmit(): Promise<void> {
        this.$v.$touch();
        if (this.$v.$invalid || this.oidcUserInfo === undefined) {
            this.submitStatus = "ERROR";
            return;
        }

        this.submitStatus = "PENDING";
        if (this.smsNumber) {
            this.smsNumber = this.smsNumber.replace(/\D+/g, "");
        }

        try {
            this.submittingRegistration = true;
            await this.createProfile({
                request: {
                    profile: {
                        hdid: this.oidcUserInfo.hdid,
                        termsOfServiceId: this.termsOfService?.id || "",
                        acceptedTermsOfService: this.accepted,
                        email: this.email || "",
                        isEmailVerified: false,
                        smsNumber: this.smsNumber || "",
                        isSMSNumberVerified: false,
                        preferences: {},
                    },
                    inviteCode: this.inviteKey || "",
                },
            });

            await this.$router.push("/home");
        } catch {
            this.logger.error("Error while registering.");
        } finally {
            this.submittingRegistration = false;
        }
    }

    private onEmailOptout(isChecked: boolean): void {
        if (!isChecked) {
            this.emailConfirmation = "";
            this.email = "";
        }
    }

    private onSMSOptout(isChecked: boolean): void {
        if (!isChecked) {
            this.smsNumber = "";
        }
    }
}
</script>

<template>
    <div>
        <LoadingComponent :is-loading="isLoading" />
        <b-container v-if="termsOfServiceLoaded">
            <div v-if="isRegistrationClosed">
                <page-title title="Closed Registration" />
                <div id="Description">
                    Thank you for your interest in the Health Gateway service.
                    At this time, the registration is closed.
                </div>
            </div>
            <div v-else>
                <b-form
                    v-if="isValidAge === true"
                    ref="registrationForm"
                    @submit.prevent="onSubmit"
                >
                    <page-title title="Registration" />
                    <h4 class="subheading mb-3">
                        Communication Preferences (Optional)
                    </h4>
                    <div class="mb-3">
                        <b-form-checkbox
                            id="emailCheckbox"
                            v-model="isEmailChecked"
                            data-testid="emailCheckbox"
                            @change="onEmailOptout($event)"
                        >
                            Email Notifications
                        </b-form-checkbox>
                        <div>
                            <em class="small">
                                Receive application and health record updates
                            </em>
                        </div>
                        <b-form-input
                            id="emailInput"
                            v-model="$v.email.$model"
                            :disabled="isPredefinedEmail || !isEmailChecked"
                            :state="isValid($v.email)"
                            data-testid="emailInput"
                            placeholder="Your email address"
                            type="email"
                        />
                        <b-form-invalid-feedback :state="isValid($v.email)">
                            Valid email is required
                        </b-form-invalid-feedback>
                    </div>
                    <div v-if="!isPredefinedEmail" class="mb-3">
                        <b-form-input
                            id="emailConfirmationInput"
                            v-model="$v.emailConfirmation.$model"
                            :disabled="!isEmailChecked"
                            :state="isValid($v.emailConfirmation)"
                            data-testid="emailConfirmationInput"
                            placeholder="Confirm your email address"
                            type="email"
                        />
                        <b-form-invalid-feedback
                            :state="$v.emailConfirmation.sameAsEmail"
                        >
                            Emails must match
                        </b-form-invalid-feedback>
                    </div>
                    <!-- SMS section -->
                    <div class="mb-3">
                        <b-form-checkbox
                            id="smsCheckbox"
                            v-model="isSMSNumberChecked"
                            @change="onSMSOptout($event)"
                        >
                            Text Notifications
                        </b-form-checkbox>
                        <div>
                            <em class="small">
                                Receive health record updates only
                            </em>
                        </div>
                        <b-form-input
                            id="smsNumberInput"
                            v-model="$v.smsNumber.$model"
                            v-mask="'(###) ###-####'"
                            :disabled="!isSMSNumberChecked"
                            :state="
                                $v.smsNumber.$pending || isValid($v.smsNumber)
                            "
                            class="d-flex"
                            data-testid="smsNumberInput"
                            placeholder="Your phone number"
                            type="tel"
                        >
                        </b-form-input>
                        <b-form-invalid-feedback
                            v-if="!$v.smsNumber.$pending"
                            :state="isValid($v.smsNumber)"
                        >
                            Valid sms number is required
                        </b-form-invalid-feedback>
                    </div>
                    <div
                        v-if="!isEmailChecked && !isSMSNumberChecked"
                        class="font-weight-bold text-primary"
                    >
                        <hg-icon
                            aria-hidden="true"
                            class="mr-2"
                            icon="exclamation-triangle"
                            size="medium"
                        />
                        <span
                            >You won't receive notifications from the Health
                            Gateway. You can update this from your Profile Page
                            later.</span
                        >
                    </div>
                    <h4 class="subheading mt-4">Terms of Service</h4>
                    <HtmlTextAreaComponent
                        :input="termsOfService.content"
                        class="termsOfService mb-3"
                    />
                    <div class="mb-3">
                        <b-form-checkbox
                            id="accept"
                            v-model="accepted"
                            :state="isValid($v.accepted)"
                            class="accept"
                            data-testid="acceptCheckbox"
                        >
                            I agree to the terms of service above
                        </b-form-checkbox>
                        <b-form-invalid-feedback :state="isValid($v.accepted)">
                            You must accept the terms of service.
                        </b-form-invalid-feedback>
                    </div>
                    <div class="mb-3 text-right">
                        <hg-button
                            :disabled="!accepted"
                            class="px-5"
                            data-testid="registerButton"
                            type="submit"
                            variant="primary"
                            >Register
                        </hg-button>
                    </div>
                </b-form>
                <div v-else-if="isValidAge === false">
                    <h1>Minimum age required for registration</h1>
                    <p data-testid="minimumAgeErrorText">
                        You must be <strong>{{ minimumAge }}</strong> years of
                        age or older to use this application
                    </p>
                </div>
                <div v-else-if="clientRegistryError">
                    <h1>Error retrieving user information</h1>
                    <p data-testid="clientRegistryErrorText">
                        There may be an issue in our Client Registry. Please
                        contact <strong>HealthGateway@gov.bc.ca</strong>
                    </p>
                </div>
                <div v-else><h1>Unknown error</h1></div>
            </div>
        </b-container>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

input {
    width: 320px !important;
    max-width: 320px !important;
}

.accept label {
    color: $primary;
}

.subheading {
    color: $soft_text;
}

.termsOfService {
    max-height: 330px;
    overflow-y: scroll;
    box-shadow: 0 0 2px #00000070;
    border: none;
}
</style>
