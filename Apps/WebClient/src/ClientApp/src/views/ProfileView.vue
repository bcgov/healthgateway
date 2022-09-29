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
import { instanceOfResultError, ResultError } from "@/models/errors";
import PatientData, { Address } from "@/models/patientData";
import User, { OidcUserInfo } from "@/models/user";
import UserProfile from "@/models/userProfile";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IUserProfileService } from "@/services/interfaces";

library.add(faExclamationTriangle);

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        BreadcrumbComponent,
        LoadingComponent,
        VerifySMSComponent,
    },
};

@Component(options)
export default class ProfileView extends Vue {
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

    @Action("updateUserEmail", { namespace: "user" })
    updateUserEmail!: ({
        emailAddress,
    }: {
        emailAddress: string;
    }) => Promise<void>;

    @Action("checkRegistration", { namespace: "user" })
    checkRegistration!: () => Promise<boolean>;

    @Action("closeUserAccount", { namespace: "user" })
    closeUserAccount!: () => Promise<void>;

    @Action("recoverUserAccount", { namespace: "user" })
    recoverUserAccount!: () => Promise<void>;

    @Action("updateSMSResendDateTime", { namespace: "user" })
    updateSMSResendDateTime!: ({ dateTime }: { dateTime: DateWrapper }) => void;

    @Action("retrievePatientData", { namespace: "user" })
    retrievePatientData!: () => Promise<void>;

    @Getter("oidcIsAuthenticated", { namespace: "auth" })
    oidcIsAuthenticated!: boolean;

    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Getter("oidcUserInfo", { namespace: "user" })
    oidcUserInfo!: OidcUserInfo | undefined;

    @Getter("patientData", { namespace: "user" })
    patientData!: PatientData;

    @Getter("userIsActive", { namespace: "user" })
    isActiveProfile!: boolean;

    @Ref("verifySMSModal")
    readonly verifySMSModal!: VerifySMSComponent;

    private isLoading = true;

    private showCheckEmailAlert = false;
    private emailVerified = false;
    private email = "";
    private isEmailEditable = false;
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
    private physicalAddress!: Address;
    private postalAddress!: Address;

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

    private get fullName(): string {
        if (this.oidcUserInfo === undefined) {
            return "";
        }
        return `${this.oidcUserInfo.given_name} ${this.oidcUserInfo.family_name}`;
    }

    private get phn(): string {
        return this.patientData.personalhealthnumber;
    }

    private get isEmptyEmail(): boolean {
        return (
            this.email === null || this.email === undefined || this.email === ""
        );
    }

    private get isUpdateAddressCombinedTextShown(): boolean {
        return (
            this.isSameAddress() &&
            this.patientData.physicalAddress !== null &&
            this.patientData.postalAddress !== null
        );
    }

    private get isUpdateAddressDifferentTextShown(): boolean {
        return !this.isSameAddress();
    }

    private get isAddAddressTextShown(): boolean {
        return (
            this.patientData.physicalAddress === null &&
            this.patientData.postalAddress === null
        );
    }

    private get isPhysicalAddressShown(): boolean {
        return (
            this.patientData.physicalAddress !== null && !this.isSameAddress()
        );
    }

    private get isPhysicalAddressSectionShown(): boolean {
        return !this.isSameAddress();
    }

    private get isPostalAddressShown(): boolean {
        return this.patientData.postalAddress != null;
    }

