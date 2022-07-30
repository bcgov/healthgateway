<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faExclamationTriangle } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { email, helpers, requiredIf, sameAs } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import HtmlTextAreaComponent from "@/components/HtmlTextAreaComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { RegistrationStatus } from "@/constants/registrationStatus";
import type { WebClientConfiguration } from "@/models/configData";
import { ResultError } from "@/models/errors";
import { TermsOfService } from "@/models/termsOfService";
import type { OidcUserInfo } from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IUserProfileService } from "@/services/interfaces";

library.add(faExclamationTriangle);

@Component({
    components: {
        LoadingComponent,
        HtmlTextAreaComponent,
    },
})
export default class RegistrationView extends Vue {
    @Prop() inviteKey?: string;
    @Prop() inviteEmail?: string;

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

    @Action("checkRegistration", { namespace: "user" })
    checkRegistration!: () => Promise<boolean>;

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
    private clientRegistryError = false;
    private errorMessage = "";

    private logger!: ILogger;
    private isValidAge?: boolean;
    private minimumAge!: number;

    private termsOfService?: TermsOfService;

    private get isLoading(): boolean {
        return this.loadingTermsOfService || this.loadingUserData;
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
        const sms = helpers.regex("sms", /^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$/);
        return {
            smsNumber: {
                required: requiredIf(() => this.isSMSNumberChecked),
                sms,
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
                this.logger.error(err);
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

    private onSubmit(event: Event): void {
        this.$v.$touch();
        if (this.$v.$invalid || this.oidcUserInfo === undefined) {
            this.submitStatus = "ERROR";
            event.preventDefault();
            return;
        }

        this.submitStatus = "PENDING";
        if (this.smsNumber) {
            this.smsNumber = this.smsNumber.replace(/\D+/g, "");
        }
        this.loadingTermsOfService = true;
        this.userProfileService
            .createProfile({
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
            })
            .then((result) => {
                this.logger.debug(
                    `Create Profile result: ${JSON.stringify(result)}`
                );
                this.redirect();
            })
            .catch((err: ResultError) => {
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Create,
                        source: ErrorSourceType.Profile,
                        traceId: err.traceId,
                    });
                }
            })
            .finally(() => {
                this.loadingTermsOfService = false;
            });

        event.preventDefault();
    }

