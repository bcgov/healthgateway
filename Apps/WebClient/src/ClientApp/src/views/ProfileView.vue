<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faExclamationTriangle } from "@fortawesome/free-solid-svg-icons";
import { BaseValidation, useVuelidate } from "@vuelidate/core";
import {
    email as emailValidator,
    helpers,
    not,
    requiredIf,
    sameAs,
} from "@vuelidate/validators";
import { Duration, DurationUnit } from "luxon";
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import LoadingComponent from "@/components/LoadingComponent.vue";
import VerifySMSComponent from "@/components/modal/VerifySMSComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import ValidationRegEx from "@/constants/validationRegEx";
import BreadcrumbItem from "@/models/breadcrumbItem";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import { isTooManyRequestsError, ResultError } from "@/models/errors";
import Patient from "@/models/patient";
import User, { OidcUserInfo } from "@/models/user";
import UserProfile from "@/models/userProfile";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IUserProfileService } from "@/services/interfaces";
import PhoneUtil from "@/utility/phoneUtil";

library.add(faExclamationTriangle);

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Profile",
        to: "/profile",
        active: true,
        dataTestId: "breadcrumb-profile",
    },
];

const validPhoneNumberFormat = (rawInputSmsNumber: string) => {
    if (!rawInputSmsNumber) {
        return true;
    }
    if (!ValidationRegEx.PhoneNumberMasked.test(rawInputSmsNumber)) {
        return false;
    }
    const phoneNumber = PhoneUtil.stripPhoneMask(rawInputSmsNumber);
    return userProfileService.isPhoneNumberValid(phoneNumber);
};

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const userProfileService = container.get<IUserProfileService>(
    SERVICE_IDENTIFIER.UserProfileService
);
const store = useStore();

const isLoading = ref(true);
const showCheckEmailAlert = ref(false);
const emailVerified = ref(false);
const email = ref("");
const isEmailEditable = ref(false);
const emailVerificationSent = ref(false);
const smsVerified = ref(false);
const smsNumber = ref("");
const isSMSEditable = ref(false);
const tempSMS = ref("");
const tempEmail = ref("");
const submitStatus = ref("");
const userProfile = ref<UserProfile>();
const loginDateTimes = ref<string[]>([]);
const showCloseWarning = ref(false);
const timeForDeletion = ref(-1);
const intervalHandler = ref(0);

const verifySMSModal = ref<InstanceType<typeof VerifySMSComponent>>();

const webClientConfig = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);
const user = computed<User>(() => store.getters["user/user"]);
const oidcUserInfo = computed<OidcUserInfo | undefined>(
    () => store.getters["user/oidcUserInfo"]
);
const patient = computed<Patient>(() => store.getters["user/patient"]);
const isActiveProfile = computed<boolean>(
    () => store.getters["user/userIsActive"]
);

const fullName = computed(() =>
    oidcUserInfo.value
        ? `${oidcUserInfo.value.given_name} ${oidcUserInfo.value.family_name}`
        : ""
);
const physicalAddress = computed(() => patient.value.physicalAddress);
const postalAddress = computed(() => patient.value.postalAddress);
const hasAddress = computed(
    () => Boolean(physicalAddress.value) || Boolean(postalAddress.value)
);
const isSameAddress = computed(() => {
    if (physicalAddress.value && postalAddress.value) {
        const arrayEqual = (a: string[], b: string[]) =>
            a.length === b.length && a.every((v, i) => v === b[i]);

        const streetLinesMatch = arrayEqual(
            postalAddress.value.streetLines,
            physicalAddress.value.streetLines
        );
        const cityMatches =
            postalAddress.value.city === physicalAddress.value?.city;
        const stateMatches =
            postalAddress.value.state === physicalAddress.value?.state;
        const postalCodeMatches =
            postalAddress.value.postalCode === physicalAddress.value.postalCode;

        return (
            streetLinesMatch && cityMatches && stateMatches && postalCodeMatches
        );
    }

    return !hasAddress.value;
});
const postalAddressLabel = computed(() =>
    !isSameAddress.value || (physicalAddress.value && !postalAddress.value)
        ? "Mailing Address"
        : "Address"
);
const timeForDeletionString = computed(() => {
    if (isActiveProfile.value) {
        return "";
    }

    const duration = Duration.fromMillis(timeForDeletion.value);
    const units: DurationUnit[] = ["day", "hour", "minute", "second"];
    for (const unit of units) {
        const amount = Math.floor(duration.as(unit));
        if (amount > 1) {
            return `${amount} ${unit}s`;
        }
    }

    return "Your account will be closed imminently";
});
const formattedLoginDateTimes = computed(() =>
    loginDateTimes.value.map((time) =>
        new DateWrapper(time, { isUtc: true }).format("yyyy-MMM-dd, t")
    )
);
const validations = computed(() => ({
    smsNumber: {
        required: requiredIf(
            () => isSMSEditable.value && Boolean(smsNumber.value)
        ),
        newSMSNumber: not(sameAs("tempSMS")),
        sms: helpers.withAsync(validPhoneNumberFormat),
    },
    email: {
        required: requiredIf(
            () => isEmailEditable.value && Boolean(email.value)
        ),
        newEmail: not(sameAs("tempEmail")),
        email: emailValidator,
    },
}));

