<script setup lang="ts">
import useVuelidate from "@vuelidate/core";
import {
    email as emailValidator,
    helpers,
    maxLength,
    minLength,
    required,
} from "@vuelidate/validators";
import { computed } from "vue";
import { ref } from "vue";

import { useDelegateStore } from "@/stores/delegate";
import ValidationUtil from "@/utility/validationUtil";

defineExpose({ saveStep });

const delegateStore = useDelegateStore();

const delegateEmail = ref("");
const delegateNickname = ref("");

const validations = computed(() => ({
    delegateEmail: {
        email: helpers.withMessage(
            "Please enter a valid email address",
            emailValidator
        ),
        required,
    },
    delegateNickname: {
        required,
        minLength: minLength(2),
        maxLength: maxLength(50),
    },
}));
const emailErrorMessages = computed(() =>
    ValidationUtil.getErrorMessages(v$.value.delegateEmail)
);
const nicknameErrorMessages = computed(() =>
    ValidationUtil.getErrorMessages(v$.value.delegateNickname)
);

const v$ = useVuelidate(validations, { delegateEmail, delegateNickname });

function saveStep(): void {
    v$.value.$touch();
    if (v$.value.$invalid) return;
    delegateStore.captureInvitationContact(
        delegateEmail.value,
        delegateNickname.value
    );
}

// INIT
if (delegateStore.invitationWizardState?.nickname !== undefined) {
    delegateNickname.value = delegateStore.invitationWizardState.nickname;
}
if (delegateStore.invitationWizardState?.email !== undefined) {
    delegateEmail.value = delegateStore.invitationWizardState.email;
}
</script>

<template>
    <v-row data-testid="invitation-contact-step">
        <v-col cols="12">
            <h5 class="text-h6 font-weight-bold mb-4">Share records with:</h5>
            <p class="text-body-1">
                You can always change what is shared or stop sharing. If you
                stop sharing, they won't be able to access your health records.
            </p>
        </v-col>
        <v-col cols="12" md="6">
            <label for="delegate-email">Email Address</label>
            <v-text-field
                id="delegate-email"
                v-model.trim="delegateEmail"
                data-testId="delegate-email-input"
                type="email"
                placeholder="e.g. email@example.com"
                persistent-placeholder
                :error-messages="emailErrorMessages"
                clearable
                @blur="v$.delegateEmail.$touch()"
            />
        </v-col>
        <v-col cols="12" md="6">
            <label for="delegate-nickname">Nickname for Receiver</label>
            <v-text-field
                id="delegate-nickname"
                v-model.trim="delegateNickname"
                data-testId="delegate-nickname-input"
                placeholder="e.g. Clair"
                :error-messages="nicknameErrorMessages"
                clearable
                @blur="v$.delegateNickname.$touch()"
            />
        </v-col>
        <v-col cols="12">
            <p class="text-body-1">
                <b>Note:</b> In order for the other users to be able to see your
                shared records, they must be a Health Gateway user.
            </p>
        </v-col>
    </v-row>
</template>
