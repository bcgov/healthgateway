<script setup lang="ts">
import useVuelidate from "@vuelidate/core";
import { helpers, not, sameAs } from "@vuelidate/validators";
import { Mask, MaskaDetail } from "maska";
import { vMaska } from "maska/vue";
import { computed, ref, watch } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
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
import { Action, Text, Type } from "@/plugins/extensions";
import { ITrackingService, IUserProfileService } from "@/services/interfaces";
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
    onMaska: (detail: MaskaDetail) =>
        (inputPhoneNumber.value = detail.unmasked),
};

const userProfileService = container.get<IUserProfileService>(
    SERVICE_IDENTIFIER.UserProfileService
);
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);
const errorStore = useErrorStore();
const loadingStore = useLoadingStore();
const userStore = useUserStore();

const isSmsEditable = ref(false);
const showVerifiedSmsMessage = ref(false);

const verified = computed(() => userStore.user.verifiedSms);
const storePhoneNumber = computed(() => userStore.user.sms);
const maskedStorePhoneNumber = computed(() =>
    mask.masked(storePhoneNumber.value)
);

const inputPhoneNumber = ref(storePhoneNumber.value);
const maskedInputPhoneNumber = ref(maskedStorePhoneNumber.value);

const verifySmsDialog = ref<InstanceType<typeof VerifySmsDialogComponent>>();

const inputErrorMessages = computed(() =>
    isSmsEditable.value
        ? ValidationUtil.getErrorMessages(v$.value.inputPhoneNumber)
        : []
);
const validations = computed(() => ({
    inputPhoneNumber: {
        newSMSNumber: helpers.withMessage(
            "New SMS number must be different from the previous one",
            not(sameAs(storePhoneNumber))
        ),
        sms: helpers.withMessage(
            "Valid SMS number is required",
            helpers.withAsync(validPhoneNumberFormat)
        ),
    },
}));

const v$ = useVuelidate(validations, { inputPhoneNumber });

async function validPhoneNumberFormat(
    value: string
): Promise<boolean | undefined> {
    if (value === storePhoneNumber.value || !value) {
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
    v$.value.inputPhoneNumber.$touch();
}

function cancelSmsEdit(): void {
    isSmsEditable.value = false;
    maskedInputPhoneNumber.value = maskedStorePhoneNumber.value;
}

function saveSmsEdit(): void {
    v$.value.$touch();
    if (v$.value.inputPhoneNumber.$invalid !== true) {
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
            .updateSmsNumber(userStore.hdid, inputPhoneNumber.value)
            .then(refreshProfile)
            .then(() => {
                isSmsEditable.value = false;
                v$.value.$reset();

                if (storePhoneNumber.value) {
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
    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.VerifyMobileNumber,
        type: Type.Profile,
    });
    verifySmsDialog.value?.showDialog();
}

function handleSmsVerified(): void {
    showVerifiedSmsMessage.value = true;
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

watch(
    maskedStorePhoneNumber,
    (value) => (maskedInputPhoneNumber.value = value)
);
watch(maskedInputPhoneNumber, (value) => {
    inputPhoneNumber.value = value ? PhoneUtil.stripPhoneMask(value) : "";
});
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
            v-model="maskedInputPhoneNumber"
            v-maska="maskOptions"
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
                    v-if="v$.inputPhoneNumber.$pending"
                    color="info"
                    indeterminate
                    size="24"
                />
            </template>
        </v-text-field>
    </v-sheet>
    <div v-if="isSmsEditable" class="mb-4">
        <HgAlertComponent
            v-if="!inputPhoneNumber && storePhoneNumber"
            data-testid="smsOptOutMessage"
            class="pt-0"
            type="warning"
            variant="text"
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
                inputPhoneNumber === storePhoneNumber ||
                !ValidationUtil.isValid(v$.inputPhoneNumber)
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
                v-else-if="!storePhoneNumber"
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
        <HgAlertComponent
            v-if="showVerifiedSmsMessage"
            data-testid="verified-sms-message"
            class="pt-0"
            type="success"
            variant="text"
            text="Health Gateway may now send you notifications. You can change your preferences at any time."
        />
        <HgButtonComponent
            v-if="!verified && storePhoneNumber"
            data-testid="verifySMSBtn"
            class="mb-4"
            variant="secondary"
            text="Verify"
            @click="verifySms"
        />
    </template>
    <VerifySmsDialogComponent
        ref="verifySmsDialog"
        :sms-number="storePhoneNumber"
        @verified="handleSmsVerified"
    />
</template>
