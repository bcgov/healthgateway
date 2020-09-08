<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#pageTitle {
    color: $primary;
}

#pageTitle hr {
    border-top: 2px solid $primary;
}

#Description {
    font-size: 1.2em;
}

input {
    width: 320px !important;
    max-width: 320px !important;
}

.accept label {
    color: $primary;
}

.subheading {
    color: $soft_text;
    font-size: inherit;
    margin-bottom: 5px;
}

label {
    font-size: 1.2em;
    margin: 0px;
}

#termsOfService {
    background-color: $light_background;
    color: $soft_text;
}
</style>
<template>
    <b-container class="py-5">
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <b-row>
            <b-col>
                <b-alert :show="hasErrors" dismissible variant="danger">
                    <h4>Error</h4>
                    <p>
                        An unexpected error occured while processing the
                        request:
                    </p>
                    <span>{{ errorMessage }}</span>
                </b-alert>
            </b-col>
        </b-row>
        <div v-if="!isLoading && termsOfService !== ''">
            <b-row v-if="isRegistrationClosed">
                <b-col>
                    <div id="pageTitle">
                        <h1 id="Subject">Closed Registration</h1>
                        <div id="Description">
                            Thank you for your interest in the Health Gateway
                            service. At this time, the registration is closed.
                        </div>
                    </div>
                </b-col>
            </b-row>
            <b-row v-else-if="isRegistrationInviteOnly && !inviteKey">
                <b-col>
                    <div id="pageTitle">
                        <h1 id="Subject">
                            Enter your email to join the waitlist
                        </h1>
                        <hr />
                    </div>
                    <b-row class="mb-3">
                        <b-col>
                            <label for="waitlistNames" class="font-weight-bold"
                                >Full Name</label
                            >
                            <b-row
                                ><b-col id="waitlistNames">
                                    {{ fullName }}
                                </b-col>
                                <b-col
                                    id="authenticatedLabel"
                                    class="ml-auto text-right text-success"
                                    >Authenticated
                                    <font-awesome-icon
                                        icon="check"
                                        aria-hidden="true"
                                    ></font-awesome-icon>
                                </b-col>
                            </b-row>
                        </b-col>
                    </b-row>
                    <b-row class="mb-3">
                        <b-col>
                            <label
                                for="waitlistEditEmail"
                                class="font-weight-bold"
                                >Email Address</label
                            >
                            <b-button
                                v-if="
                                    !waitlistEdditable &&
                                    waitlistTempEmail &&
                                    !waitlistedSuccessfully
                                "
                                id="waitlistEditEmail"
                                class="mx-auto"
                                variant="link"
                                @click="makeWaitlistEdditable()"
                                >Edit
                            </b-button>
                            <div class="form-inline">
                                <b-form-input
                                    id="waitlistEmail"
                                    v-model="$v.email.$model"
                                    type="email"
                                    placeholder="Your email address"
                                    :disabled="!waitlistEdditable"
                                    :state="isValid($v.email)"
                                />
                                <div
                                    v-if="
                                        !waitlistEdditable && waitlistTempEmail
                                    "
                                    id="authenticatedLabel"
                                    class="ml-auto text-right text-warning"
                                >
                                    Waitlisted
                                </div>
                            </div>
                            <b-form-invalid-feedback :state="isValid($v.email)">
                                Valid email is required
                            </b-form-invalid-feedback>
                            <b-form-invalid-feedback :state="$v.email.newEmail">
                                New email must be different from the previous
                                one
                            </b-form-invalid-feedback>
                        </b-col>
                    </b-row>
                    <b-row v-if="waitlistEdditable" class="mb-3">
                        <b-col>
                            <b-form-input
                                id="waitlistEmailConfirmation"
                                v-model="$v.emailConfirmation.$model"
                                type="email"
                                placeholder="Confirm your email address"
                                :state="isValid($v.emailConfirmation)"
                            />
                            <b-form-invalid-feedback
                                :state="$v.emailConfirmation.sameAsEmail"
                            >
                                Emails must match
                            </b-form-invalid-feedback>
                        </b-col>
                    </b-row>
                    <b-row
                        v-if="waitlistEdditable"
                        class="mb-3 justify-content-end"
                    >
                        <b-col class="text-right">
                            <b-button
                                v-if="waitlistTempEmail"
                                id="cancelBtn"
                                class="mx-2 actionButton"
                                @click="cancelWaitlistEdit()"
                                >Cancel
                            </b-button>
                            <b-button
                                id="saveBtn"
                                variant="primary"
                                class="mx-2 actionButton"
                                :disabled="waitlistTempEmail === email"
                                @click="saveWaitlistEdit()"
                                ><span v-if="waitlistTempEmail">Save</span>
                                <span v-else>Join Waitlist</span>
                            </b-button>
                        </b-col>
                    </b-row>
                    <b-row>
                        <b-col
                            v-if="waitlistedSuccessfully"
                            class="font-weight-bold text-primary text-center"
                        >
                            Thanks! Your email has been added to the wait list.
                            You should receive an email confirmation from Health
                            Gateway shortly.
                        </b-col>
                    </b-row>
                </b-col>
            </b-row>
            <b-form v-else ref="registrationForm" @submit.prevent="onSubmit">
                <b-row class="my-3">
                    <b-col>
                        <div id="pageTitle">
                            <h1 id="Subject">Communication Preferences</h1>
                            <div id="Description">
                                Health Gateway will send you notifications via
                                email and based on your preference, may send
                                email and/or text for other notifications.
                            </div>
                        </div>
                    </b-col>
                </b-row>
                <b-row class="mb-3">
                    <b-col>
                        <b-row class="d-flex">
                            <b-col class="d-flex pr-0">
                                <b-form-checkbox
                                    id="emailCheckbox"
                                    v-model="isEmailChecked"
                                    @change="onEmailOptout($event)"
                                >
                                </b-form-checkbox>
                                <label class="d-flex" for="email">
                                    Email Notifications <Address></Address>
                                </label>
                            </b-col>
                        </b-row>
                        <b-form-input
                            id="email"
                            v-model="$v.email.$model"
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
                            id="emailConfirmation"
                            v-model="$v.emailConfirmation.$model"
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
                                ></b-form-checkbox>
                                <label class="d-flex" for="smsNumber"
                                    >SMS Notifications <Address></Address>
                                </label>
                            </b-col>
                        </b-row>
                        <b-form-input
                            id="smsNumber"
                            v-model="$v.smsNumber.$model"
                            class="d-flex"
                            type="text"
                            placeholder="Your phone number"
                            :state="isValid($v.smsNumber)"
                            :disabled="!isSMSNumberChecked"
                        >
                        </b-form-input>
                        <b-form-invalid-feedback :state="isValid($v.smsNumber)">
                            Valid sms number is required
                        </b-form-invalid-feedback>
                    </b-col>
                </b-row>
                <b-row v-if="!isEmailChecked && !isSMSNumberChecked">
                    <b-col class="font-weight-bold text-primary">
                        <font-awesome-icon
                            icon="exclamation-triangle"
                            aria-hidden="true"
                        ></font-awesome-icon>
                        You won't receive notifications from the Health Gateway.
                        You can update this from your Profile Page later.
                    </b-col>
                </b-row>
                <b-row class="mt-4">
                    <b-col>
                        <h2>Terms of Service</h2>
                    </b-col>
                </b-row>
                <b-row class="mb-3">
                    <b-col>
                        <HtmlTextAreaComponent :input="termsOfService" />
                    </b-col>
                </b-row>
                <b-row class="mb-3">
                    <b-col>
                        <b-form-checkbox
                            id="accept"
                            v-model="accepted"
                            class="accept"
                            :state="isValid($v.accepted)"
                        >
                            I agree to the terms of service above.
                        </b-form-checkbox>
                        <b-form-invalid-feedback :state="isValid($v.accepted)">
                            You must accept the terms of service.
                        </b-form-invalid-feedback>
                    </b-col>
                </b-row>
                <b-row class="mb-5">
                    <b-col class="justify-content-right">
                        <b-button
                            class="px-5 float-right"
                            type="submit"
                            size="lg"
                            variant="primary"
                            :class="{ disabled: !accepted }"
                            >Register</b-button
                        >
                    </b-col>
                </b-row>
            </b-form>
        </div>
    </b-container>