const v$ = useVuelidate(validations, { smsNumber, email });

function addError(
    errorType: ErrorType,
    source: ErrorSourceType,
    traceId: string | undefined
): void {
    store.dispatch("errorBanner/addError", { errorType, source, traceId });
}

function addCustomError(
    title: string,
    source: ErrorSourceType,
    traceId: string | undefined
): void {
    store.dispatch("errorBanner/addCustomError", { title, source, traceId });
}

function setTooManyRequestsError(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsError", { key });
}

function setTooManyRequestsWarning(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsWarning", { key });
}

function updateUserEmail(emailAddress: string): Promise<void> {
    return store.dispatch("user/updateUserEmail", { emailAddress });
}

function retrieveProfile(): Promise<void> {
    return store.dispatch("user/retrieveProfile");
}

function closeUserAccount(): Promise<void> {
    return store.dispatch("user/closeUserAccount");
}

function recoverUserAccount(): Promise<void> {
    return store.dispatch("user/recoverUserAccount");
}

function updateSMSResendDateTime(dateTime: DateWrapper): Promise<void> {
    return store.dispatch("user/updateSMSResendDateTime", { dateTime });
}

function calculateTimeForDeletion(): void {
    if (isActiveProfile.value) {
        return undefined;
    }

    let endDate = new DateWrapper(user.value.closedDateTime);
    endDate = endDate.add({ hour: webClientConfig.value.hoursForDeletion });
    timeForDeletion.value = endDate.diff(new DateWrapper()).milliseconds;
}

function isValid(param: BaseValidation): boolean | undefined {
    return param.$dirty === false
        ? undefined
        : !param.$invalid && !param.$pending;
}

function makeEmailEditable(): void {
    isEmailEditable.value = true;
    tempEmail.value = email.value || "";
}

function makeSMSEditable(): void {
    isSMSEditable.value = true;
    tempSMS.value = smsNumber.value || "";
}

function cancelEmailEdit(): void {
    isEmailEditable.value = false;
    email.value = tempEmail.value;
    tempEmail.value = "";
    v$.value.$reset();
}

function cancelSMSEdit(): void {
    isSMSEditable.value = false;
    smsNumber.value = tempSMS.value;
    tempSMS.value = "";
    v$.value.$reset();
}

function saveEmailEdit(): void {
    v$.value.$touch();
    if (v$.value.email.$invalid) {
        submitStatus.value = "ERROR";
    } else {
        submitStatus.value = "PENDING";
        logger.debug(`saveEmailEdit: ${JSON.stringify(email.value)}`);
        sendUserEmailUpdate();
    }
}

function saveSMSEdit(): void {
    v$.value.$touch();
    if (v$.value.smsNumber.$invalid) {
        submitStatus.value = "ERROR";
    } else {
        submitStatus.value = "PENDING";
        if (smsNumber.value) {
            smsNumber.value = smsNumber.value.replace(/\D+/g, "");
        }
        updateSMS();
    }
}

