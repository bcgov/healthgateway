<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faExclamationTriangle } from "@fortawesome/free-solid-svg-icons";
import { Duration } from "luxon";
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";
import {
    email,
    helpers,
    minLength,
    not,
    requiredIf,
    sameAs,
} from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/LoadingComponent.vue";
import VerifySMSComponent from "@/components/modal/VerifySMSComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import BreadcrumbItem from "@/models/breadcrumbItem";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import PatientData from "@/models/patientData";
import User, { OidcUserProfile } from "@/models/user";
import UserProfile from "@/models/userProfile";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import {
    IAuthenticationService,
    ILogger,
    IUserProfileService,
} from "@/services/interfaces";

library.add(faExclamationTriangle);

const userNamespace = "user";
const authNamespace = "auth";

@Component({
    components: {
        BreadcrumbComponent,
        LoadingComponent,
        VerifySMSComponent,
    },
})
export default class ProfileView extends Vue {
    @Getter("oidcIsAuthenticated", {
        namespace: authNamespace,
    })
    oidcIsAuthenticated!: boolean;

    @Action("updateUserEmail", { namespace: userNamespace })
    updateUserEmail!: ({
        emailAddress,
    }: {
        emailAddress: string;
    }) => Promise<void>;

    @Action("checkRegistration", { namespace: userNamespace })
    checkRegistration!: () => Promise<boolean>;

    @Action("closeUserAccount", { namespace: userNamespace })
    closeUserAccount!: () => Promise<void>;

    @Action("recoverUserAccount", { namespace: userNamespace })
    recoverUserAccount!: () => Promise<void>;

    @Action("updateSMSResendDateTime", { namespace: userNamespace })
    updateSMSResendDateTime!: ({ dateTime }: { dateTime: DateWrapper }) => void;

    @Getter("user", { namespace: userNamespace }) user!: User;

    @Getter("userIsActive", { namespace: userNamespace })
    isActiveProfile!: boolean;

    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;

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

    @Action("retrievePatientData", { namespace: "user" })
    retrievePatientData!: () => Promise<void>;

    @Getter("patientData", { namespace: "user" })
    patientData!: PatientData;

    @Ref("verifySMSModal")
    readonly verifySMSModal!: VerifySMSComponent;

    private isLoading = true;

    private showCheckEmailAlert = false;
    private emailVerified = false;
    private email = "";
    private isEmailEditable = false;
    private oidcUser!: OidcUserProfile;
    private emailVerificationSent = false;

    private smsVerified = false;
    private smsNumber = "";
    private isSMSEditable = false;
    private tempSMS = "";
    private invalidSMSVerificationCode = false;

    private tempEmail = "";
    private submitStatus = "";
    private logger!: ILogger;
    private userProfileService!: IUserProfileService;
    private userProfile!: UserProfile;

    private loginDateTimes: string[] | undefined = [];

    private showCloseWarning = false;

    private timeForDeletion = -1;

    private intervalHandler = 0;

