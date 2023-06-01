<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faExclamationTriangle } from "@fortawesome/free-solid-svg-icons";
import { BaseValidation, useVuelidate } from "@vuelidate/core";
import {
    email as emailValidator,
    helpers,
    requiredIf,
    sameAs,
} from "@vuelidate/validators";
import { computed, ref } from "vue";
import { useRouter, useStore } from "vue-composition-wrapper";

import HtmlTextAreaComponent from "@/components/HtmlTextAreaComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import ValidationRegEx from "@/constants/validationRegEx";
import type { WebClientConfiguration } from "@/models/configData";
import { ResultError } from "@/models/errors";
import { TermsOfService } from "@/models/termsOfService";
import type { OidcUserInfo } from "@/models/user";
import { CreateUserRequest } from "@/models/userProfile";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IUserProfileService } from "@/services/interfaces";
import PhoneUtil from "@/utility/phoneUtil";

library.add(faExclamationTriangle);

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

const userProfileService = container.get<IUserProfileService>(
    SERVICE_IDENTIFIER.UserProfileService
);
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const router = useRouter();
const store = useStore();

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

const webClientConfig = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);
const oidcUserInfo = computed<OidcUserInfo | undefined>(
    () => store.getters["user/oidcUserInfo"]
);
const isLoading = computed(
    () =>
        loadingTermsOfService.value ||
        loadingUserData.value ||
        submittingRegistration.value
);
const termsOfServiceLoaded = computed(
    () => !isLoading.value && Boolean(termsOfService.value?.content)
);
const validations = computed(() => ({
    smsNumber: {
        required: requiredIf(() => isSMSNumberChecked.value),
        sms: helpers.withAsync(validPhoneNumberFormat),
    },
    email: {
        required: requiredIf(() => isEmailChecked.value),
        email: emailValidator,
    },
    emailConfirmation: {
        required: requiredIf(() => isEmailChecked.value),
        sameAsEmail: sameAs("email"),
        email: emailValidator,
    },
    accepted: { isChecked: sameAs(() => true) },
}));

const v$ = useVuelidate(validations, {
    smsNumber,
    email,
    emailConfirmation,
    accepted,
});

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

function createProfile(request: CreateUserRequest): Promise<void> {
    return store.dispatch("user/createProfile", { request });
}