</template>

<script lang="ts">
import Vue from "vue";
import { Action, Getter } from "vuex-class";
import { Component, Prop, Ref } from "vue-property-decorator";
import {
    ILogger,
    IAuthenticationService,
    IBetaRequestService,
    IUserProfileService,
} from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import User from "@/models/user";
import {
    ValidationRule,
    email,
    helpers,
    required,
    requiredIf,
    sameAs,
} from "vuelidate/lib/validators";
import { RegistrationStatus } from "@/constants/registrationStatus";
import LoadingComponent from "@/components/loading.vue";
import HtmlTextAreaComponent from "@/components/htmlTextarea.vue";
import { WebClientConfiguration } from "@/models/configData";
import BetaRequest from "@/models/betaRequest";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faCheck } from "@fortawesome/free-solid-svg-icons";
library.add(faCheck);

@Component({
    components: {
        LoadingComponent,
        HtmlTextAreaComponent,
    },
})
export default class RegistrationView extends Vue {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    @Action("checkRegistration", { namespace: "user" })
    checkRegistration!: (params: { hdid: string }) => Promise<boolean>;

    @Ref("registrationForm") form!: HTMLFormElement;

    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;

    @Prop() inviteKey?: string;
    @Prop() inviteEmail?: string;

    private accepted: boolean = false;
    private email: string = "";
    private emailConfirmation: string = "";
    private smsNumber: string = "";

