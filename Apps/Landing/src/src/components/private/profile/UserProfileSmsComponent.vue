<script setup lang="ts">
import useVuelidate from "@vuelidate/core";
import { helpers, not, sameAs } from "@vuelidate/validators";
import { Mask, MaskaDetail, vMaska } from "maska";
import { computed, ref, watch } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import SectionHeaderComponent from "@/components/common/SectionHeaderComponent.vue";
import VerifySmsDialogComponent from "@/components/private/profile/VerifySmsDialogComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { Loader } from "@/constants/loader";
import ValidationRegEx from "@/constants/validationRegEx";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { isTooManyRequestsError } from "@/models/errors";
import { IUserProfileService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useLoadingStore } from "@/stores/loading";
import { useUserStore } from "@/stores/user";
import PhoneUtil from "@/utility/phoneUtil";
import PromiseUtility from "@/utility/promiseUtility";
import ValidationUtil from "@/utility/validationUtil";

const maskString = "(###) ###-####";
const mask = new Mask({ mask: maskString });
const maskOptions = {
    mask: maskString,
    onMaska: (detail: MaskaDetail) => (rawValue.value = detail.unmasked),
};

const userProfileService = container.get<IUserProfileService>(
    SERVICE_IDENTIFIER.UserProfileService
);
const errorStore = useErrorStore();
const loadingStore = useLoadingStore();
const userStore = useUserStore();

const isSmsEditable = ref(false);

const verified = computed(() => userStore.user.verifiedSms);
const rawStoreValue = computed(() => userStore.user.sms);
const maskedStoreValue = computed(() => mask.masked(rawStoreValue.value));

const rawValue = ref(rawStoreValue.value);
const maskedValue = ref(maskedStoreValue.value);

const verifySmsDialog = ref<InstanceType<typeof VerifySmsDialogComponent>>();

const inputErrorMessages = computed(() =>
    !isSmsEditable.value
        ? []
        : ValidationUtil.getErrorMessages(v$.value.rawValue)
);
const validations = computed(() => ({
    rawValue: {
        newSMSNumber: helpers.withMessage(
            "New SMS number must be different from the previous one",
            not(sameAs(rawStoreValue))
        ),
        sms: helpers.withMessage(
            "Valid SMS number is required",
            helpers.withAsync(validPhoneNumberFormat)
        ),
    },
}));

const v$ = useVuelidate(validations, { rawValue });

async function validPhoneNumberFormat(
    value: string
): Promise<boolean | undefined> {
    if (value === rawStoreValue.value || !value) {
        return true;
    }

    if (!ValidationRegEx.PhoneNumberMasked.test(value)) {
        return false;
    }

    const phoneNumber = PhoneUtil.stripPhoneMask(value);
    return await PromiseUtility.withMinimumDelay(
        userProfileService.isPhoneNumberValid(phoneNumber),
        500
    );
}

function makeSmsEditable(): void {
    isSmsEditable.value = true;
    v$.value.rawValue.$touch();
}

function cancelSmsEdit(): void {
    isSmsEditable.value = false;
    maskedValue.value = maskedStoreValue.value;
}

function saveSmsEdit(): void {
    v$.value.$touch();
    if (v$.value.rawValue.$invalid !== true) {
        updateSms();
    }
}

function updateSms(): void {
    // Reset timer when user submits their SMS number
    userStore.updateSmsResendDateTime(DateWrapper.now());

    // Send update to backend
    loadingStore.applyLoader(
        Loader.UserProfile,
        "updateSms",
        userProfileService
            .updateSmsNumber(userStore.hdid, rawValue.value)
            .then(refreshProfile)
            .then(() => {
                isSmsEditable.value = false;
                v$.value.$reset();

                if (rawStoreValue.value) {
                    verifySms();
                }
            })
            .catch((error) => {
                if (isTooManyRequestsError(error)) {
                    errorStore.setTooManyRequestsError("page");
                } else {
                    errorStore.addError(
                        ErrorType.Update,
                        ErrorSourceType.Profile,
                        undefined
                    );
                }
            })
    );
}

