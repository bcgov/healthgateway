<script setup lang="ts">
import { useVuelidate } from "@vuelidate/core";
import { helpers, required, sameAs } from "@vuelidate/validators";
import { vMaska } from "maska/vue";
import { computed, Ref, ref } from "vue";
import { useRouter } from "vue-router";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HtmlTextAreaComponent from "@/components/common/HtmlTextAreaComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import PageErrorComponent from "@/components/error/PageErrorComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import ValidationRegEx from "@/constants/validationRegEx";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { TermsOfService } from "@/models/termsOfService";
import { ILogger, IUserProfileService } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";
import PhoneUtil from "@/utility/phoneUtil";
import ValidationUtil from "@/utility/validationUtil";

const smsMaskaOptions = {
    mask: "(###) ###-####",
    eager: true,
};

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

const extractErrorString = (message: string | Ref<string>): string => {
    if (typeof message === "string") {
        return message;
    }
    return message.value;
};

const userProfileService = container.get<IUserProfileService>(
    SERVICE_IDENTIFIER.UserProfileService
);
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const router = useRouter();
const configStore = useConfigStore();
const userStore = useUserStore();
const errorStore = useErrorStore();

const accepted = ref(false);
const email = ref("");
const emailConfirmation = ref("");
const smsNumber = ref("");
const isEmailChecked = ref(true);
const isSMSNumberChecked = ref(true);
const submitStatus = ref("");
const loadingUserData = ref(true);
const loadingTermsOfService = ref(true);
const submittingRegistration = ref(false);
const clientRegistryError = ref(false);
const isValidAge = ref<boolean | null>(null);
const termsOfService = ref<TermsOfService>();

const webClientConfig = computed(() => configStore.webConfig);

const isLoading = computed(
    () =>
        loadingTermsOfService.value ||
        loadingUserData.value ||
        submittingRegistration.value
);

const termsOfServiceLoaded = computed(
    () => !isLoading.value && Boolean(termsOfService.value?.content)
);

const validateEmail = (value: string) => {
    return !isEmailChecked.value || isValidEmail(value);
};

const validations = computed(() => ({
    smsNumber: {
        ...ValidationUtil.getConditionalValidators(isSMSNumberChecked.value, {
            required: required,
            sms: helpers.withMessage(
                "Invalid phone number",
                helpers.withAsync(validPhoneNumberFormat)
            ),
        }),
    },
    email: {
        ...ValidationUtil.getConditionalValidators(isEmailChecked.value, {
            required: required,
            email: helpers.withMessage("Invalid email", validateEmail),
        }),
    },
    emailConfirmation: {
        ...ValidationUtil.getConditionalValidators(isEmailChecked.value, {
            required: required,
            sameAsEmail: helpers.withMessage(
                "Both email addresses must match",
                sameAs(email)
            ),
            email: helpers.withMessage("Invalid email", validateEmail),
        }),
    },
    accepted: { isChecked: sameAs(true) },
}));

const emailErrorMessages = computed(() =>
    v$.value.email.$errors.map((error) => extractErrorString(error.$message))
);

const emailConfirmationErrorMessages = computed(() =>
    v$.value.emailConfirmation.$errors.map((error) =>
        extractErrorString(error.$message)
    )
);

const smsNumberErrorMessages = computed(() =>
    v$.value.smsNumber.$errors.map((error) =>
        extractErrorString(error.$message)
    )
);

const acceptedErrorMessages = computed(() =>
    v$.value.accepted.$errors.map((error) => extractErrorString(error.$message))
);

const v$ = useVuelidate(validations, {
    smsNumber,
    email,
    emailConfirmation,
    accepted,
});

function isValidEmail(emailAddress: string): boolean {
    return ValidationRegEx.Email.test(emailAddress);
}

function loadUserData(): void {
    if (userStore.oidcUserInfo === undefined) {
        errorStore.addError(
            ErrorType.Retrieve,
            ErrorSourceType.User,
            undefined
        );
        return;
    }

    if (userStore.oidcUserInfo.email !== null) {
        email.value = userStore.oidcUserInfo.email;
        emailConfirmation.value = userStore.oidcUserInfo.email;
    }

    loadingUserData.value = true;
    userProfileService
        .validateAge(userStore.oidcUserInfo.hdid)
        .then((valid) => {
            isValidAge.value = valid;
        })
        .catch((err: ResultError) => {
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsError("page");
            } else {
                clientRegistryError.value = true;
                errorStore.addCustomError(
                    "Unable to validate " + ErrorSourceType.User.toLowerCase(),
                    ErrorSourceType.User,
                    undefined
                );
            }
        })
        .finally(() => {
            loadingUserData.value = false;
        });
}

function loadTermsOfService(): void {
    loadingTermsOfService.value = true;
    userProfileService
        .getTermsOfService()
        .then((result) => {
            logger.debug(`getTermsOfService result: ${JSON.stringify(result)}`);
            termsOfService.value = result;
        })
        .catch((err: ResultError) => {
            logger.error(err.message);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("page");
            } else {
                errorStore.addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.TermsOfService,
                    undefined
                );
            }
        })
        .finally(() => {
            loadingTermsOfService.value = false;
        });
}