    private isEmailChecked: boolean = true;
    private isSMSNumberChecked: boolean = true;

    private oidcUser: any = {};
    private userProfileService!: IUserProfileService;
    private submitStatus: string = "";
    private loadingUserData: boolean = true;
    private loadingTermsOfService: boolean = true;
    private hasErrors: boolean = false;
    private errorMessage: string = "";

    private betaRequestService!: IBetaRequestService;
    private waitlistEdditable = true;
    private waitlistTempEmail: string = "";
    private waitlistEmail: string = "";
    private waitlistEmailConfirmation: string = "";

    private waitlistedSuccessfully = false;

    private termsOfService: string = "";

    private mounted() {
        this.betaRequestService = container.get(
            SERVICE_IDENTIFIER.BetaRequestService
        );

        if (
            this.webClientConfig.registrationStatus == RegistrationStatus.Open
        ) {
            this.email = "";
            this.emailConfirmation = "";
        } else {
            this.email = this.inviteEmail || "";
            this.emailConfirmation = this.email;
        }

        this.userProfileService = container.get(
            SERVICE_IDENTIFIER.UserProfileService
        );

        // Load the user name
        this.loadingUserData = true;
        var authenticationService: IAuthenticationService = container.get(
            SERVICE_IDENTIFIER.AuthenticationService
        );
        authenticationService
            .getOidcUserProfile()
            .then((oidcUser) => {
                if (oidcUser) {
                    this.oidcUser = oidcUser;

                    this.betaRequestService
                        .getRequest(this.oidcUser.hdid)
                        .then((betaRequest) => {
                            this.logger.debug(
                                `getOidcUserProfile result: ${JSON.stringify(
                                    betaRequest
                                )}`
                            );
                            if (betaRequest) {
                                this.email = betaRequest.emailAddress;
                                this.emailConfirmation = this.email;
                                this.waitlistTempEmail = this.email;
                                this.waitlistEdditable = false;
                            } else {
                                this.makeWaitlistEdditable();
                            }
                        })
                        .catch(() => {
                            this.hasErrors = true;
                        })
                        .finally(() => {
                            this.loadingUserData = false;
                        });
                } else {
                    this.loadingUserData = false;
                }
            })
            .catch(() => {
                this.hasErrors = true;
                this.loadingUserData = false;
            });

        this.loadTermsOfService();
    }

    validations() {
        const sms = helpers.regex("sms", /^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$/);
        return {
            smsNumber: {
                required: requiredIf(() => {
                    return this.isSMSNumberChecked;
                }),
                sms,
            },
            email: {
                required: requiredIf(() => {
                    return this.isEmailChecked;
                }),
                email,
            },
            emailConfirmation: {
                required: requiredIf(() => {
                    return this.isEmailChecked;
                }),
                sameAsEmail: sameAs("email"),
                email,
            },
            accepted: { isChecked: sameAs(() => true) },
        };
    }