function loadUserData(): void {
    if (oidcUserInfo.value === undefined) {
        addError(ErrorType.Retrieve, ErrorSourceType.User, undefined);
        return;
    }

    if (oidcUserInfo.value.email !== null) {
        email.value = oidcUserInfo.value.email;
        emailConfirmation.value = oidcUserInfo.value.email;
    }

    loadingUserData.value = true;
    userProfileService
        .validateAge(oidcUserInfo.value.hdid)
        .then((valid) => {
            isValidAge.value = valid;
        })
        .catch((err: ResultError) => {
            if (err.statusCode === 429) {
                setTooManyRequestsError("page");
            } else {
                clientRegistryError.value = true;
                addCustomError(
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
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                setTooManyRequestsWarning("page");
            } else {
                addError(
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

function isValid(param: BaseValidation): boolean | undefined {
    return param.$dirty === false
        ? undefined
        : !param.$invalid && !param.$pending;
}

async function onSubmit(): Promise<void> {
    v$.value.$touch();
    if (v$.value.$invalid || oidcUserInfo.value === undefined) {
        submitStatus.value = "ERROR";
        return;
    }

    submitStatus.value = "PENDING";
    if (smsNumber.value) {
        smsNumber.value = smsNumber.value.replace(/\D+/g, "");
    }

    try {
        submittingRegistration.value = true;
        await createProfile({
            profile: {
                hdid: oidcUserInfo.value.hdid,
                termsOfServiceId: termsOfService.value?.id || "",
                acceptedTermsOfService: accepted.value,
                email: email.value || "",
                isEmailVerified: false,
                smsNumber: smsNumber.value || "",
                isSMSNumberVerified: false,
                preferences: {},
                lastLoginDateTimes: [],
            },
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
    <div>
        <LoadingComponent :is-loading="isLoading" />
        <b-container v-if="termsOfServiceLoaded">
            <b-form
                v-if="isValidAge === true"
                ref="registrationForm"
                @submit.prevent="onSubmit"
            >
                <page-title title="Registration" />
                <h4 class="subheading mb-3">
                    Communication Preferences (Optional)
                </h4>
                <div class="mb-3">
                    <b-form-checkbox
                        id="emailCheckbox"
                        v-model="isEmailChecked"
                        data-testid="emailCheckbox"
                        @change="onEmailOptout($event)"
                    >
                        Email Notifications
                    </b-form-checkbox>
                    <div>
                        <em class="small">
                            Receive application and health record updates
                        </em>
                    </div>
                    <b-form-input
                        id="emailInput"
                        v-model="v$.email.$model"
                        :disabled="!isEmailChecked"
                        :state="isValid(v$.email)"
                        data-testid="emailInput"
                        placeholder="Your email address"
                        type="email"
                    />
                    <b-form-invalid-feedback :state="isValid(v$.email)">
                        Valid email is required
                    </b-form-invalid-feedback>
                </div>
                <div class="mb-3">
                    <b-form-input
                        id="emailConfirmationInput"
                        v-model="v$.emailConfirmation.$model"
                        :disabled="!isEmailChecked"
                        :state="isValid(v$.emailConfirmation)"
                        data-testid="emailConfirmationInput"
                        placeholder="Confirm your email address"
                        type="email"
                    />
                    <b-form-invalid-feedback
                        :state="v$.emailConfirmation.sameAsEmail"
                    >
                        Emails must match
                    </b-form-invalid-feedback>
                </div>
                <!-- SMS section -->
                <div class="mb-3">
                    <b-form-checkbox
                        id="smsCheckbox"
                        v-model="isSMSNumberChecked"
                        @change="onSMSOptout($event)"
                    >
                        Text Notifications
                    </b-form-checkbox>
                    <div>
                        <em class="small">
                            Receive health record updates only
                        </em>
                    </div>
                    <b-form-input
                        id="smsNumberInput"
                        v-model="v$.smsNumber.$model"
                        v-mask="'(###) ###-####'"
                        :disabled="!isSMSNumberChecked"
                        :state="isValid(v$.smsNumber)"
                        class="d-flex"
                        data-testid="smsNumberInput"
                        placeholder="Your phone number"
                        type="tel"
                    >
                    </b-form-input>
                    <b-form-invalid-feedback
                        v-if="!v$.smsNumber.$pending"
                        :state="isValid(v$.smsNumber)"
                    >
                        Valid sms number is required
                    </b-form-invalid-feedback>
                </div>
                <div
                    v-if="!isEmailChecked && !isSMSNumberChecked"
                    class="font-weight-bold text-primary"
                >
                    <hg-icon
                        aria-hidden="true"
                        class="mr-2"
                        icon="exclamation-triangle"
                        size="medium"
                    />
                    <span
                        >You won't receive notifications from the Health
                        Gateway. You can update this from your Profile Page
                        later.</span
                    >
                </div>
                <h4 class="subheading mt-4">Terms of Service</h4>
                <HtmlTextAreaComponent
                    :input="termsOfService?.content"
                    class="termsOfService mb-3"
                />
                <div class="mb-3">
                    <b-form-checkbox
                        id="accept"
                        v-model="accepted"
                        :state="isValid(v$.accepted)"
                        class="accept"
                        data-testid="acceptCheckbox"
                    >
                        I agree to the terms of service above
                    </b-form-checkbox>
                    <b-form-invalid-feedback :state="isValid(v$.accepted)">
                        You must accept the terms of service.
                    </b-form-invalid-feedback>
                </div>
                <div class="mb-3 text-right">
                    <hg-button
                        :disabled="!accepted"
                        class="px-5"
                        data-testid="registerButton"
                        type="submit"
                        variant="primary"
                        >Register
                    </hg-button>
                </div>
            </b-form>
            <div v-else-if="isValidAge === false">
                <h1>Minimum age required for registration</h1>
                <p data-testid="minimumAgeErrorText">
                    You must be
                    <strong>{{ webClientConfig.minPatientAge }}</strong>
                    years of age or older to use this application
                </p>
            </div>
            <div v-else-if="clientRegistryError">
                <h1>Error retrieving user information</h1>
                <p data-testid="clientRegistryErrorText">
                    There may be an issue in our Client Registry. Please contact
                    <strong
                        ><a href="mailto:HealthGateway@gov.bc.ca"
                            >HealthGateway@gov.bc.ca</a
                        ></strong
                    >
                </p>
            </div>
            <div v-else><h1>Unknown error</h1></div>
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