function verifySMS(): void {
    verifySMSModal.value?.showModal();
}

function onVerifySMSSubmit(): void {
    retrieveProfile()
        .then(() => {
            smsVerified.value = user.value.verifiedSMS;
        })
        .catch((error) => {
            if (isTooManyRequestsError(error)) {
                setTooManyRequestsWarning("page");
            } else {
                addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.Profile,
                    undefined
                );
            }
        });
}

function sendUserEmailUpdate(): void {
    isLoading.value = true;
    updateUserEmail(email.value)
        .then(() => {
            logger.verbose("success!");
            isEmailEditable.value = false;
            emailVerified.value = false;
            emailVerificationSent.value = true;
            tempEmail.value = "";
            v$.value.$reset();
            showCheckEmailAlert.value = Boolean(email.value);

            retrieveProfile().catch((error) => {
                if (isTooManyRequestsError(error)) {
                    setTooManyRequestsWarning("page");
                } else {
                    addError(
                        ErrorType.Retrieve,
                        ErrorSourceType.Profile,
                        undefined
                    );
                }
            });
        })
        .catch((err) => {
            logger.error(err);
            if (err.statusCode === 429) {
                setTooManyRequestsError("page");
            } else {
                addError(ErrorType.Update, ErrorSourceType.Profile, undefined);
            }
        })
        .finally(() => {
            isLoading.value = false;
        });
}

function updateSMS(): void {
    logger.debug(
        `Updating ${smsNumber.value ? smsNumber.value : "sms number..."}`
    );

    // Reset timer when user submits their SMS number
    updateSMSResendDateTime(new DateWrapper());

    // Send update to backend
    userProfileService
        .updateSMSNumber(user.value.hdid, smsNumber.value)
        .then(() => {
            isSMSEditable.value = false;
            smsVerified.value = false;
            tempSMS.value = "";

            if (smsNumber.value) {
                verifySMS();
            }
            v$.value.$reset();

            retrieveProfile().catch((error) => {
                if (isTooManyRequestsError(error)) {
                    setTooManyRequestsWarning("page");
                } else {
                    addError(
                        ErrorType.Retrieve,
                        ErrorSourceType.Profile,
                        undefined
                    );
                }
            });
        })
        .catch((error) => {
            if (isTooManyRequestsError(error)) {
                setTooManyRequestsError("page");
            } else {
                addError(ErrorType.Update, ErrorSourceType.Profile, undefined);
            }
        });
}

function recoverAccount(): void {
    isLoading.value = true;
    recoverUserAccount()
        .then(() => logger.verbose("success!"))
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                setTooManyRequestsError("page");
            } else {
                addCustomError(
                    `Unable to recover ${ErrorSourceType.Profile}`,
                    ErrorSourceType.Profile,
                    undefined
                );
            }
        })
        .finally(() => (isLoading.value = false));
}

function closeAccount(): void {
    isLoading.value = true;
    closeUserAccount()
        .then(() => {
            logger.verbose("success!");
            showCloseWarning.value = false;
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                setTooManyRequestsError("page");
            } else {
                addCustomError(
                    `Unable to close ${ErrorSourceType.Profile}`,
                    ErrorSourceType.Profile,
                    undefined
                );
            }
        })
        .finally(() => {
            isLoading.value = false;
        });
}

userProfileService
    .getProfile(user.value.hdid)
    .then((profile) => {
        if (profile) {
            logger.verbose(
                `User Profile: ${JSON.stringify(userProfile.value)}`
            );

            userProfile.value = profile;
            loginDateTimes.value = profile.lastLoginDateTimes;

            email.value = profile.email;
            emailVerified.value = profile.isEmailVerified;
            emailVerificationSent.value = emailVerified.value;

            smsNumber.value = profile.smsNumber;
            smsVerified.value = profile.isSMSNumberVerified;
        }

        isLoading.value = false;
    })
    .catch((error: ResultError) => {
        logger.error(`Error loading profile: ${error.resultMessage}`);
        if (isTooManyRequestsError(error)) {
            setTooManyRequestsError("page");
        } else {
            addError(ErrorType.Retrieve, ErrorSourceType.Profile, undefined);
        }
        isLoading.value = false;
    });