    private get isLoading(): boolean {
        return this.loadingTermsOfService || this.loadingUserData;
    }

    private get fullName(): string {
        return this.oidcUser.given_name + " " + this.oidcUser.family_name;
    }
    private get isRegistrationClosed(): boolean {
        return (
            this.webClientConfig.registrationStatus == RegistrationStatus.Closed
        );
    }
    private get isRegistrationInviteOnly(): boolean {
        return (
            this.webClientConfig.registrationStatus ==
            RegistrationStatus.InviteOnly
        );
    }
    private get isPredefinedEmail() {
        if (
            this.webClientConfig.registrationStatus != RegistrationStatus.Open
        ) {
            return !!this.inviteEmail;
        }
        return false;
    }

    private loadTermsOfService(): void {
        this.loadingTermsOfService = true;
        this.userProfileService
            .getTermsOfService()
            .then((result) => {
                this.logger.debug(
                    `getTermsOfService result: ${JSON.stringify(result)}`
                );
                this.termsOfService = result.content;
            })
            .catch((err) => {
                this.logger.error(err);
                this.handleError("Please refresh your browser.");
            })
            .finally(() => {
                this.loadingTermsOfService = false;
            });
    }

    private isValid(param: any): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    private onSubmit(event: Event) {
        this.$v.$touch();
        if (this.$v.$invalid) {
            this.submitStatus = "ERROR";
        } else {
            this.submitStatus = "PENDING";
            if (this.smsNumber) {
                this.smsNumber = this.smsNumber.replace(/\D+/g, "");
            }
            this.loadingTermsOfService = true;
            this.userProfileService
                .createProfile({
                    profile: {
                        hdid: this.oidcUser.hdid,
                        acceptedTermsOfService: this.accepted,
                        email: this.email || "",
                        smsNumber: this.smsNumber || "",
                        preferences: {},
                    },
                    inviteCode: this.inviteKey || "",
                })
                .then((result) => {
                    this.logger.debug(
                        `Create Profile result: ${JSON.stringify(result)}`
                    );
                    this.checkRegistration({ hdid: this.oidcUser.hdid }).then(
                        (isRegistered: boolean) => {
                            if (isRegistered) {
                                this.$router.push({ path: "/timeline" });
                            } else {
                                this.hasErrors = true;
                            }
                        }
                    );
                })
                .catch((err) => {
                    this.handleError(err);
                })
                .finally(() => {
                    this.loadingTermsOfService = false;
                });
        }

        event.preventDefault();
    }

    private makeWaitlistEdditable(): void {
        this.waitlistEdditable = true;
        this.waitlistTempEmail = this.email || "";
    }

    private saveWaitlistEdit(): void {
        this.$v.$touch();
        this.loadingUserData = true;
        if (this.$v.email.$invalid || this.$v.emailConfirmation.$invalid) {
            this.submitStatus = "ERROR";
        } else {
            this.submitStatus = "PENDING";

            let newRequest: BetaRequest = {
                hdid: this.oidcUser.hdid,
                emailAddress: this.email,
            };

            this.betaRequestService
                .putRequest(newRequest)
                .then((result) => {
                    this.logger.debug(
                        `Save Beta Request Profile result: ${JSON.stringify(
                            result
                        )}`
                    );
                    this.waitlistEdditable = false;
                    this.waitlistEmailConfirmation = "";
                    this.waitlistTempEmail = this.email;
                    this.waitlistedSuccessfully = true;
                    this.hasErrors = false;
                    this.$v.$reset();
                })
                .catch((err) => {
                    this.hasErrors = true;
                    this.logger.error(`Error saving new beta request. ${err}`);
                })
                .finally(() => {
                    this.loadingUserData = false;
                });
        }
    }

    private cancelWaitlistEdit(): void {
        this.waitlistEdditable = false;
        this.email = this.waitlistTempEmail;
        this.waitlistEmailConfirmation = "";
        this.$v.$reset();
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

    private handleError(error: string): void {
        this.hasErrors = true;
        this.errorMessage = error;
        this.logger.error(error);
    }
}
</script>
