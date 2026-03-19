<script setup lang="ts">
import useVuelidate from "@vuelidate/core";
import {
    helpers,
    minLength,
    minValue,
    required,
    requiredIf,
} from "@vuelidate/validators";
import { Mask, MaskaDetail } from "maska";
import { vMaska } from "maska/vue";
import { computed, ref } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgDatePickerComponent from "@/components/common/HgDatePickerComponent.vue";
import InfoPopoverComponent from "@/components/common/InfoPopoverComponent.vue";
import InfoTooltipComponent from "@/components/common/InfoTooltipComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import { ActionType } from "@/constants/actionType";
import { ErrorSourceType } from "@/constants/errorType";
import { PcrDataSource } from "@/constants/pcrTestDataSource";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import PcrTestData from "@/models/pcrTestData";
import RegisterTestKitPublicRequest from "@/models/registerTestKitPublicRequest";
import RegisterTestKitRequest from "@/models/registerTestKitRequest";
import { ILogger, IPcrTestService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";
import { phnMask, postalCodeMask, smsMask } from "@/utility/masks";
import ValidationUtil from "@/utility/validationUtil";

interface Props {
    dataSource: PcrDataSource;
    serialNumber?: string;
}
const props = defineProps<Props>();

const emit = defineEmits<{
    (e: "on-success"): void;
    (e: "on-cancel"): void;
}>();

defineExpose({ resetForm });

const phoneMaska = new Mask({ mask: smsMask });
const phoneMaskOptions = {
    mask: smsMask,
    eager: true,
};
const phnMaska = new Mask({ mask: phnMask });
const phnMaskOptions = {
    mask: phnMask,
    eager: true,
};
const postalCodeMaskOptions = {
    mask: postalCodeMask,
    eager: true,
    onMaska: (value: MaskaDetail) =>
        (pcrTest.value.postalOrZip = value.masked.toUpperCase()),
};

interface ISelectOption {
    title: string;
    value: unknown;
}

const currentDate = DateWrapper.today();

const testTakenMinutesAgoOptions: ISelectOption[] = [
    { value: -1, title: "Time" },
    { value: 5, title: "Just now" },
    { value: 30, title: "Within 30 minutes" },
    { value: 120, title: "Within 2 hours" },
    { value: 240, title: "Within 4 hours" },
    { value: 360, title: "Within 6 hours" },
    { value: 480, title: "Within 8 hours" },
    { value: 720, title: "Within 12 hours" },
    { value: 1080, title: "Within 18 hours" },
    { value: 1440, title: "Within 24 hours" },
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
    testKitCid: props.serialNumber ?? "",
    testKitCode: "",
};

const validationMessages = {
    firstName: {
        required: "First name is required.",
    },
    lastName: {
        required: "Last name is required.",
    },
    phn: {
        formatted: "A valid PHN is required.",
    },
    address: {
        required: "Address is required.",
    },
    city: {
        required: "City is required.",
    },
    dob: {
        required: "A valid date of birth is required.",
        maxValue: "Date of birth must be in the past.",
    },
    contactPhoneNumber: {
        minLength: "Phone number must be valid.",
    },
    postalOrZip: {
        required: "Postal code is required.",
        minLength: "Postal code must be at least 7 characters.",
    },
    testTakenMinutesAgo: {
        required: "Time since test taken is required.",
        minValue: "Time since test taken cannot be negative.",
    },
    testKitCode: {
        required: "Test Kit Code is required.",
        formatted: "Test Kit Code is invalid.",
    },
};

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const pcrTestService = container.get<IPcrTestService>(
    SERVICE_IDENTIFIER.PcrTestService
);
const userStore = useUserStore();
const errorStore = useErrorStore();

const isLoading = ref(false);
const errorMessage = ref("");
const noPhn = ref(false);
const noSerialNumber = ref(false);
const noTestKitCode = ref(false);
const pcrTest = ref({ ...emptyPcrTestData });

const fullName = computed(() =>
    userStore.oidcUserInfo
        ? `${userStore.oidcUserInfo.given_name} ${userStore.oidcUserInfo.family_name}`
        : ""
);
const validations = computed(() => ({
    firstName: {
        required: helpers.withMessage(
            validationMessages.firstName.required,
            requiredIf(() => props.dataSource === PcrDataSource.Manual)
        ),
    },
    lastName: {
        required: helpers.withMessage(
            validationMessages.lastName.required,
            requiredIf(() => props.dataSource === PcrDataSource.Manual)
        ),
    },
    phn: {
        required: helpers.withMessage(
            validationMessages.phn.formatted,
            requiredIf(
                () => props.dataSource === PcrDataSource.Manual && !noPhn.value
            )
        ),
        formatted: helpers.withMessage(
            validationMessages.phn.formatted,
            (value: string) =>
                props.dataSource === PcrDataSource.Manual && !noPhn.value
                    ? ValidationUtil.validatePhn(value)
                    : true
        ),
    },
    dob: {
        required: helpers.withMessage(
            validationMessages.dob.required,
            requiredIf(() => props.dataSource === PcrDataSource.Manual)
        ),
        maxValue: helpers.withMessage(
            validationMessages.dob.maxValue,
            (value: string) =>
                props.dataSource === PcrDataSource.Manual
                    ? DateWrapper.fromIsoDate(value).isBefore(
                          DateWrapper.today()
                      )
                    : true
        ),
    },
    contactPhoneNumber: {
        minLength: helpers.withMessage(
            validationMessages.contactPhoneNumber.minLength,
            minLength(14)
        ),
    },
    streetAddress: {
        required: helpers.withMessage(
            validationMessages.address.required,
            requiredIf(
                () => props.dataSource === PcrDataSource.Manual && noPhn.value
            )
        ),
    },
    city: {
        required: helpers.withMessage(
            validationMessages.city.required,
            requiredIf(
                () => props.dataSource === PcrDataSource.Manual && noPhn.value
            )
        ),
    },
    postalOrZip: {
        required: helpers.withMessage(
            validationMessages.postalOrZip.required,
            requiredIf(
                () => props.dataSource === PcrDataSource.Manual && noPhn.value
            )
        ),
        minLength: helpers.withMessage(
            validationMessages.postalOrZip.minLength,
            minLength(7)
        ),
    },
    testTakenMinutesAgo: {
        required: helpers.withMessage(
            validationMessages.testTakenMinutesAgo.required,
            required
        ),
        minValue: helpers.withMessage(
            validationMessages.testTakenMinutesAgo.required,
            minValue(0)
        ),
    },
    testKitCode: {
        required: helpers.withMessage(
            validationMessages.testKitCode.required,
            requiredIf(noSerialNumber)
        ),
        formatted: helpers.withMessage(
            validationMessages.testKitCode.formatted,
            (value: string) =>
                /^([a-zA-Z\d]{7})-([a-zA-Z\d]{5})$|^$/.test(value)
        ),
    },
}));

const v$ = useVuelidate(validations, pcrTest);

function setHasNoPhn(value: boolean | null): void {
    noPhn.value = value ?? false;
    const phnField = v$.value.phn;
    if (phnField) {
        pcrTest.value.phn = "";
        phnField.$touch();
    }
}

function submitKeycloak(shortCodeFirst?: string, shortCodeSecond?: string) {
    const testKitRequest: RegisterTestKitRequest = {
        hdid: userStore.oidcUserInfo?.hdid,
        testTakenMinutesAgo: pcrTest.value.testTakenMinutesAgo,
        testKitCid: pcrTest.value.testKitCid,
        shortCodeFirst,
        shortCodeSecond,
    };
    logger.debug(JSON.stringify(testKitRequest));

    isLoading.value = true;
    pcrTestService
        .registerTestKit(userStore.user.hdid, testKitRequest)
        .then((response) => {
            logger.debug(
                `registerTestKit Response: ${JSON.stringify(response)}`
            );
            emit("on-success");
        })
        .catch((err: ResultError) => handleError(err, "registerTestKit"))
        .finally(() => (isLoading.value = false));
}

function submitManual(shortCodeFirst?: string, shortCodeSecond?: string) {
    const phnDigits = pcrTest.value.phn;
    const phoneDigits = pcrTest.value.contactPhoneNumber;
    const testKitPublicRequest: RegisterTestKitPublicRequest = {
        firstName: pcrTest.value.firstName,
        lastName: pcrTest.value.lastName,
        phn: phnDigits ? phnMaska.unmasked(phnDigits) : "",
        dob: pcrTest.value.dob,
        contactPhoneNumber: phoneDigits ? phoneMaska.unmasked(phoneDigits) : "",
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
                `registerTestKitPublic Response: ${JSON.stringify(response)}`
            );
            emit("on-success");
        })
        .catch((err: ResultError) => handleError(err, "registerTestKitPublic"));
}