    private breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Profile",
            to: "/profile",
            active: true,
            dataTestId: "breadcrumb-profile",
        },
    ];

    private get isEmptyEmail(): boolean {
        return (
            this.email === null || this.email === undefined || this.email === ""
        );
    }

    private get formattedLoginDateTimes(): string[] {
        let items: string[] = [];
        if (
            this.loginDateTimes !== undefined &&
            this.loginDateTimes.length > 0
        ) {
            this.loginDateTimes.forEach((item) =>
                items.push(
                    new DateWrapper(item, { isUtc: true }).format(
                        "yyyy-MMM-dd, t"
                    )
                )
            );
        }
        return items;
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );

        // Load the user name and current email
        let authenticationService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );

        this.isLoading = true;
        var patientPromise = this.retrievePatientData();
        var oidcUserPromise = authenticationService.getOidcUserProfile();
        var userProfilePromise = this.userProfileService.getProfile(
            this.user.hdid
        );

        Promise.all([oidcUserPromise, userProfilePromise, patientPromise])
            .then((results) => {
                // Load oidc user details
                if (results[0]) {
                    this.oidcUser = results[0];
                }

                if (results[1]) {
                    // Load user profile
                    this.logger.verbose(
                        `User Profile: ${JSON.stringify(this.userProfile)}`
                    );
                    this.userProfile = results[1];
                    this.loginDateTimes = this.userProfile.lastLoginDateTimes;
                    this.email = this.userProfile.email;
                    this.emailVerified = this.userProfile.isEmailVerified;
                    this.emailVerificationSent = this.emailVerified;
                    // Load user sms
                    this.smsNumber = this.userProfile.smsNumber;
                    this.smsVerified = this.userProfile.isSMSNumberVerified;
                }

                this.checkToVerifyPhone();
                this.checkToVerifyEmail();

                this.isLoading = false;
            })
            .catch((err) => {
                this.logger.error(`Error loading profile: ${err}`);
                this.addError({
                    errorType: ErrorType.Retrieve,
                    source: ErrorSourceType.Profile,
                    traceId: undefined,
                });
                this.isLoading = false;
            });

        this.calculateTimeForDeletion();
        this.intervalHandler = window.setInterval(() => {
            this.calculateTimeForDeletion();
        }, 1000);
    }

    private checkToVerifyPhone() {
        let toVerifyPhone = this.$route.query.toVerifyPhone;
        this.logger.debug(
            `toVerifyPhone: ${toVerifyPhone}; smsVerified: ${this.smsVerified}`
        );
        if (toVerifyPhone === "true" && !this.smsVerified) {
            this.logger.debug(`display Verifying SMS popup`);
            this.verifySMS();
        }
    }

    private checkToVerifyEmail() {
        let toVerifyEmail = this.$route.query.toVerifyEmail;
        this.logger.debug(
            `toVerifyEmail: ${toVerifyEmail}; emailVerified: ${this.emailVerified}`
        );
        if (
            toVerifyEmail === "true" &&
            !this.emailVerified &&
            !this.isEmptyEmail
        ) {
            this.logger.debug(`display Verification Email`);
            this.showCheckEmailAlert = true;
        }
    }

    private validations() {
        const sms = helpers.regex("sms", /^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$/);
        return {
            smsNumber: {
                required: requiredIf(() => {
                    return this.isSMSEditable && this.smsNumber !== "";
                }),
                newSMSNumber: not(sameAs("tempSMS")),
                sms,
            },
            smsVerificationCode: {
                required: requiredIf(() => {
                    return (
                        !this.smsVerified &&
                        this.smsNumber !== "" &&
                        !this.isSMSEditable
                    );
                }),
                minLength: minLength(6),
            },
            email: {
                required: requiredIf(() => {
                    return this.isEmailEditable && !this.isEmptyEmail;
                }),
                newEmail: not(sameAs("tempEmail")),
                email,
            },
        };
    }

    private get fullName(): string {
        return this.oidcUser.given_name + " " + this.oidcUser.family_name;
    }

    private calculateTimeForDeletion(): void {
        if (this.isActiveProfile) {
            return undefined;
        }

        let endDate = new DateWrapper(this.user.closedDateTime);
        endDate = endDate.add({ hour: this.webClientConfig.hoursForDeletion });
        this.timeForDeletion = endDate.diff(new DateWrapper()).milliseconds;
    }

    private get timeForDeletionString(): string {
        if (this.isActiveProfile) {
            return "";
        }

        if (this.timeForDeletion < 0) {
            return "Your account will be closed imminently";
        }

        let duration = Duration.fromMillis(this.timeForDeletion);
        let timeRemaining = duration.as("days");
        if (timeRemaining > 1) {
            return this.pluralize(timeRemaining, "day");
        }
        timeRemaining = duration.as("hours");
        if (timeRemaining > 1) {
            return this.pluralize(timeRemaining, "hour");
        }
        timeRemaining = duration.as("minutes");
        if (timeRemaining > 1) {
            return this.pluralize(timeRemaining, "minute");
        }

        timeRemaining = duration.as("seconds");
        return this.pluralize(timeRemaining, "second");
    }

    private pluralize(count: number, message: string): string {
        let roundCount = Math.floor(count);
        return (
            roundCount.toString() + " " + message + (roundCount > 1 ? "s" : "")
        );
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    private makeEmailEditable(): void {
        this.isEmailEditable = true;
        this.tempEmail = this.email || "";
    }

    private makeSMSEditable(): void {
        this.isSMSEditable = true;
        this.tempSMS = this.smsNumber || "";
    }

    private cancelEmailEdit(): void {
        this.isEmailEditable = false;
        this.email = this.tempEmail;
        this.tempEmail = "";
        this.$v.$reset();
    }

    private cancelSMSEdit(): void {
        this.isSMSEditable = false;
        this.smsNumber = this.tempSMS;
        this.tempSMS = "";
        this.$v.$reset();
    }

    private saveEmailEdit(): void {
        this.$v.$touch();
        if (this.$v.email.$invalid) {
            this.submitStatus = "ERROR";
        } else {
            this.submitStatus = "PENDING";
            this.logger.debug(`saveEmailEdit: ${JSON.stringify(this.email)}`);
            this.sendUserEmailUpdate();
        }
    }

    private saveSMSEdit(): void {
        this.$v.$touch();
        if (this.$v.smsNumber.$invalid) {
            this.submitStatus = "ERROR";
        } else {
            this.submitStatus = "PENDING";
            if (this.smsNumber) {
                this.smsNumber = this.smsNumber.replace(/\D+/g, "");
            }
            this.updateSMS();
        }
    }

    private verifySMS(): void {
        this.verifySMSModal.showModal();
    }

    private onVerifySMSSubmit(): void {
        this.checkRegistration();
        this.smsVerified = true;
    }
    private sendUserEmailUpdate(): void {
        this.isLoading = true;
        this.updateUserEmail({
            emailAddress: this.email,
        })
            .then(() => {
                this.logger.verbose("success!");
                this.isEmailEditable = false;
                this.emailVerified = false;
                this.emailVerificationSent = true;
                this.tempEmail = "";
                this.checkRegistration();
                this.$v.$reset();
                this.showCheckEmailAlert = !this.isEmptyEmail;
            })
            .catch((err) => {
                this.logger.error(err);
                this.addError({
                    errorType: ErrorType.Update,
                    source: ErrorSourceType.Profile,
                    traceId: undefined,
                });
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private updateSMS(): void {
        this.logger.debug(
            `Updating ${this.smsNumber ? this.smsNumber : "sms number..."}`
        );
        // Reset timer when user submits their SMS number
        this.updateSMSResendDateTime({
            dateTime: new DateWrapper(),
        });
        // Send update to backend
        this.userProfileService
            .updateSMSNumber(this.user.hdid, this.smsNumber)
            .then(() => {
                this.isSMSEditable = false;
                this.smsVerified = false;
                this.tempSMS = "";
                this.checkRegistration();
                if (this.smsNumber) {
                    this.verifySMS();
                }
                this.$v.$reset();
            });
    }

    private recoverAccount(): void {
        this.isLoading = true;
        this.recoverUserAccount()
            .then(() => {
                this.logger.verbose("success!");
            })
            .catch((err) => {
                this.logger.error(err);
                this.addCustomError({
                    title:
                        "Unable to recover " +
                        ErrorSourceType.Profile.toLowerCase(),
                    source: ErrorSourceType.Profile,
                    traceId: undefined,
                });
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private showCloseWarningBtn(): void {
        this.showCloseWarning = true;
    }

    private cancelClose(): void {
        this.showCloseWarning = false;
    }

    private closeAccount(): void {
        this.isLoading = true;
        this.closeUserAccount()
            .then(() => {
                this.logger.verbose("success!");
                this.showCloseWarning = false;
            })
            .catch((err) => {
                this.logger.error(err);
                this.addCustomError({
                    title:
                        "Unable to close " +
                        ErrorSourceType.Profile.toLowerCase(),
                    source: ErrorSourceType.Profile,
                    traceId: undefined,
                });
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private get phn(): string {
        return this.patientData.personalhealthnumber;
    }
}
</script>

<template>
    <div class="m-3 m-md-4 flex-grow-1 d-flex flex-column">
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="isLoading" />
        <b-alert
            :show="showCheckEmailAlert"
            dismissible
            variant="info"
            class="no-print"
            data-testid="verifyEmailTxt"
            @dismissed="showCheckEmailAlert = false"
        >
            <h4>Please check your email</h4>
            <span>
                Please check your email for an email verification link. If you
                didn't receive one, please check your junk mail.
            </span>
        </b-alert>
        <page-title title="Profile" />
        <div v-if="!isLoading">
            <div v-if="isActiveProfile">
                <b-row class="mb-3">
                    <b-col>
                        <label for="profileNames" class="hg-label"
                            >Full Name</label
                        >
                        <div id="profileNames">
                            {{ fullName }}
                        </div>
                    </b-col>
                </b-row>
                <b-row class="mb-3">
                    <b-col>
                        <label for="PHN" class="hg-label"
                            >Personal Health Number</label
                        >
                        <div id="PHN" data-testid="PHN">
                            {{ phn }}
                        </div>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <b-row>
                            <b-col>
                                <b-form-group
                                    :state="
                                        isValid($v.email) || !isEmailEditable
                                            ? null
                                            : false
                                    "
                                >
                                    <label for="email" class="hg-label">
                                        Email Address
                                    </label>
                                    <b-link
                                        v-if="!isEmailEditable"
                                        id="editEmail"
                                        data-testid="editEmailBtn"
                                        class="ml-3"
                                        variant="link"
                                        @click="makeEmailEditable()"
                                        >Edit</b-link
                                    >
                                    <div class="form-inline mb-1">
                                        <b-row>
                                            <b-col>
                                                <b-form-input
                                                    id="email"
                                                    v-model="$v.email.$model"
                                                    data-testid="emailInput"
                                                    type="email"
                                                    :placeholder="
                                                        isEmailEditable
                                                            ? 'Your email address'
                                                            : 'Empty'
                                                    "
                                                    :disabled="!isEmailEditable"
                                                    :state="
                                                        isValid($v.email) ||
                                                        !isEmailEditable
                                                            ? null
                                                            : false
                                                    "
                                                />
                                            </b-col>
                                            <b-col
                                                cols="12"
                                                md="auto"
                                                class="pl-md-0 pl-3"
                                            >
                                                <hg-button
                                                    id="resendEmail"
                                                    data-testid="resendEmailBtn"
                                                    variant="secondary"
                                                    class="mt-md-0 mt-2"
                                                    :disabled="
                                                        emailVerificationSent
                                                    "
                                                    @click="
                                                        sendUserEmailUpdate()
                                                    "
                                                >
                                                    Resend Verification
                                                </hg-button>
                                            </b-col>
                                        </b-row>
                                    </div>
                                    <b-form-invalid-feedback
                                        :state="$v.email.email"
                                    >
                                        Valid email is required
                                    </b-form-invalid-feedback>
                                    <b-form-invalid-feedback
                                        :state="$v.email.newEmail"
                                        data-testid="emailInvalidNewEqualsOld"
                                    >
                                        New email must be different from the
                                        previous one
                                    </b-form-invalid-feedback>
                                    <div
                                        v-if="!isEmailEditable"
                                        id="emailStatus"
                                        data-testid="emailStatus"
                                    >
                                        <status-label
                                            v-if="emailVerified"
                                            status="Verified"
                                            variant="success"
                                            data-testid="emailStatusVerified"
                                        />
                                        <status-label
                                            v-else-if="
                                                email == null || email === ''
                                            "
                                            status="Opted Out"
                                            data-testid="emailStatusOptedOut"
                                        />
                                        <status-label
                                            v-else
                                            status="Not Verified"
                                            variant="danger"
                                            data-testid="emailStatusNotVerified"
                                        />
                                    </div>
                                </b-form-group>
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="!email && tempEmail"
                            class="mb-3"
                            data-testid="emailOptOutMessage"
                        >
                            <b-col
                                class="font-weight-bold text-primary text-center"
                            >
                                <hg-icon
                                    icon="exclamation-triangle"
                                    size="medium"
                                    aria-hidden="true"
                                    class="mr-2"
                                />
                                <span>
                                    Removing your email address will disable
                                    future email communications from the Health
                                    Gateway.
                                </span>
                            </b-col>
                        </b-row>
                        <b-row v-if="isEmailEditable" class="mb-3">
                            <b-col>
                                <hg-button
                                    id="editEmailCancelBtn"
                                    data-testid="editEmailCancelBtn"
                                    variant="secondary"
                                    size="small"
                                    class="mr-2"
                                    @click="cancelEmailEdit()"
                                >
                                    Cancel
                                </hg-button>
                                <hg-button
                                    id="editSMSSaveBtn"
                                    data-testid="editEmailSaveBtn"
                                    variant="primary"
                                    size="small"
                                    class="mx-2"
                                    :disabled="
                                        tempEmail === email ||
                                        !isValid($v.email)
                                    "
                                    @click="saveEmailEdit($event)"
                                >
                                    Save
                                </hg-button>
                            </b-col>
                        </b-row>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <b-row>
                            <b-col>
                                <b-form-group
                                    :state="
                                        isValid($v.smsNumber) || !isSMSEditable
                                            ? null
                                            : false
                                    "
                                >
                                    <label for="smsNumber" class="hg-label">
                                        Cell Number (SMS notifications)
                                    </label>
                                    <b-link
                                        v-if="!isSMSEditable"
                                        id="editSMS"
                                        data-testid="editSMSBtn"
                                        class="ml-3"
                                        variant="link"
                                        @click="makeSMSEditable()"
                                        >Edit</b-link
                                    >
                                    <div class="form-inline mb-1">
                                        <b-row>
                                            <b-col>
                                                <b-form-input
                                                    id="smsNumber"
                                                    v-model="
                                                        $v.smsNumber.$model
                                                    "
                                                    v-mask="'(###) ###-####'"
                                                    type="tel"
                                                    data-testid="smsNumberInput"
                                                    :placeholder="
                                                        isSMSEditable
                                                            ? 'Your phone number'
                                                            : 'Empty'
                                                    "
                                                    :disabled="!isSMSEditable"
                                                    :state="
                                                        isValid($v.smsNumber) ||
                                                        !isSMSEditable
                                                            ? null
                                                            : false
                                                    "
                                                />
                                            </b-col>
                                            <b-col
                                                cols="12"
                                                md="auto"
                                                class="pl-md-0 pl-3"
                                            >
                                                <hg-button
                                                    id="verifySMS"
                                                    variant="secondary"
                                                    data-testid="verifySMSBtn"
                                                    class="mt-md-0 mt-2"
                                                    @click="verifySMS()"
                                                >
                                                    Verify
                                                </hg-button>
                                            </b-col>
                                        </b-row>
                                    </div>
                                    <b-form-invalid-feedback
                                        :state="$v.smsNumber.sms"
                                    >
                                        Valid SMS number is required
                                    </b-form-invalid-feedback>
                                    <b-form-invalid-feedback
                                        data-testid="smsInvalidNewEqualsOld"
                                        :state="$v.smsNumber.newSMSNumber"
                                    >
                                        New SMS number must be different from
                                        the previous one
                                    </b-form-invalid-feedback>
                                    <div
                                        v-if="!isSMSEditable"
                                        id="smsStatus"
                                        data-testid="smsStatus"
                                    >
                                        <status-label
                                            v-if="smsVerified"
                                            status="Verified"
                                            variant="success"
                                            data-testid="smsStatusVerified"
                                        />
                                        <status-label
                                            v-else-if="
                                                smsNumber == null ||
                                                smsNumber === ''
                                            "
                                            status="Opted Out"
                                            data-testid="smsStatusOptedOut"
                                        />
                                        <status-label
                                            v-else
                                            status="Not Verified"
                                            variant="danger"
                                            data-testid="smsStatusNotVerified"
                                        />
                                    </div>
                                </b-form-group>
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="!smsNumber && tempSMS"
                            data-testid="smsOptOutMessage"
                            class="mb-3"
                        >
                            <b-col
                                class="font-weight-bold text-primary text-center"
                            >
                                <hg-icon
                                    icon="exclamation-triangle"
                                    size="medium"
                                    aria-hidden="true"
                                    class="mr-2"
                                />
                                <span>
                                    Removing your phone number will disable
                                    future SMS communications from the Health
                                    Gateway.
                                </span>
                            </b-col>
                        </b-row>
                        <b-row v-if="isSMSEditable" class="mb-3">
                            <b-col>
                                <hg-button
                                    id="cancelBtn"
                                    data-testid="cancelSMSEditBtn"
                                    variant="secondary"
                                    size="small"
                                    class="mr-2"
                                    @click="cancelSMSEdit()"
                                >
                                    Cancel
                                </hg-button>
                                <hg-button
                                    id="saveBtn"
                                    data-testid="saveSMSEditBtn"
                                    variant="primary"
                                    size="small"
                                    class="mx-2"
                                    :disabled="tempSMS === smsNumber"
                                    @click="saveSMSEdit()"
                                >
                                    Save
                                </hg-button>
                            </b-col>
                        </b-row>
                        <b-row class="mb-3">
                            <b-col>
                                <label for="lastLoginDate" class="hg-label"
                                    >Login History</label
                                >
                                <div id="lastLoginDate">
                                    <ul>
                                        <li
                                            v-for="(
                                                item, index
                                            ) in formattedLoginDateTimes"
                                            :key="index"
                                            data-testid="lastLoginDateItem"
                                        >
                                            {{ item }}
                                        </li>
                                    </ul>
                                </div>
                            </b-col>
                        </b-row>
                    </b-col>
                </b-row>
            </div>
            <div v-else>
                <b-row class="mb-3">
                    <b-col>
                        <hg-icon
                            icon="exclamation-triangle"
                            size="medium"
                            aria-hidden="true"
                            class="text-danger mr-2"
                        />
                        <label for="deletionWarning" class="hg-label">
                            Account marked for removal
                        </label>
                        <div id="deletionWarning">
                            Your account has been deactivated. If you wish to
                            recover your account click on the "Recover Account"
                            button before the time expires.
                        </div>
                    </b-col>
                </b-row>
                <b-row class="mb-3">
                    <b-col>
                        <label class="hg-label"
                            >Time remaining for deletion:
                        </label>
                        {{ timeForDeletionString }}
                    </b-col>
                </b-row>
                <b-row class="mb-3">
                    <b-col>
                        <hg-button
                            id="recoverAccountCancelBtn"
                            data-testid="recoverAccountCancelBtn"
                            class="mx-auto"
                            variant="primary"
                            @click="recoverAccount()"
                            >Recover Account
                        </hg-button>
                    </b-col>
                </b-row>
            </div>
            <b-row v-if="isActiveProfile" class="mb-3">
                <b-col>
                    <label class="hg-label">Manage Account</label>
                    <div>
                        <hg-button
                            v-if="!showCloseWarning"
                            id="recoverAccountShowCloseWarningBtn"
                            data-testid="recoverAccountShowCloseWarningBtn"
                            class="p-0 pt-2"
                            variant="link-danger"
                            @click="showCloseWarningBtn()"
                            >Delete My Account
                        </hg-button>
                        <b-row v-if="showCloseWarning" class="mb-3">
                            <b-col
                                class="font-weight-bold text-danger text-center"
                            >
                                <hr />
                                <hg-icon
                                    icon="exclamation-triangle"
                                    size="medium"
                                    aria-hidden="true"
                                    class="mr-2"
                                />
                                <span
                                    >Your account will be marked for removal,
                                    preventing you from accessing your
                                    information on the Health Gateway. After a
                                    set period of time it will be removed
                                    permanently.</span
                                >
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="showCloseWarning"
                            class="mb-3 justify-content-end"
                        >
                            <b-col class="text-right">
                                <hg-button
                                    id="closeAccountCancelBtn"
                                    data-testid="closeAccountCancelBtn"
                                    variant="secondary"
                                    @click="cancelClose()"
                                    >Cancel
                                </hg-button>
                                <hg-button
                                    id="closeAccountBtn"
                                    data-testid="closeAccountBtn"
                                    class="mx-2"
                                    variant="danger"
                                    @click="closeAccount()"
                                    >Delete Account
                                </hg-button>
                            </b-col>
                        </b-row>
                    </div>
                </b-col>
            </b-row>
        </div>
        <div v-else>
            <b-row class="mb-3">
                <b-col>
                    <content-placeholders>
                        <content-placeholders-heading />
                        <content-placeholders-text :lines="1" />
                        <content-placeholders-heading />
                        <content-placeholders-text :lines="3" />
                    </content-placeholders>
                </b-col>
            </b-row>
        </div>
        <VerifySMSComponent
            ref="verifySMSModal"
            :sms-number="smsNumber"
            @submit="onVerifySMSSubmit"
        />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

input {
    width: 320px !important;
    max-width: 320px !important;
}
</style>