    private redirect(): void {
        this.checkRegistration().then((isRegistered: boolean) => {
            if (!isRegistered) {
                this.addError({
                    errorType: ErrorType.Create,
                    source: ErrorSourceType.Profile,
                    traceId: undefined,
                });
                return;
            }

            const defaultRoute = this.webClientConfig.modules[
                "VaccinationStatus"
            ]
                ? "/home"
                : "/timeline";

            this.$router.push({
                path:
                    this.smsNumber === "" && this.email === ""
                        ? defaultRoute
                        : "/profile",
                query: {
                    toVerifyPhone: this.smsNumber === "" ? "false" : "true",
                    toVerifyEmail: this.email === "" ? "false" : "true",
                },
            });
        });
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

    private get termsOfServiceLoaded(): boolean {
        return !this.isLoading && Boolean(this.termsOfService?.content);
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
                    v-if="isValid === true"
                    ref="registrationForm"
                    @submit.prevent="onSubmit"
                >
                    <page-title title="Registration" />
                    <b-row class="mb-2">
                        <b-col>
                            <h4 class="subheading">
                                Communication Preferences (Optional)
                            </h4>
                        </b-col>
                    </b-row>
                    <b-row class="mb-3">
                        <b-col>
                            <b-row class="d-flex">
                                <b-col class="d-flex pr-0">
                                    <b-form-checkbox
                                        id="emailCheckbox"
                                        v-model="isEmailChecked"
                                        data-testid="emailCheckbox"
                                        @change="onEmailOptout($event)"
                                    >
                                        Email Notifications
                                    </b-form-checkbox>
                                </b-col>
                            </b-row>
                            <b-row class="d-flex">
                                <b-col class="d-flex pr-0">
                                    <em class="small">
                                        Receive application and health record
                                        updates
                                    </em>
                                </b-col>
                            </b-row>
                            <b-form-input
                                id="emailInput"
                                v-model="$v.email.$model"
                                data-testid="emailInput"
                                type="email"
                                placeholder="Your email address"
                                :disabled="isPredefinedEmail || !isEmailChecked"
                                :state="isValid($v.email)"
                            />
                            <b-form-invalid-feedback :state="isValid($v.email)">
                                Valid email is required
                            </b-form-invalid-feedback>
                        </b-col>
                    </b-row>
                    <b-row v-if="!isPredefinedEmail" class="mb-3">
                        <b-col>
                            <b-form-input
                                id="emailConfirmationInput"
                                v-model="$v.emailConfirmation.$model"
                                data-testid="emailConfirmationInput"
                                type="email"
                                placeholder="Confirm your email address"
                                :disabled="!isEmailChecked"
                                :state="isValid($v.emailConfirmation)"
                            />
                            <b-form-invalid-feedback
                                :state="$v.emailConfirmation.sameAsEmail"
                            >
                                Emails must match
                            </b-form-invalid-feedback>
                        </b-col>
                    </b-row>
                    <!-- SMS section -->
                    <b-row class="mb-3">
                        <b-col>
                            <b-row class="d-flex">
                                <b-col class="d-flex pr-0">
                                    <b-form-checkbox
                                        id="smsCheckbox"
                                        v-model="isSMSNumberChecked"
                                        @change="onSMSOptout($event)"
                                    >
                                        Text Notifications
                                    </b-form-checkbox>
                                </b-col>
                            </b-row>
                            <b-row class="d-flex">
                                <b-col class="d-flex pr-0">
                                    <em class="small">
                                        Receive health record updates only
                                    </em>
                                </b-col>
                            </b-row>
                            <b-form-input
                                id="smsNumberInput"
                                v-model="$v.smsNumber.$model"
                                v-mask="'(###) ###-####'"
                                type="tel"
                                data-testid="smsNumberInput"
                                class="d-flex"
                                placeholder="Your phone number"
                                :state="isValid($v.smsNumber)"
                                :disabled="!isSMSNumberChecked"
                            >
                            </b-form-input>
                            <b-form-invalid-feedback
                                :state="isValid($v.smsNumber)"
                            >
                                Valid sms number is required
                            </b-form-invalid-feedback>
                        </b-col>
                    </b-row>
                    <b-row v-if="!isEmailChecked && !isSMSNumberChecked">
                        <b-col class="font-weight-bold text-primary">
                            <hg-icon
                                icon="exclamation-triangle"
                                size="medium"
                                aria-hidden="true"
                                class="mr-2"
                            />
                            <span
                                >You won't receive notifications from the Health
                                Gateway. You can update this from your Profile
                                Page later.</span
                            >
                        </b-col>
                    </b-row>
                    <b-row class="mt-4">
                        <b-col>
                            <h4 class="subheading">Terms of Service</h4>
                        </b-col>
                    </b-row>
                    <b-row class="mb-3">
                        <b-col>
                            <HtmlTextAreaComponent
                                class="termsOfService"
                                :input="termsOfService.content"
                            />
                        </b-col>
                    </b-row>
                    <b-row class="mb-3">
                        <b-col>
                            <b-form-checkbox
                                id="accept"
                                v-model="accepted"
                                data-testid="acceptCheckbox"
                                class="accept"
                                :state="isValid($v.accepted)"
                            >
                                I agree to the terms of service above
                            </b-form-checkbox>
                            <b-form-invalid-feedback
                                :state="isValid($v.accepted)"
                            >
                                You must accept the terms of service.
                            </b-form-invalid-feedback>
                        </b-col>
                    </b-row>
                    <b-row class="mb-5">
                        <b-col class="justify-content-right">
                            <hg-button
                                class="px-5 float-right"
                                type="submit"
                                data-testid="registerButton"
                                variant="primary"
                                :disabled="!accepted"
                                >Register</hg-button
                            >
                        </b-col>
                    </b-row>
                </b-form>
                <div v-else-if="isValid === false">
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