async function onSubmit(): Promise<void> {
    v$.value.$touch();
    if (v$.value.$invalid || userStore.oidcUserInfo === undefined) {
        submitStatus.value = "ERROR";
        window.scrollTo({ top: 0, behavior: "smooth" });
        return;
    }

    submitStatus.value = "PENDING";
    if (smsNumber.value) {
        smsNumber.value = smsNumber.value.replace(/\D+/g, "");
    }

    try {
        submittingRegistration.value = true;
        await userStore.createProfile({
            hdid: userStore.oidcUserInfo.hdid,
            termsOfServiceId: termsOfService.value?.id ?? "",
            acceptedTermsOfService: accepted.value,
            email: isEmailChecked.value ? (email.value ?? "") : "",
            isEmailVerified: false,
            smsNumber: isSMSNumberChecked.value ? (smsNumber.value ?? "") : "",
            isSMSNumberVerified: false,
            preferences: {},
            lastLoginDateTimes: [],
            notificationSettings: [],
        });

        await router.push({
            path: "home",
            query: { registration: "success" },
        });
    } catch {
        logger.error("Error while registering.");
    } finally {
        submittingRegistration.value = false;
    }
}

function onEmailOptout(isChecked: boolean): void {
    if (!isChecked) {
        emailConfirmation.value = "";
        email.value = "";
    }
}

function onSMSOptout(isChecked: boolean): void {
    if (!isChecked) {
        smsNumber.value = "";
    }
}

loadUserData();
loadTermsOfService();
</script>

<template>
    <LoadingComponent :is-loading="isLoading" />
    <template v-if="termsOfServiceLoaded">
        <v-form
            v-if="isValidAge === true"
            ref="registrationForm"
            @submit.prevent="onSubmit"
        >
            <PageTitleComponent title="Registration" />
            <h4 class="text-h6 text-grey mb-3">
                Communication Preferences (Optional)
            </h4>
            <div>
                <v-checkbox
                    id="emailCheckbox"
                    v-model="isEmailChecked"
                    data-testid="emailCheckbox"
                    label="Email Notifications"
                    @change="onEmailOptout($event)"
                />
                <em class="text-body-2">
                    Receive application and health record updates
                </em>
                <v-text-field
                    id="emailInput"
                    v-model="v$.email.$model"
                    :disabled="!isEmailChecked"
                    data-testid="emailInput"
                    placeholder="Your email address"
                    type="email"
                    :error-messages="emailErrorMessages"
                />
                <v-text-field
                    id="emailConfirmationInput"
                    v-model="v$.emailConfirmation.$model"
                    :disabled="!isEmailChecked"
                    data-testid="emailConfirmationInput"
                    placeholder="Confirm your email address"
                    type="email"
                    :error-messages="emailConfirmationErrorMessages"
                />
            </div>
            <v-divider thickness="3" class="mt-6" />
            <!-- SMS section -->
            <div>
                <v-checkbox
                    id="smsCheckbox"
                    v-model="isSMSNumberChecked"
                    data-testid="sms-checkbox"
                    label="Text Notifications"
                    @change="onSMSOptout($event)"
                />
                <em class="text-body-2">
                    Receive health record updates only
                </em>
                <v-text-field
                    id="smsNumberInput"
                    v-model="v$.smsNumber.$model"
                    v-maska="smsMaskaOptions"
                    :disabled="!isSMSNumberChecked"
                    data-testid="smsNumberInput"
                    placeholder="Your phone number"
                    type="tel"
                    :error-messages="smsNumberErrorMessages"
                />
            </div>
            <div
                v-if="!isEmailChecked && !isSMSNumberChecked"
                class="font-weight-bold text-primary"
            >
                <v-icon
                    aria-hidden="true"
                    class="mr-2"
                    icon="fas fa-exclamation-triangle"
                    size="medium"
                />
                <span
                    >You won't receive notifications from the Health Gateway.
                    You can update this from your Profile Page later.</span
                >
            </div>
            <h4 class="text-h5 font-weight-bold mt-10 mb-2">
                Terms of Service
            </h4>
            <HtmlTextAreaComponent
                :input="termsOfService?.content"
                class="mb-3 overflow-y-auto border"
                height="330px"
            />
            <v-checkbox
                id="accept"
                v-model="accepted"
                class="accept"
                data-testid="acceptCheckbox"
                label="I agree to the terms of service above"
                :error-messages="acceptedErrorMessages"
            />
            <div class="mb-3 text-right">
                <HgButtonComponent
                    :disabled="!accepted"
                    class="px-12"
                    data-testid="registerButton"
                    type="submit"
                    variant="primary"
                    text="Register"
                />
            </div>
        </v-form>
        <div v-else-if="isValidAge === false">
            <PageErrorComponent title="Minimum age required for registration">
                <p
                    data-testid="minimumAgeErrorText"
                    class="text-body-1 text-h5 mt-1 mb-4"
                >
                    You must be
                    <strong>{{ webClientConfig.minPatientAge }}</strong>
                    years of age or older to use this application.
                </p>
                <HgButtonComponent
                    variant="secondary"
                    prepend-icon="fas fa-sign-out-alt"
                    data-testid="registration-logout-button"
                    text="Log Out"
                    to="/logout"
                />
            </PageErrorComponent>
        </div>
        <div v-else-if="clientRegistryError">
            <PageErrorComponent title="Error retrieving user information">
                <p
                    data-testid="clientRegistryErrorText"
                    class="text-body-1 text-h5 mt-1 mb-4"
                >
                    There may be an issue in our Client Registry. Please contact
                    <a
                        href="mailto:HealthGateway@gov.bc.ca"
                        class="text-link font-weight-bold"
                        >HealthGateway@gov.bc.ca</a
                    >
                </p>
                <HgButtonComponent
                    variant="secondary"
                    prepend-icon="fas fa-sign-out-alt"
                    data-testid="registration-logout-button"
                    text="Log Out"
                    to="/logout"
                />
            </PageErrorComponent>
        </div>
        <h1 v-else class="text-h4">Unknown error</h1>
    </template>
</template>