calculateTimeForDeletion();
intervalHandler.value = window.setInterval(
    () => calculateTimeForDeletion(),
    1000
);
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
                <div class="mb-3">
                    <label for="profileNames" class="hg-label">Full Name</label>
                    <div id="profileNames">{{ fullName }}</div>
                </div>
                <div class="mb-3">
                    <label for="PHN" class="hg-label"
                        >Personal Health Number</label
                    >
                    <div id="PHN" data-testid="PHN">
                        {{ patient.personalHealthNumber }}
                    </div>
                </div>
                <div>
                    <b-form-group
                        :state="
                            isValid(v$.email) || !isEmailEditable ? null : false
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
                                        v-model="v$.email.$model"
                                        data-testid="emailInput"
                                        type="email"
                                        :placeholder="
                                            isEmailEditable
                                                ? 'Your email address'
                                                : 'Empty'
                                        "
                                        :disabled="!isEmailEditable"
                                        :state="
                                            isValid(v$.email) ||
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
                                        :disabled="emailVerificationSent"
                                        @click="sendUserEmailUpdate()"
                                    >
                                        Resend Verification
                                    </hg-button>
                                </b-col>
                            </b-row>
                        </div>
                        <b-form-invalid-feedback
                            :state="isValid(v$.email.email)"
                        >
                            Valid email is required
                        </b-form-invalid-feedback>
                        <b-form-invalid-feedback
                            :state="isValid(v$.email.newEmail)"
                            data-testid="emailInvalidNewEqualsOld"
                        >
                            New email must be different from the previous one
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
                                v-else-if="email == null || email === ''"
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
                    <div
                        v-if="!email && tempEmail"
                        class="mb-3 font-weight-bold text-primary text-center"
                        data-testid="emailOptOutMessage"
                    >
                        <hg-icon
                            icon="exclamation-triangle"
                            size="medium"
                            aria-hidden="true"
                            class="mr-2"
                        />
                        <span>
                            Removing your email address will disable future
                            email communications from the Health Gateway.
                        </span>
                    </div>
                    <div v-if="isEmailEditable" class="mb-3">
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
                                tempEmail === email || !isValid(v$.email)
                            "
                            @click="saveEmailEdit()"
                        >
                            Save
                        </hg-button>
                    </div>
                </div>
                <div>
                    <b-form-group
                        :state="
                            isValid(v$.smsNumber) || !isSMSEditable
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
                                        v-model="v$.smsNumber.$model"
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
                                            isValid(v$.smsNumber) ||
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
                            :state="isValid(v$.smsNumber.sms)"
                        >
                            Valid SMS number is required
                        </b-form-invalid-feedback>
                        <b-form-invalid-feedback
                            data-testid="smsInvalidNewEqualsOld"
                            :state="isValid(v$.smsNumber.newSMSNumber)"
                        >
                            New SMS number must be different from the previous
                            one
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
                                    smsNumber == null || smsNumber === ''
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
                    <div
                        v-if="!smsNumber && tempSMS"
                        data-testid="smsOptOutMessage"
                        class="mb-3 font-weight-bold text-primary text-center"
                    >
                        <hg-icon
                            icon="exclamation-triangle"
                            size="medium"
                            aria-hidden="true"
                            class="mr-2"
                        />
                        <span>
                            Removing your phone number will disable future SMS
                            communications from the Health Gateway.
                        </span>
                    </div>
                    <div v-if="isSMSEditable" class="mb-3">
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
                            :disabled="
                                tempSMS === smsNumber || !isValid(v$.smsNumber)
                            "
                            @click="saveSMSEdit()"
                        >
                            Save
                        </hg-button>
                    </div>
                    <div class="mb-3">
                        <label
                            for="postal-address-section"
                            class="hg-label"
                            data-testid="postal-address-label"
                        >
                            {{ postalAddressLabel }}
                        </label>
                        <div id="postal-address-section">
                            <div
                                v-if="postalAddress"
                                id="postal-address-div"
                                data-testid="postal-address-div"
                            >
                                <div
                                    v-for="(
                                        item, index
                                    ) in postalAddress.streetLines"
                                    :key="index"
                                >
                                    {{ item }}
                                </div>
                                <div>
                                    {{ postalAddress.city }},
                                    {{ postalAddress.state }},
                                    {{ postalAddress.postalCode }}
                                </div>
                            </div>
                            <div v-else id="no-postal-address-text-div">
                                <em data-testid="no-postal-address-text">
                                    No address on record
                                </em>
                            </div>
                        </div>
                    </div>
                    <div v-if="!isSameAddress" class="mb-3">
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
                                v-if="physicalAddress"
                                id="physical-address-div"
                                data-testid="physical-address-div"
                            >
                                <div
                                    v-for="(
                                        item, index
                                    ) in physicalAddress.streetLines"
                                    :key="index"
                                >
                                    {{ item }}
                                </div>
                                <div>
                                    {{ physicalAddress.city }},
                                    {{ physicalAddress.state }},
                                    {{ physicalAddress.postalCode }}
                                </div>
                            </div>
                            <div v-else id="no-physical-address-text-div">
                                <em data-testid="no-physical-address-text">
                                    No address on record
                                </em>
                            </div>
                        </div>
                    </div>
                    <div v-if="hasAddress && isSameAddress" class="mb-3">
                        If this address is incorrect, update it
                        <a
                            href="https://www.addresschange.gov.bc.ca/"
                            target="_blank"
                            rel="noopener"
                            >here</a
                        >
                        <span>.</span>
                    </div>
                    <div v-if="!isSameAddress" class="mb-3">
                        If either of these addresses is incorrect, update them
                        <a
                            href="https://www.addresschange.gov.bc.ca/"
                            target="_blank"
                            rel="noopener"
                            >here</a
                        >
                        <span>.</span>
                    </div>
                    <div v-if="!hasAddress" class="mb-3">
                        To add an address, visit
                        <a
                            href="https://www.addresschange.gov.bc.ca/"
                            target="_blank"
                            rel="noopener"
                            >this page</a
                        >
                        <span>.</span>
                    </div>
                    <div class="mb-3">
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
                    </div>
                </div>
            </div>
            <div v-else>
                <div class="mb-3">
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
                </div>
                <div class="mb-3">
                    <label class="hg-label"
                        >Time remaining for deletion:
                    </label>
                    {{ timeForDeletionString }}
                </div>
                <div class="mb-3">
                    <hg-button
                        id="recoverAccountCancelBtn"
                        data-testid="recoverAccountCancelBtn"
                        class="mx-auto"
                        variant="primary"
                        @click="recoverAccount()"
                        >Recover Account
                    </hg-button>
                </div>
            </div>
            <div v-if="isActiveProfile" class="mb-3">
                <label class="hg-label">Manage Account</label>
                <div>
                    <hg-button
                        v-if="!showCloseWarning"
                        id="recoverAccountShowCloseWarningBtn"
                        data-testid="recoverAccountShowCloseWarningBtn"
                        class="p-0 pt-2"
                        variant="link-danger"
                        @click="showCloseWarning = true"
                        >Delete My Account
                    </hg-button>
                    <div
                        v-if="showCloseWarning"
                        class="mb-3 font-weight-bold text-danger text-center"
                    >
                        <hr />
                        <hg-icon
                            icon="exclamation-triangle"
                            size="medium"
                            aria-hidden="true"
                            class="mr-2"
                        />
                        <span
                            >Your account will be marked for removal, preventing
                            you from accessing your information on the Health
                            Gateway. After a set period of time it will be
                            removed permanently.</span
                        >
                    </div>
                    <div v-if="showCloseWarning" class="mb-3 text-right">
                        <hg-button
                            id="closeAccountCancelBtn"
                            data-testid="closeAccountCancelBtn"
                            variant="secondary"
                            @click="showCloseWarning = false"
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
                    </div>
                </div>
            </div>
        </div>
        <content-placeholders v-else>
            <content-placeholders-heading />
            <content-placeholders-text :lines="1" />
            <content-placeholders-heading />
            <content-placeholders-text :lines="3" />
        </content-placeholders>
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