function handleSubmit(): void {
    v$.value.$touch();
    errorMessage.value = "";
    if (v$.value.$invalid) {
        return;
    }
    errorStore.clearErrors();
    const shortCodeFirst =
        pcrTest.value.testKitCode.length > 0
            ? pcrTest.value.testKitCode.split("-")[0]
            : "";

    const shortCodeSecond =
        pcrTest.value.testKitCode.length > 0
            ? pcrTest.value.testKitCode.split("-")[1]
            : "";

    switch (props.dataSource) {
        case PcrDataSource.Keycloak:
            submitKeycloak(shortCodeFirst, shortCodeSecond);
            break;
        case PcrDataSource.Manual:
            submitManual(shortCodeFirst, shortCodeSecond);
            break;
        default:
            break;
    }
}

function handleError(err: ResultError, domain: string): void {
    logger.error(`${domain} Error: ${JSON.stringify(err)}`);
    if (err.statusCode === 429) {
        errorStore.setTooManyRequestsError("page");
    } else if (err.actionCode == ActionType.Processed) {
        errorMessage.value = err.message;
    } else {
        errorStore.addCustomError(
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
    emit("on-cancel");
}

function resetForm(): void {
    pcrTest.value = { ...emptyPcrTestData };
    noPhn.value = false;
    v$.value.$reset();
}

if (!props.serialNumber) {
    noSerialNumber.value = true;
    noTestKitCode.value = true;
}
</script>

<template>
    <PageTitleComponent title="Register a Test Kit" />
    <HgAlertComponent
        v-if="!!errorMessage"
        data-testid="alreadyProcessedBanner"
        type="warning"
        closable
        variant="outlined"
        class="d-print-none"
        :text="errorMessage"
        :center-content="true"
    />
    <v-row>
        <v-col>
            <form @submit.prevent="handleSubmit">
                <DisplayFieldComponent
                    v-if="dataSource === PcrDataSource.Keycloak"
                    name="Name"
                    :value="fullName"
                />
                <v-text-field
                    v-if="noTestKitCode"
                    v-model="v$.testKitCode.$model"
                    label="Test Kit Code"
                    class="mb-2"
                    required
                    data-testid="test-kit-code-input"
                    placeholder="Test Kit Code"
                    :error-messages="
                        ValidationUtil.getErrorMessages(v$.testKitCode)
                    "
                    @blur="v$.testKitCode.$touch()"
                />
                <v-row v-if="dataSource === PcrDataSource.Manual">
                    <v-col>
                        <v-text-field
                            v-model="v$.firstName.$model"
                            label="First Name"
                            class="mb-2"
                            data-testid="first-name-input"
                            placeholder="First Name"
                            :error-messages="
                                ValidationUtil.getErrorMessages(v$.firstName)
                            "
                            @blur="v$.firstName.$touch()"
                        />
                        <v-text-field
                            v-model="v$.lastName.$model"
                            label="Last Name"
                            class="mb-2"
                            data-testid="last-name-input"
                            type="text"
                            placeholder="Last Name"
                            :error-messages="
                                ValidationUtil.getErrorMessages(v$.lastName)
                            "
                            @blur="v$.lastName.$touch()"
                        />
                        <v-text-field
                            v-model="v$.phn.$model"
                            v-maska="phnMaskOptions"
                            label="Personal Health Number"
                            class="mb-2"
                            data-testid="phn-input"
                            placeholder="PHN"
                            aria-label="Personal Health Number"
                            :error-messages="
                                ValidationUtil.getErrorMessages(v$.phn)
                            "
                            :disabled="noPhn"
                            @blur="v$.phn.$touch()"
                        />
                    </v-col>
                </v-row>
                <v-row
                    v-if="dataSource === PcrDataSource.Manual"
                    align="center"
                    no-gutters
                >
                    <v-col cols="auto">
                        <v-checkbox
                            v-model="noPhn"
                            class="d-inline"
                            data-testid="phn-checkbox"
                            color="primary"
                            hide-details
                            label="I Don't Have a PHN"
                            @update:model-value="setHasNoPhn"
                        />
                    </v-col>
                    <v-col class="d-flex align-center">
                        <InfoTooltipComponent
                            data-testid="pcr-no-phn-info-button"
                            text="You can find your personal health number
                                    (PHN) on your BC Services Card. If you do
                                    not have a PHN, please enter your address so
                                    we can register your PCR test kit to you."
                            class="ml-2"
                        />
                    </v-col>
                </v-row>
                <v-row v-if="dataSource === PcrDataSource.Manual && noPhn">
                    <v-col>
                        <v-text-field
                            v-model="v$.streetAddress.$model"
                            label="Street Address"
                            data-testid="pcr-street-address-input"
                            type="text"
                            class="mb-2"
                            placeholder="Address"
                            :error-messages="
                                ValidationUtil.getErrorMessages(
                                    v$.streetAddress
                                )
                            "
                            @blur="v$.streetAddress.$touch()"
                        />
                        <v-text-field
                            v-model="v$.city.$model"
                            label="City"
                            data-testid="pcr-city-input"
                            type="text"
                            class="mb-2"
                            placeholder="City"
                            :error-messages="
                                ValidationUtil.getErrorMessages(v$.city)
                            "
                            @blur="v$.city.$touch()"
                        />
                        <v-text-field
                            v-model="v$.postalOrZip.$model"
                            v-maska="postalCodeMaskOptions"
                            label="Postal Code"
                            data-testid="pcr-zip-input"
                            type="text"
                            class="mb-2"
                            placeholder="Postal Code"
                            :error-messages="
                                ValidationUtil.getErrorMessages(v$.postalOrZip)
                            "
                            @blur="v$.postalOrZip.$touch()"
                        />
                    </v-col>
                </v-row>
                <v-row>
                    <v-col>
                        <HgDatePickerComponent
                            v-if="dataSource === PcrDataSource.Manual"
                            v-model="v$.dob.$model"
                            label="Date of Birth"
                            data-testid="dob-input"
                            class="mb-2"
                            :max-date="currentDate"
                            :error-messages="
                                ValidationUtil.getErrorMessages(v$.dob)
                            "
                            aria-label="Date of Birth"
                            @blur="v$.dob.$touch()"
                        />
                        <v-text-field
                            v-if="dataSource === PcrDataSource.Manual"
                            v-model="v$.contactPhoneNumber.$model"
                            v-maska="phoneMaskOptions"
                            data-testid="contact-phone-number-input"
                            class="mb-2"
                            type="tel"
                            maxlength="14"
                            placeholder="(###) ###-####"
                            :error-messages="
                                ValidationUtil.getErrorMessages(
                                    v$.contactPhoneNumber
                                )
                            "
                            @blur="v$.contactPhoneNumber.$touch()"
                        >
                            <template #label>
                                Mobile Number
                                <span class="ml-2 text-medium-emphasis">
                                    (to receive a notification once your
                                    COVID‑19 test result is available)
                                </span>
                            </template>
                        </v-text-field>
                        <v-select
                            v-model="v$.testTakenMinutesAgo.$model"
                            label="Time Since Test Taken"
                            data-testid="test-taken-minutes-ago"
                            :items="testTakenMinutesAgoOptions"
                            :error-messages="
                                ValidationUtil.getErrorMessages(
                                    v$.testTakenMinutesAgo
                                )
                            "
                            @blur="v$.testTakenMinutesAgo.$touch()"
                        />
                    </v-col>
                </v-row>
                <div data-testid="pcr-privacy-statement" class="my-4">
                    <InfoPopoverComponent
                        button-text="Privacy Statement"
                        button-test-id="privacy-statement-button"
                        popover-test-id="privacy-statement-popover"
                    >
                        Your information is being collected to provide you with
                        your COVID‑19 test result under s. 26(c) of the
                        <em
                            >Freedom of Information and Protection of Privacy
                            Act</em
                        >. Contact the Ministry Privacy Officer at
                        <a
                            href="mailto:MOH.Privacy.Officer@gov.bc.ca"
                            class="text-link"
                            >MOH.Privacy.Officer@gov.bc.ca</a
                        >
                        if you have any questions about this collection.
                    </InfoPopoverComponent>
                </div>
                <v-row>
                    <v-col cols="4">
                        <HgButtonComponent
                            id="btn-cancel"
                            block
                            variant="secondary"
                            data-testid="btn-cancel"
                            :disabled="isLoading"
                            text="Cancel"
                            @click="handleCancel"
                        />
                    </v-col>
                    <v-col cols="8">
                        <HgButtonComponent
                            id="btn-register-kit"
                            block
                            type="submit"
                            variant="primary"
                            data-testid="btn-register-kit"
                            :loading="isLoading"
                            text="Register Kit"
                            @click="v$.$touch()"
                        />
                    </v-col>
                </v-row>
            </form>
        </v-col>
    </v-row>
</template>
