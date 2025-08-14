<script setup lang="ts">
import useVuelidate from "@vuelidate/core";
import { helpers, not, sameAs } from "@vuelidate/validators";
import { computed, ref, watch } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import SectionHeaderComponent from "@/components/common/SectionHeaderComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { Loader } from "@/constants/loader";
import ValidationRegEx from "@/constants/validationRegEx";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { isTooManyRequestsError } from "@/models/errors";
import { ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useLoadingStore } from "@/stores/loading";
import { useUserStore } from "@/stores/user";
import ValidationUtil from "@/utility/validationUtil";

const emit = defineEmits<{
    (e: "email-updated", value: string): void;
}>();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const errorStore = useErrorStore();
const loadingStore = useLoadingStore();
const userStore = useUserStore();

const isEmailEditable = ref(false);
const inputValue = ref(userStore.user.email);

const email = computed(() => userStore.user.email);
const emailVerified = computed(() => userStore.user.verifiedEmail);
const emailVerificationSent = computed(() => userStore.user.verifiedEmail);
const inputErrorMessages = computed(() =>
    !isEmailEditable.value
        ? []
        : ValidationUtil.getErrorMessages(v$.value.inputValue)
);
const validations = computed(() => ({
    inputValue: {
        newEmail: helpers.withMessage(
            "New email must be different from the previous one",
            not(sameAs(email))
        ),
        email: helpers.withMessage("Valid email is required", validateEmail),
    },
}));

const v$ = useVuelidate(validations, { inputValue });

function validateEmail(emailAddress: string): boolean {
    return (
        !emailAddress ||
        emailAddress.length == 0 ||
        ValidationRegEx.Email.test(emailAddress)
    );
}

function makeEmailEditable(): void {
    isEmailEditable.value = true;
    v$.value.inputValue.$touch();
}

function cancelEmailEdit(): void {
    isEmailEditable.value = false;
    inputValue.value = email.value;
    v$.value.$reset();
}

function saveEmailEdit(): void {
    v$.value.$touch();
    if (v$.value.inputValue.$invalid !== true) {
        logger.debug(`saveEmailEdit: ${inputValue.value}`);
        sendUserEmailUpdate();
    }
}

function sendUserEmailUpdate(): void {
    loadingStore.applyLoader(
        Loader.UserProfile,
        "updateUserEmail",
        userStore
            .updateUserEmail(inputValue.value)
            .then(() => {
                userStore
                    .retrieveProfile()
                    .then(() => {
                        isEmailEditable.value = false;
                        v$.value.$reset();
                        emit("email-updated", email.value);
                    })
                    .catch((error) => {
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
            })
            .catch((err) => {
                logger.error(err);
                if (err.statusCode === 429) {
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

watch(email, (value) => (inputValue.value = value));
</script>

<template>
    <SectionHeaderComponent title="Email Address">
        <template #append>
            <HgButtonComponent
                id="editEmail"
                data-testid="editEmailBtn"
                class="ml-2"
                :class="{ invisible: isEmailEditable }"
                variant="link"
                text="Edit"
                @click="makeEmailEditable"
            />
        </template>
    </SectionHeaderComponent>
    <v-sheet :max-width="400">
        <v-text-field
            v-model.trim="inputValue"
            :class="{ 'mb-4': inputErrorMessages.length > 0 }"
            data-testId="email-input"
            type="email"
            label="Email Address"
            :placeholder="isEmailEditable ? 'Your email address' : 'Empty'"
            persistent-placeholder
            :readonly="!isEmailEditable"
            :clearable="isEmailEditable"
            :error-messages="inputErrorMessages"
            @blur="v$.inputValue.$touch()"
        />
    </v-sheet>
    <div v-if="isEmailEditable" class="mb-4">
        <HgAlertComponent
            v-if="!inputValue && email"
            data-testid="emailOptOutMessage"
            class="pt-0"
            type="error"
            variant="text"
            text="Removing your email address will disable future email
                communications from the Health Gateway."
        />
        <HgButtonComponent
            id="editEmailCancelBtn"
            data-testid="editEmailCancelBtn"
            class="mr-2"
            variant="secondary"
            text="Cancel"
            @click="cancelEmailEdit"
        />
        <HgButtonComponent
            id="editEmailSaveBtn"
            data-testid="editEmailSaveBtn"
            class="mx-2"
            variant="primary"
            text="Save"
            :disabled="
                inputValue === email ||
                !ValidationUtil.isValid(v$.inputValue, undefined, false)
            "
            @click="saveEmailEdit"
        />
    </div>
    <template v-else>
        <div id="emailStatus" data-testid="emailStatus" class="mb-4">
            <DisplayFieldComponent
                v-if="emailVerified"
                name="Status"
                value="Verified"
                value-class="text-success"
                data-testid="emailStatusVerified"
                horizontal
            />
            <DisplayFieldComponent
                v-else-if="!email"
                name="Status"
                value="Opted Out"
                value-class="text-medium-emphasis"
                data-testid="emailStatusOptedOut"
                horizontal
            />
            <DisplayFieldComponent
                v-else
                name="Status"
                value="Not Verified"
                value-class="text-error"
                data-testid="emailStatusNotVerified"
                horizontal
            />
        </div>
        <HgButtonComponent
            v-if="!emailVerified && email"
            id="resendEmail"
            data-testid="resendEmailBtn"
            class="mb-4"
            variant="secondary"
            text="Resend Verification"
            :disabled="emailVerificationSent"
            @click="sendUserEmailUpdate"
        />
    </template>
</template>
