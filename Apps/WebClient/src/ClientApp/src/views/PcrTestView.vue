<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import { BaseValidation, useVuelidate } from "@vuelidate/core";
import {
    maxLength,
    minLength,
    minValue,
    required,
    requiredIf,
} from "@vuelidate/validators";
import { computed, ref, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import LoadingComponent from "@/components/LoadingComponent.vue";
import HgDateDropdownComponent from "@/components/shared/HgDateDropdownComponent.vue";
import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { PcrDataSource } from "@/constants/pcrTestDataSource";
import { IdentityProviderConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import PcrTestData from "@/models/pcrTestData";
import RegisterTestKitPublicRequest from "@/models/registerTestKitPublicRequest";
import RegisterTestKitRequest from "@/models/registerTestKitRequest";
import User, { OidcUserInfo } from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IPcrTestService } from "@/services/interfaces";
import { phnMask, smsMask } from "@/utility/masks";
import PHNValidator from "@/utility/phnValidator";

library.add(faInfoCircle);

interface Props {
    serialNumber?: string;
}
const props = defineProps<Props>();

interface ISelectOption {
    text: string;
    value: unknown;
}

const testTakenMinutesAgoOptions: ISelectOption[] = [
    { value: -1, text: "Time" },
    { value: 5, text: "Just now" },
    { value: 30, text: "Within 30 minutes" },
    { value: 120, text: "Within 2 hours" },
    { value: 240, text: "Within 4 hours" },
    { value: 360, text: "Within 6 hours" },
    { value: 480, text: "Within 8 hours" },
    { value: 720, text: "Within 12 hours" },
    { value: 1080, text: "Within 18 hours" },
    { value: 1440, text: "Within 24 hours" },
];

const emptyPcrTestData: PcrTestData = {
    firstName: "",
    lastName: "",
    phn: "",
    dob: "",
    contactPhoneNumber: "",
    streetAddress: "",
    city: "",
    postalOrZip: "",
    testTakenMinutesAgo: -1,
    hdid: "",
    testKitCid: props.serialNumber || "",
    testKitCode: "",
};

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const pcrTestService = container.get<IPcrTestService>(
    SERVICE_IDENTIFIER.PcrTestService
);
const store = useStore();

const noSerialNumber = ref(false);
const noTestKitCode = ref(false);
const noPhn = ref(false);
const registrationComplete = ref(false);
const isLoading = ref(false);
const errorMessage = ref("");
const pcrTest = ref<PcrTestData>({ ...emptyPcrTestData });
const dataSource = ref(PcrDataSource.None);

const user = computed<User>(() => store.getters["user/user"]);
const oidcUserInfo = computed<OidcUserInfo | undefined>(
    () => store.getters["user/oidcUserInfo"]
);
const oidcIsAuthenticated = computed<User>(
    () => store.getters["auth/oidcIsAuthenticated"]
);
const identityProviders = computed<IdentityProviderConfiguration[]>(
    () => store.getters["config/identityProviders"]
);

const fullName = computed(() =>
    oidcUserInfo.value
        ? `${oidcUserInfo.value.given_name} ${oidcUserInfo.value.family_name}`
        : ""
);
const validations = computed(() => ({
    pcrTest: {
        firstName: {
            required: requiredIf(dataSource.value === PcrDataSource.Manual),
        },
        lastName: {
            required: requiredIf(dataSource.value === PcrDataSource.Manual),
        },
        phn: {
            required: requiredIf(
                dataSource.value === PcrDataSource.Manual && !noPhn.value
            ),
            formatted: (value: string) =>
                dataSource.value === PcrDataSource.Manual && !noPhn.value
                    ? PHNValidator.IsValid(value)
                    : true,
        },
        dob: {
            required: requiredIf(dataSource.value === PcrDataSource.Manual),
            maxValue: (value: string) =>
                dataSource.value === PcrDataSource.Manual
                    ? new DateWrapper(value).isBefore(new DateWrapper())
                    : true,
        },
        contactPhoneNumber: {
            required: false,
            minLength: minLength(14),
            maxLength: maxLength(14),
        },
        streetAddress: {
            required: requiredIf(
                dataSource.value === PcrDataSource.Manual && noPhn.value
            ),
        },
        city: {
            required: requiredIf(
                dataSource.value === PcrDataSource.Manual && noPhn.value
            ),
        },
        postalOrZip: {
            required: requiredIf(
                dataSource.value === PcrDataSource.Manual && noPhn.value
            ),
            minLength: minLength(7),
        },
        testTakenMinutesAgo: {
            required,
            minValue: minValue(0),
        },
        testKitCode: {
            required: requiredIf(noSerialNumber.value),
            formatted: (value: string) =>
                /^([a-zA-Z\d]{7})-([a-zA-Z\d]{5})$|^$/.test(value),
        },
    },
}));

const v$ = useVuelidate(validations, { pcrTest });

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

function clearErrors(): void {
    store.dispatch("errorBanner/clearErrors");
}

function signIn(
    redirectPath: string,
    idpHint: string | undefined
): Promise<void> {
    return store.dispatch("auth/signIn", { redirectPath, idpHint });
}

function setHasNoPhn(value: boolean): void {
    noPhn.value = value;
    const phnField = v$.value.pcrTest.phn;
    if (phnField) {
        pcrTest.value.phn = "";
        phnField.$touch();
    }
}

// Sets the data source to either NONE (landing), KEYCLOAK (login), or MANUAL (registration)
function setDataSource(value: PcrDataSource): void {
    if (value === PcrDataSource.Keycloak && !oidcIsAuthenticated.value) {
        isLoading.value = true;
        const redirectPath = props.serialNumber
            ? `/pcrtest/${props.serialNumber}`
            : "/pcrtest";
        signIn(redirectPath, identityProviders.value[0].hint)
            .catch((err) => {
                logger.error(`oidcLogin Error: ${err}`);
                addError(ErrorType.Retrieve, ErrorSourceType.User, undefined);
            })
            .finally(() => (isLoading.value = false));
    }
    resetForm();
    dataSource.value = value;
}

function isValid(param: BaseValidation): boolean | undefined {
    return param.$dirty ? !param.$invalid : undefined;
}

function handleSubmit(): void {
    v$.value.$touch();
    errorMessage.value = "";
    if (v$.value.$invalid) {
        return;
    }
    clearErrors();
    const shortCodeFirst =
        pcrTest.value.testKitCode.length > 0
            ? pcrTest.value.testKitCode.split("-")[0]
            : "";

    const shortCodeSecond =
        pcrTest.value.testKitCode.length > 0
            ? pcrTest.value.testKitCode.split("-")[1]
            : "";

    switch (dataSource.value) {
        // ### Submitted through OIDC
        case PcrDataSource.Keycloak:
            const testKitRequest: RegisterTestKitRequest = {
                hdid: oidcUserInfo.value?.hdid,
                testTakenMinutesAgo: pcrTest.value.testTakenMinutesAgo,
                testKitCid: pcrTest.value.testKitCid,
                shortCodeFirst,
                shortCodeSecond,
            };
            logger.debug(JSON.stringify(testKitRequest));

            isLoading.value = true;
            pcrTestService
                .registerTestKit(user.value.hdid, testKitRequest)
                .then((response) => {
                    logger.debug(
                        `registerTestKit Response: ${JSON.stringify(response)}`
                    );
                    displaySuccess();
                })
                .catch((err: ResultError) =>
                    handleError(err, "registerTestKit")
                );
            break;
        // ### Submitted through manual input
        case PcrDataSource.Manual:
            const phnDigits = pcrTest.value.phn;
            const phoneDigits = pcrTest.value.contactPhoneNumber;
            const testKitPublicRequest: RegisterTestKitPublicRequest = {
                firstName: pcrTest.value.firstName,
                lastName: pcrTest.value.lastName,
                phn: phnDigits ? phnDigits.replace(/\D/g, "") : "",
                dob: pcrTest.value.dob,
                contactPhoneNumber: phoneDigits
                    ? phoneDigits.replace(/\D/g, "")
                    : "",
                streetAddress: pcrTest.value.streetAddress,
                city: pcrTest.value.city,
                postalOrZip: pcrTest.value.postalOrZip ?? "",
                testTakenMinutesAgo: pcrTest.value.testTakenMinutesAgo,
                testKitCid: pcrTest.value.testKitCid,
                shortCodeFirst,
                shortCodeSecond,
            };
            logger.debug(JSON.stringify(testKitPublicRequest));
            isLoading.value = true;
            pcrTestService
                .registerTestKitPublic(testKitPublicRequest)
                .then((response) => {
                    logger.debug(
                        `registerTestKitPublic Response: ${JSON.stringify(
                            response
                        )}`
                    );
                    displaySuccess();
                })
                .catch((err: ResultError) =>
                    handleError(err, "registerTestKitPublic")
                );
            break;
        default:
            break;
    }
}

function displaySuccess(): void {
    resetForm();
    isLoading.value = false;
    registrationComplete.value = true;
    window.scrollTo({ top: 0, behavior: "smooth" });
}

function handleError(err: ResultError, domain: string): void {
    logger.error(`${domain} Error: ${err}`);
    if (err.statusCode === 429) {
        setTooManyRequestsError("page");
    } else if (err.actionCode == ActionType.Processed) {
        errorMessage.value = err.resultMessage;
    } else {
        addCustomError(
            "Unable to register test kit",
            ErrorSourceType.TestKit,
            err.traceId
        );
    }
    isLoading.value = false;
    window.scrollTo({ top: 0, behavior: "smooth" });
}

function handleCancel(): void {
    resetForm();
    dataSource.value = PcrDataSource.None;
    window.scrollTo({ top: 0, behavior: "smooth" });
}

function resetForm(): void {
    pcrTest.value = { ...emptyPcrTestData };
    noPhn.value = false;
    v$.value.$reset();
}

watch(oidcIsAuthenticated, (value) => {
    if (value) {
        dataSource.value = PcrDataSource.Keycloak;
    }
});

if (!props.serialNumber) {
    noSerialNumber.value = true;
    noTestKitCode.value = true;
}

if (oidcIsAuthenticated.value) {
    dataSource.value = PcrDataSource.Keycloak;
}
</script>

<template>
    <div>
        <!-- LANDING -->
        <b-container v-if="isLoading">
            <LoadingComponent :is-loading="isLoading" />
        </b-container>
        <b-container
            v-if="
                !registrationComplete &&
                dataSource === PcrDataSource.None &&
                !isLoading
            "
        >
            <b-row class="pt-3">
                <b-col>
                    <strong>
                        Register your COVID‑19 test kit using one of the
                        following methods:
                    </strong>
                </b-col>
            </b-row>
            <b-row>
                <b-col>
                    <!-- add whitespace above buttons -->
                    <b-row class="pt-3" />
                    <b-row class="pt-5" />
                    <b-row class="pt-5" align="center">
                        <b-col>
                            <hg-button
                                id="btnLogin"
                                aria-label="BC Services Card Login"
                                data-testid="btn-login"
                                variant="primary"
                                class="login-button"
                                @click="setDataSource(PcrDataSource.Keycloak)"
                            >
                                <span>Log in with BC Services Card</span>
                            </hg-button>
                        </b-col>
                    </b-row>
                    <b-row class="my-3 no-gutters align-items-center">
                        <b-col>
                            <hr />
                        </b-col>
                        <b-col cols="auto">
                            <h3 class="h5 m-0 px-3 text-muted">OR</h3>
                        </b-col>
                        <b-col>
                            <hr />
                        </b-col>
                    </b-row>
                    <b-row align="center">
                        <b-col>
                            <hg-button
                                id="btn-manual"
                                data-testid="btn-manual"
                                variant="secondary"
                                class="manual-enter-button"
                                @click="setDataSource(PcrDataSource.Manual)"
                            >
                                Manually Enter Your Information
                            </hg-button>
                        </b-col>
                    </b-row>
                </b-col>
            </b-row>
        </b-container>

        <!-- FORM -->

        <b-container
            v-if="!registrationComplete && dataSource !== PcrDataSource.None"
            v-show="!isLoading"
        >
            <b-row id="title" class="mt-3">
                <b-col>
                    <h1 class="h4 mb-2 font-weight-normal">
                        <strong>Register a Test Kit</strong>
                    </h1>
                </b-col>
            </b-row>
            <b-row v-if="!!errorMessage" id="processError" class="pt-3">
                <b-col>
                    <b-alert
                        data-testid="alreadyProcessedBanner"
                        variant="warning"
                        dismissible
                        class="no-print"
                        :show="true"
                    >
                        <p data-testid="alreadyProcessedText">
                            {{ errorMessage }}
                        </p>
                    </b-alert>
                </b-col>
            </b-row>
            <b-row>
                <b-col>
                    <form @submit.prevent="handleSubmit">
                        <!-- NAME -->
                        <b-row
                            v-if="dataSource === PcrDataSource.Keycloak"
                            class="pt-2"
                        >
                            <b-col>
                                <label for="pcrTestFullName">Name:</label>
                                <strong id="prcTestFullName">
                                    {{ fullName }}
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row v-if="noTestKitCode" class="mt-2">
                            <b-col>
                                <label for="testKitCode">Test Kit Code</label>
                                <b-form-input
                                    id="testKitCode"
                                    v-model="pcrTest.testKitCode"
                                    data-testid="test-kit-code-input"
                                    type="text"
                                    placeholder="Test Kit Code"
                                    :state="isValid(v$.pcrTest.testKitCode)"
                                    @blur.native="
                                        v$.pcrTest.testKitCode.$touch()
                                    "
                                />
                                <b-form-invalid-feedback
                                    v-if="
                                        v$.pcrTest.testKitCode.$dirty &&
                                        !v$.pcrTest.testKitCode.required
                                    "
                                    aria-label="PCR Test Kit Code is required"
                                    data-testid="feedback-testkitcode-is-required"
                                >
                                    PCR Test Kit Code is required.
                                </b-form-invalid-feedback>
                                <b-form-invalid-feedback
                                    v-else-if="
                                        v$.pcrTest.testKitCode.$dirty &&
                                        !v$.pcrTest.testKitCode.formatted
                                    "
                                    aria-label="PCR Test Kit Code is invalid"
                                    data-testid="feedback-testkitcode-is-invalid"
                                >
                                    PCR Test Kit Code is invalid.
                                </b-form-invalid-feedback>
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="dataSource === PcrDataSource.Manual"
                            data-testid="pcr-name"
                        >
                            <b-col>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrFirstName">
                                            First Name
                                        </label>
                                        <b-form-input
                                            id="pcrFirstName"
                                            v-model="pcrTest.firstName"
                                            data-testid="first-name-input"
                                            type="text"
                                            placeholder="First Name"
                                            :state="
                                                isValid(v$.pcrTest.firstName)
                                            "
                                            @blur.native="
                                                v$.pcrTest.firstName.$touch()
                                            "
                                        />
                                        <b-form-invalid-feedback
                                            v-if="
                                                v$.pcrTest.firstName.$dirty &&
                                                !v$.pcrTest.firstName.required
                                            "
                                            aria-label="First name is required"
                                            force-show
                                            data-testid="feedback-firstname-is-required"
                                        >
                                            First name is required.
                                        </b-form-invalid-feedback>
                                    </b-col>
                                </b-row>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrLastName"
                                            >Last Name</label
                                        >
                                        <b-form-input
                                            id="pcrLastName"
                                            v-model="pcrTest.lastName"
                                            data-testid="last-name-input"
                                            type="text"
                                            placeholder="Last Name"
                                            :state="
                                                isValid(v$.pcrTest.lastName)
                                            "
                                            @blur.native="
                                                v$.pcrTest.lastName.$touch()
                                            "
                                        />
                                        <b-form-invalid-feedback
                                            v-if="
                                                v$.pcrTest.lastName.$dirty &&
                                                !v$.pcrTest.lastName.required
                                            "
                                            aria-label="Last name is required"
                                            force-show
                                            data-testid="feedback-lastname-is-required"
                                        >
                                            Last name is required.
                                        </b-form-invalid-feedback>
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                        <!-- PHN -->
                        <b-row
                            v-if="dataSource === PcrDataSource.Manual"
                            data-testid="pcr-phn"
                        >
                            <b-col v-if="dataSource === PcrDataSource.Manual">
                                <b-row class="mt-2">
                                    <b-col>
                                        <b-form-group
                                            label="Personal Health Number"
                                            label-for="phn"
                                        >
                                            <b-form-input
                                                id="phn"
                                                v-model="pcrTest.phn"
                                                v-mask="phnMask"
                                                data-testid="phn-input"
                                                placeholder="PHN"
                                                aria-label="Personal Health Number"
                                                :state="isValid(v$.pcrTest.phn)"
                                                :disabled="noPhn"
                                                @blur.native="
                                                    v$.pcrTest.phn.$touch()
                                                "
                                            />
                                            <b-form-invalid-feedback
                                                aria-label="Valid PHN is required"
                                                :state="isValid(v$.pcrTest.phn)"
                                                data-testid="feedback-phn-is-required"
                                            >
                                                Valid PHN is required.
                                            </b-form-invalid-feedback>
                                        </b-form-group>
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                        <!-- NO PHN -->
                        <b-row>
                            <b-col>
                                <!-- Checkbox for PHN or no PHN -->
                                <b-form-checkbox
                                    v-if="dataSource === PcrDataSource.Manual"
                                    id="phnCheckbox"
                                    v-model="noPhn"
                                    data-testid="phn-checkbox"
                                    @change="setHasNoPhn($event)"
                                >
                                    <span>I Don't Have a PHN</span>
                                    <hg-button
                                        :id="'pcr-no-phn-info-button'"
                                        aria-label="Result Description"
                                        href="#"
                                        variant="link"
                                        data-testid="pcr-no-phn-info-button"
                                        class="shadow-none p-0 ml-1"
                                    >
                                        <hg-icon
                                            icon="info-circle"
                                            size="small"
                                        />
                                    </hg-button>
                                    <b-popover
                                        :target="'pcr-no-phn-info-button'"
                                        triggers="hover focus"
                                        placement="bottomleft"
                                        boundary="viewport"
                                        data-testid="pcr-no-phn-info-popover"
                                    >
                                        You can find your personal health number
                                        (PHN) on your BC Services Card. If you
                                        do not have a PHN, please enter your
                                        address so we can register your PCR test
                                        kit to you.
                                    </b-popover>
                                </b-form-checkbox>
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="dataSource === PcrDataSource.Manual && noPhn"
                            data-testid="pcr-home-address"
                        >
                            <b-col>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrStreetAddress">
                                            Street Address
                                        </label>
                                        <b-form-input
                                            id="pcrStreetAddress"
                                            v-model="pcrTest.streetAddress"
                                            data-testid="pcr-street-address-input"
                                            type="text"
                                            placeholder="Address"
                                            :state="
                                                isValid(
                                                    v$.pcrTest.streetAddress
                                                )
                                            "
                                            @blur.native="
                                                v$.pcrTest.streetAddress.$touch()
                                            "
                                        />
                                        <b-form-invalid-feedback
                                            v-if="
                                                v$.pcrTest.streetAddress
                                                    .$dirty &&
                                                !v$.pcrTest.streetAddress
                                                    .required
                                            "
                                            aria-label="Street address is required"
                                            force-show
                                            data-testid="feedback-streetaddress-is-required"
                                        >
                                            Street address is required.
                                        </b-form-invalid-feedback>
                                    </b-col>
                                </b-row>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrCity">City</label>
                                        <b-form-input
                                            id="pcrCity"
                                            v-model="pcrTest.city"
                                            data-testid="pcr-city-input"
                                            type="text"
                                            placeholder="City"
                                            :state="isValid(v$.pcrTest.city)"
                                            @blur.native="
                                                v$.pcrTest.city.$touch()
                                            "
                                        />
                                        <b-form-invalid-feedback
                                            v-if="
                                                v$.pcrTest.city.$dirty &&
                                                !v$.pcrTest.city.required
                                            "
                                            aria-label="City is required"
                                            force-show
                                            data-testid="feedback-city-is-required"
                                        >
                                            City is required.
                                        </b-form-invalid-feedback>
                                    </b-col>
                                </b-row>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrZip">Postal Code</label>
                                        <b-form-input
                                            id="pcrZip"
                                            v-model="pcrTest.postalOrZip"
                                            v-mask="'A#A #A#'"
                                            data-testid="pcr-zip-input"
                                            type="text"
                                            placeholder="Postal Code"
                                            :state="
                                                isValid(v$.pcrTest.postalOrZip)
                                            "
                                            @blur.native="
                                                v$.pcrTest.postalOrZip.$touch()
                                            "
                                        />
                                        <b-form-invalid-feedback
                                            v-if="
                                                v$.pcrTest.postalOrZip.$dirty &&
                                                !v$.pcrTest.postalOrZip.required
                                            "
                                            aria-label="Postal code is required"
                                            force-show
                                            data-testid="feedback-postal-is-required"
                                        >
                                            Postal code is required.
                                        </b-form-invalid-feedback>
                                        <b-form-invalid-feedback
                                            v-else-if="
                                                v$.pcrTest.postalOrZip.$dirty &&
                                                !v$.pcrTest.postalOrZip
                                                    .minLength
                                            "
                                            aria-label="Postal code is required"
                                            force-show
                                        >
                                            Postal code is required.
                                        </b-form-invalid-feedback>
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                        <!-- DOB -->
                        <b-row data-testid="pcr-dob" class="mt-2">
                            <b-col v-if="dataSource === PcrDataSource.Manual">
                                <b-form-group
                                    label="Date of Birth"
                                    label-for="dob"
                                    :state="isValid(v$.pcrTest.dob)"
                                >
                                    <HgDateDropdownComponent
                                        id="dob"
                                        v-model="pcrTest.dob"
                                        data-testid="dob-input"
                                        :state="isValid(v$.pcrTest.dob)"
                                        :allow-future="false"
                                        aria-label="Date of Birth"
                                        @blur="v$.pcrTest.dob.$touch()"
                                    />
                                    <b-form-invalid-feedback
                                        v-if="
                                            v$.pcrTest.dob.$dirty &&
                                            !v$.pcrTest.dob.required
                                        "
                                        aria-label="Invalid Date of Birth"
                                        data-testid="feedback-dob-is-required"
                                        force-show
                                    >
                                        A valid date of birth is required.
                                    </b-form-invalid-feedback>
                                    <b-form-invalid-feedback
                                        v-else-if="
                                            v$.pcrTest.dob.$dirty &&
                                            !v$.pcrTest.dob.maxValue
                                        "
                                        aria-label="Invalid Date of Birth"
                                        force-show
                                        data-testid="feedback-dob-is-invalid"
                                    >
                                        Date of birth must be in the past.
                                    </b-form-invalid-feedback>
                                </b-form-group>
                            </b-col>
                        </b-row>
                        <!-- MOBILE NUMBER -->
                        <b-row
                            v-if="dataSource === PcrDataSource.Manual"
                            data-testid="pcr-mobile-number"
                        >
                            <b-col>
                                <b-row class="mt-2">
                                    <b-col>
                                        <label for="pcrMobileNumber">
                                            Mobile Number (to receive a
                                            notification once your COVID‑19 test
                                            result is available)
                                        </label>
                                        <b-form-input
                                            id="pcrMobileNumber"
                                            v-model="pcrTest.contactPhoneNumber"
                                            v-mask="smsMask"
                                            data-testid="contact-phone-number-input"
                                            type="tel"
                                            placeholder="(###) ###-####"
                                            :state="
                                                isValid(
                                                    v$.pcrTest
                                                        .contactPhoneNumber
                                                )
                                            "
                                            @blur.native="
                                                v$.pcrTest.contactPhoneNumber.$touch()
                                            "
                                        />
                                        <b-form-invalid-feedback
                                            v-if="
                                                v$.pcrTest.contactPhoneNumber
                                                    .$dirty &&
                                                (!v$.pcrTest.contactPhoneNumber
                                                    .maxLength ||
                                                    !v$.pcrTest
                                                        .contactPhoneNumber
                                                        .minLength)
                                            "
                                            aria-label="Phone number must be valid."
                                            force-show
                                            data-testid="feedback-phonenumber-valid"
                                        >
                                            Phone number must be valid.
                                        </b-form-invalid-feedback>
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                        <!-- TIME SINCE TEST TAKEN -->
                        <b-row class="mt-2">
                            <b-col>
                                <label for="testTakenMinutesAgo">
                                    Time Since Test Taken
                                </label>
                                <b-form-select
                                    id="testTakenMinutesAgo"
                                    v-model="pcrTest.testTakenMinutesAgo"
                                    data-testid="test-taken-minutes-ago"
                                    :options="testTakenMinutesAgoOptions"
                                    :state="
                                        isValid(v$.pcrTest.testTakenMinutesAgo)
                                    "
                                    @blur.native="
                                        v$.pcrTest.testTakenMinutesAgo.$touch()
                                    "
                                >
                                </b-form-select>
                                <b-form-invalid-feedback
                                    v-if="
                                        v$.pcrTest.testTakenMinutesAgo.$dirty &&
                                        (!v$.pcrTest.testTakenMinutesAgo
                                            .required ||
                                            !v$.pcrTest.testTakenMinutesAgo
                                                .minValue)
                                    "
                                    aria-label="Time since test taken is required"
                                    force-show
                                    data-testid="feedback-testtaken-is-required"
                                >
                                    Time since test taken is required.
                                </b-form-invalid-feedback>
                            </b-col>
                        </b-row>
                        <!-- PRIVACY STATEMENT -->
                        <b-row data-testid="pcr-privacy-statement" class="pt-2">
                            <b-col>
                                <hg-button
                                    id="privacy-statement"
                                    aria-label="Privacy Statement"
                                    href="#"
                                    tabindex="0"
                                    variant="link"
                                    data-testid="btn-privacy-statement"
                                    class="shadow-none p-0"
                                >
                                    <hg-icon
                                        icon="info-circle"
                                        size="small"
                                        class="mr-1"
                                    />
                                    <small>Privacy Statement</small>
                                </hg-button>
                                <b-popover
                                    target="privacy-statement"
                                    triggers="hover focus"
                                    placement="topright"
                                    boundary="viewport"
                                >
                                    Your information is being collected to
                                    provide you with your COVID‑19 test result
                                    under s. 26(c) of the
                                    <em
                                        >Freedom of Information and Protection
                                        of Privacy Act</em
                                    >. Contact the Ministry Privacy Officer at
                                    <a
                                        href="mailto:MOH.Privacy.Officer@gov.bc.ca"
                                        >MOH.Privacy.Officer@gov.bc.ca</a
                                    >
                                    if you have any questions about this
                                    collection.
                                </b-popover>
                            </b-col>
                        </b-row>
                        <!-- FORM ACTIONS -->
                        <b-row class="my-3">
                            <b-col cols="4">
                                <hg-button
                                    id="btn-cancel"
                                    block
                                    variant="secondary"
                                    data-testid="btn-cancel"
                                    @click="handleCancel"
                                >
                                    Cancel
                                </hg-button>
                            </b-col>
                            <b-col cols="8">
                                <hg-button
                                    id="btn-register-kit"
                                    block
                                    type="submit"
                                    variant="primary"
                                    data-testid="btn-register-kit"
                                >
                                    Register Kit
                                </hg-button>
                            </b-col>
                        </b-row>
                    </form>
                </b-col>
            </b-row>

            <b-row data-testid="pcr-form-actions"></b-row>
        </b-container>

        <!-- SUCCESS -->

        <b-container v-if="registrationComplete && !isLoading">
            <b-row class="text-center pt-3" align-v="center">
                <b-col>
                    <b-alert
                        data-testid="registration-success-banner"
                        variant="success"
                        class="no-print"
                        :show="true"
                    >
                        <h4 data-testid="registration-success-banner-title">
                            Success!
                        </h4>
                        <span>
                            Your kit has been registered to your profile.
                        </span>
                        <div v-if="oidcIsAuthenticated" class="pt-2">
                            <router-link to="/logout">
                                <hg-button
                                    id="logoutBtn"
                                    aria-label="Logout"
                                    data-testid="logoutBtn"
                                    variant="link"
                                    class="continue-button"
                                >
                                    <span>Log Out</span>
                                </hg-button>
                            </router-link>
                        </div>
                        <div v-else class="pt-2">
                            <router-link to="/">
                                <hg-button
                                    id="btn-continue"
                                    aria-label="Continue"
                                    data-testid="btn-continue"
                                    variant="link"
                                    class="continue-button"
                                >
                                    Back to Home
                                </hg-button>
                            </router-link>
                        </div>
                    </b-alert>
                </b-col>
            </b-row>
        </b-container>
    </div>
</template>

<style scoped lang="scss">
.login-button {
    background-color: #1a5a95 !important;
    border-color: #1a5a95 !important;
}

.phn-info {
    color: #636363 !important;
}
</style>