function verifySms(): void {
    verifySmsDialog.value?.showDialog();
}

function handleSmsVerified(): void {
    loadingStore.applyLoader(Loader.UserProfile, "verifySms", refreshProfile());
}

function refreshProfile(): Promise<void> {
    return userStore.retrieveProfile().catch((error) => {
        if (isTooManyRequestsError(error)) {
            errorStore.setTooManyRequestsWarning("page");
        } else {
            errorStore.addError(
                ErrorType.Retrieve,
                ErrorSourceType.Profile,
                undefined
            );
        }
    });
}

watch(maskedStoreValue, (value) => (maskedValue.value = value));
</script>

<template>
    <SectionHeaderComponent title="Cell Number (SMS notifications)">
        <template #append>
            <HgButtonComponent
                data-testid="editSMSBtn"
                class="ml-2"
                :class="{ invisible: isSmsEditable }"
                variant="link"
                text="Edit"
                @click="makeSmsEditable"
            />
        </template>
    </SectionHeaderComponent>
    <v-sheet :max-width="400">
        <v-text-field
            v-model="maskedValue"
            v-maska:[maskOptions]
            data-testid="smsNumberInput"
            :class="{ 'mb-4': inputErrorMessages.length > 0 }"
            type="tel"
            maxlength="14"
            label="Cell Number"
            :placeholder="isSmsEditable ? 'Your phone number' : 'Empty'"
            persistent-placeholder
            :readonly="!isSmsEditable"
            :clearable="isSmsEditable"
            :error-messages="inputErrorMessages"
        >
            <template #append-inner>
                <v-progress-circular
                    v-if="v$.rawValue.$pending"
                    color="info"
                    indeterminate
                    size="24"
                />
            </template>
        </v-text-field>
    </v-sheet>
    <div v-if="isSmsEditable" class="mb-4">
        <v-alert
            v-if="!rawValue && rawStoreValue"
            data-testid="smsOptOutMessage"
            class="pt-0"
            type="error"
            variant="text"
            icon="exclamation-triangle"
            text="Removing your phone number will disable future SMS communications
                from the Health Gateway."
        />
        <HgButtonComponent
            data-testid="cancelSMSEditBtn"
            class="mr-2"
            variant="secondary"
            text="Cancel"
            @click="cancelSmsEdit"
        />
        <HgButtonComponent
            data-testid="saveSMSEditBtn"
            class="mx-2"
            variant="primary"
            text="Save"
            :disabled="
                rawValue === rawStoreValue ||
                !ValidationUtil.isValid(v$.rawValue)
            "
            @click="saveSmsEdit"
        />
    </div>
    <template v-else>
        <div data-testid="smsStatus" class="mb-4">
            <DisplayFieldComponent
                v-if="verified"
                name="Status"
                value="Verified"
                value-class="text-success"
                data-testid="smsStatusVerified"
                horizontal
            />
            <DisplayFieldComponent
                v-else-if="!rawStoreValue"
                name="Status"
                value="Opted Out"
                value-class="text-medium-emphasis"
                data-testid="smsStatusOptedOut"
                horizontal
            />
            <DisplayFieldComponent
                v-else
                name="Status"
                value="Not Verified"
                value-class="text-error"
                data-testid="smsStatusNotVerified"
                horizontal
            />
        </div>
        <HgButtonComponent
            v-if="!verified && rawStoreValue"
            data-testid="verifySMSBtn"
            class="mb-4"
            variant="secondary"
            text="Verify"
            @click="verifySms"
        />
    </template>
    <VerifySmsDialogComponent
        ref="verifySmsDialog"
        :sms-number="rawStoreValue"
        @verified="handleSmsVerified"
    />
</template>