    private get postalAddressLabel(): string {
        if (
            !this.isSameAddress() ||
            (this.patientData.physicalAddress !== null &&
                this.patientData.postalAddress === null)
        ) {
            return "Mailing Address";
        }
        return "Address";
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

    private mounted(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );

        this.isLoading = true;
        let patientPromise = this.retrievePatientData();
        let userProfilePromise = this.userProfileService.getProfile(
            this.user.hdid
        );

        Promise.all([userProfilePromise, patientPromise])
            .then(([userProfile]) => {
                if (userProfile) {
                    // Load user profile
                    this.logger.verbose(
                        `User Profile: ${JSON.stringify(this.userProfile)}`
                    );
                    this.userProfile = userProfile;
                    this.loginDateTimes = this.userProfile.lastLoginDateTimes;
                    this.email = this.userProfile.email;
                    this.emailVerified = this.userProfile.isEmailVerified;
                    this.emailVerificationSent = this.emailVerified;
                    // Load user sms
                    this.smsNumber = this.userProfile.smsNumber;
                    this.smsVerified = this.userProfile.isSMSNumberVerified;
                }

                this.setAddresses();

                this.isLoading = false;
            })
            .catch((error: ResultError) => {
                this.logger.error(
                    `Error loading profile: ${error.resultMessage}`
                );
                if (instanceOfResultError(error) && error.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Retrieve,
                        source: ErrorSourceType.Profile,
                        traceId: undefined,
                    });
                }
                this.isLoading = false;
            });

        this.calculateTimeForDeletion();
        this.intervalHandler = window.setInterval(
            () => this.calculateTimeForDeletion(),
            1000
        );
    }

    private validations(): unknown {
        const sms = helpers.regex("sms", /^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$/);
        return {
            smsNumber: {
                required: requiredIf(
                    () => this.isSMSEditable && this.smsNumber !== ""
                ),
                newSMSNumber: not(sameAs("tempSMS")),
                sms,
            },
            smsVerificationCode: {
                required: requiredIf(
                    () =>
                        !this.smsVerified &&
                        this.smsNumber !== "" &&
                        !this.isSMSEditable
                ),
                minLength: minLength(6),
            },
            email: {
                required: requiredIf(
                    () => this.isEmailEditable && !this.isEmptyEmail
                ),
                newEmail: not(sameAs("tempEmail")),
                email,
            },
        };
    }

    private calculateTimeForDeletion(): void {
        if (this.isActiveProfile) {
            return undefined;
        }

        let endDate = new DateWrapper(this.user.closedDateTime);
        endDate = endDate.add({ hour: this.webClientConfig.hoursForDeletion });
        this.timeForDeletion = endDate.diff(new DateWrapper()).milliseconds;
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
        this.checkRegistration()
            .then(() => {
                this.smsVerified = this.user.verifiedSMS;
            })
            .catch((error) => {
                if (instanceOfResultError(error) && error.statusCode === 429) {
                    this.setTooManyRequestsWarning({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Retrieve,
                        source: ErrorSourceType.Profile,
                        traceId: undefined,
                    });
                }
            });
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
                this.$v.$reset();
                this.showCheckEmailAlert = !this.isEmptyEmail;
                this.checkRegistration().catch((error) => {
                    if (
                        instanceOfResultError(error) &&
                        error.statusCode === 429
                    ) {
                        this.setTooManyRequestsWarning({ key: "page" });
                    } else {
                        this.addError({
                            errorType: ErrorType.Retrieve,
                            source: ErrorSourceType.Profile,
                            traceId: undefined,
                        });
                    }
                });
            })
            .catch((err) => {
                this.logger.error(err);
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Update,
                        source: ErrorSourceType.Profile,
                        traceId: undefined,
                    });
                }
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

                if (this.smsNumber) {
                    this.verifySMS();
                }
                this.$v.$reset();

                this.checkRegistration().catch((error) => {
                    if (
                        instanceOfResultError(error) &&
                        error.statusCode === 429
                    ) {
                        this.setTooManyRequestsError({ key: "page" });
                    } else {
                        this.addError({
                            errorType: ErrorType.Retrieve,
                            source: ErrorSourceType.Profile,
                            traceId: undefined,
                        });
                    }
                });
            })
            .catch((error) => {
                if (instanceOfResultError(error) && error.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Update,
                        source: ErrorSourceType.Profile,
                        traceId: undefined,
                    });
                }
            });
    }

    private recoverAccount(): void {
        this.isLoading = true;
        this.recoverUserAccount()
            .then(() => this.logger.verbose("success!"))
            .catch((err: ResultError) => {
                this.logger.error(err.resultMessage);
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                } else {
                    this.addCustomError({
                        title:
                            "Unable to recover " +
                            ErrorSourceType.Profile.toLowerCase(),
                        source: ErrorSourceType.Profile,
                        traceId: undefined,
                    });
                }
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
            .catch((err: ResultError) => {
                this.logger.error(err.resultMessage);
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                } else {
                    this.addCustomError({
                        title:
                            "Unable to close " +
                            ErrorSourceType.Profile.toLowerCase(),
                        source: ErrorSourceType.Profile,
                        traceId: undefined,
                    });
                }
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private isSameAddress(): boolean {
        let result = false;
        if (this.postalAddress != null && this.physicalAddress != null) {
            const equals = (a: string[], b: string[]) =>
                a.length === b.length && a.every((v, i) => v === b[i]);

            const isStreetLineSame = equals(
                this.postalAddress.streetLines,
                this.physicalAddress.streetLines
            );

            const isCitySame =
                this.postalAddress.city === this.physicalAddress.city;

            const isStateSame =
                this.postalAddress.state === this.physicalAddress.state;

            const isPostalCodeSame =
                this.postalAddress.postalCode ===
                this.physicalAddress.postalCode;

            result =
                isStreetLineSame &&
                isCitySame &&
                isStateSame &&
                isPostalCodeSame;
        } else {
            result = this.postalAddress == null && this.physicalAddress == null;
        }

        this.logger.debug(
            `Physical Address and Postal Address same: ${result}`
        );

        return result;
    }

    private setAddresses(): void {
        // Physical Address
        this.physicalAddress = this.getNewAddress(
            this.patientData.physicalAddress
        );
        this.logger.debug(
            `Physical Address: ${JSON.stringify(this.physicalAddress)}`
        );

        // Postal Address
        this.postalAddress = this.getNewAddress(this.patientData.postalAddress);
        this.logger.debug(
            `Postal Address: ${JSON.stringify(this.postalAddress)}`
        );
    }

    private getNewAddress(address: Address | undefined): Address {
        let newAddress = new Address();
        newAddress.streetLines = address?.streetLines ?? [];
        newAddress.city = address?.city ?? "";
        newAddress.postalCode = address?.postalCode ?? "";
        newAddress.state = address?.state ?? "";
        return newAddress;
    }
}
</script>

<template>
    <div>
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
                                                v-if="
                                                    !emailVerified &&
                                                    !isEmailEditable &&
                                                    email
                                                "
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
                                                v-if="
                                                    !smsVerified &&
                                                    !isSMSEditable &&
                                                    smsNumber
                                                "
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
                                <label
                                    for="postal-address-section"
                                    class="hg-label"
                                    data-testid="postal-address-label"
                                >
                                    {{ postalAddressLabel }}
                                </label>
                                <div id="postal-address-section">
                                    <div
                                        v-if="!isPostalAddressShown"
                                        id="no-postal-address-text-div"
                                    >
                                        <em
                                            data-testid="no-postal-address-text"
                                        >
                                            No address on record
                                        </em>
                                    </div>
                                    <div
                                        v-if="isPostalAddressShown"
                                        id="postal-address-div"
                                        data-testid="postal-address-div"
                                    >
                                        <b-row
                                            v-for="(
                                                item, index
                                            ) in postalAddress.streetLines"
                                            :key="index"
                                        >
                                            <b-col>{{ item }}</b-col>
                                        </b-row>
                                        <b-row>
                                            <b-col
                                                >{{ postalAddress.city }},
                                                {{ postalAddress.state }},
                                                {{ postalAddress.postalCode }}
                                            </b-col>
                                        </b-row>
                                    </div>
                                </div>
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="isPhysicalAddressSectionShown"
                            class="mb-3"
                        >
                            <b-col>
                                <label
                                    for="physical-address-section"
                                    class="hg-label"
                                    data-testid="physical-address-label"
                                    >Physical Address
                                </label>
                                <div
                                    id="physical-address-section"
                                    data-testid="physical-address-section"
                                >
                                    <div
                                        v-if="!isPhysicalAddressShown"
                                        id="no-physical-address-text-div"
                                    >
                                        <em
                                            data-testid="no-physical-address-text"
                                        >
                                            No address on record
                                        </em>
                                    </div>
                                    <div
                                        v-if="isPhysicalAddressShown"
                                        id="physical-address-div"
                                        data-testid="physical-address-div"
                                    >
                                        <b-row
                                            v-for="(
                                                item, index
                                            ) in physicalAddress.streetLines"
                                            :key="index"
                                        >
                                            <b-col>{{ item }}</b-col>
                                        </b-row>
                                        <b-row>
                                            <b-col
                                                >{{ physicalAddress.city }},
                                                {{ physicalAddress.state }},
                                                {{ physicalAddress.postalCode }}
                                            </b-col>
                                        </b-row>
                                    </div>
                                </div>
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="isUpdateAddressCombinedTextShown"
                            class="mb-3"
                        >
                            <b-col>
                                If this address is incorrect, update it
                                <a
                                    href="https://www.addresschange.gov.bc.ca/"
                                    target="_blank"
                                    rel="noopener"
                                    >here</a
                                >
                                <span>.</span>
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="isUpdateAddressDifferentTextShown"
                            class="mb-3"
                        >
                            <b-col>
                                If either of these addresses is incorrect,
                                update them
                                <a
                                    href="https://www.addresschange.gov.bc.ca/"
                                    target="_blank"
                                    rel="noopener"
                                    >here</a
                                >
                                <span>.</span>
                            </b-col>
                        </b-row>
                        <b-row v-if="isAddAddressTextShown" class="mb-3">
                            <b-col>
                                To add an address, visit
                                <a
                                    href="https://www.addresschange.gov.bc.ca/"
                                    target="_blank"
                                    rel="noopener"
                                    >this page</a
                                >
                                <span>.</span>
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
