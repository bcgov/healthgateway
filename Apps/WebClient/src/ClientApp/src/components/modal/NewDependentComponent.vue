<script setup lang="ts">
import { useVuelidate } from "@vuelidate/core";
import { minLength, required, sameAs } from "@vuelidate/validators";
import { Duration } from "luxon";
import { computed, nextTick, ref, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import DatePickerComponent from "@/components/DatePickerComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import TooManyRequestsComponent from "@/components/TooManyRequestsComponent.vue";
import { ActionType } from "@/constants/actionType";
import AddDependentRequest from "@/models/addDependentRequest";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper, IDateWrapper } from "@/models/dateWrapper";
import { Dependent } from "@/models/dependent";
import { ResultError } from "@/models/errors";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IDependentService } from "@/services/interfaces";
import ValidationUtil from "@/utility/validationUtil";

const emit = defineEmits<{
    (e: "handle-submit"): void;
}>();

defineExpose({ showModal });

const maxBirthdate = new DateWrapper();

const dependentService = container.get<IDependentService>(
    SERVICE_IDENTIFIER.DependentService
);
const store = useStore();

const isVisible = ref(false);
const isLoading = ref(false);
const isDateOfBirthValid = ref(true);
const errorMessage = ref("");
const errorType = ref<ActionType | null>(null);
const accepted = ref(false);
const dependent = ref<AddDependentRequest>({
    firstName: "",
    lastName: "",
    dateOfBirth: "",
    PHN: "",
});

const user = computed<User>(() => store.getters["user/user"]);
const webClientConfig = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);
const dependents = computed<Dependent[]>(
    () => store.getters["dependent/dependents"]
);

const isError = computed(
    () => errorType.value !== null || errorMessage.value.length > 0
);
const isErrorDataMismatch = computed(
    () => errorType.value === ActionType.DataMismatch
);
const isErrorNoHdid = computed(() => errorType.value === ActionType.NoHdId);
const isErrorProtected = computed(
    () => errorType.value === ActionType.Protected
);
const isDependentAlreadyAdded = computed(() =>
    dependents.value.some(
        (d) =>
            d.dependentInformation.PHN ===
            dependent.value.PHN.replace(/\s/g, "")
    )
);
const minBirthdate = computed<IDateWrapper>(() =>
    new DateWrapper().subtract(
        Duration.fromObject({ years: webClientConfig.value.maxDependentAge })
    )
);
const validations = computed(() => ({
    dependent: {
        firstName: {
            required,
        },
        lastName: {
            required,
        },
        dateOfBirth: {
            required,
            minLength: minLength(10),
            minValue: (value: string) =>
                new DateWrapper(value).isAfter(minBirthdate.value),
            maxValue: (value: string) =>
                new DateWrapper(value).isBefore(new DateWrapper()),
        },
        PHN: {
            required,
            minLength: minLength(12),
            validPersonalHealthNumber: ValidationUtil.validatePhn,
            isNew: () => !isDependentAlreadyAdded.value,
        },
    },
    accepted: { isChecked: sameAs(true) },
}));

const v$ = useVuelidate(validations, { dependent, accepted });

function setTooManyRequestsError(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsError", { key });
}

function showModal(): void {
    isVisible.value = true;
}

function hideModal(): void {
    isVisible.value = false;
}

function handleOk(): void {
    v$.value.$touch();
    if (!v$.value.$invalid && isDateOfBirthValid.value) {
        v$.value.$reset();
        addDependent();
    }
}

function addDependent(): void {
    dependentService
        .addDependent(user.value.hdid, {
            ...dependent.value,
            PHN: dependent.value.PHN.replace(/\D/g, ""),
        })
        .then(async () => {
            errorType.value = null;

            await nextTick();

            hideModal();
            emit("handle-submit");
        })
        .catch((err: ResultError) => {
            if (err.statusCode === 429) {
                setTooManyRequestsError("addDependentModal");
            } else {
                errorMessage.value = err.resultMessage;
                errorType.value = err.actionCode ?? null;
            }
        });
}

function clear(): void {
    dependent.value = {
        firstName: "",
        lastName: "",
        dateOfBirth: "",
        PHN: "",
    };
    accepted.value = false;
    errorMessage.value = "";
    errorType.value = null;
    v$.value.$reset();
}

function touchDateOfBirth(): void {
    v$.value.dependent.dateOfBirth.$touch();
}

watch(() => dependent.value.dateOfBirth, touchDateOfBirth);
</script>

<template>
    <b-modal
        id="new-dependent-modal"
        v-model="isVisible"
        data-testid="newDependentModal"
        title="Dependent Registration"
        size="lg"
        header-bg-variant="primary"
        header-text-variant="light"
        centered
        @hidden="clear"
    >
        <TooManyRequestsComponent location="addDependentModal" />
        <b-alert
            data-testid="dependentErrorBanner"
            variant="danger"
            class="no-print"
            :show="isError"
        >
            <p data-testid="dependentErrorText">
                <span v-if="isErrorDataMismatch">
                    The information you entered does not match our records.
                    Please try again with the exact given and last names on the
                    BC Services Card.
                </span>
                <span v-else-if="isErrorNoHdid">
                    Please ensure you are using a current
                    <a
                        href="https://www2.gov.bc.ca/gov/content/governments/government-id/bc-services-card"
                        target="_blank"
                        rel="noopener"
                        >BC Services Card</a
                    >.
                </span>
                <span v-else-if="isErrorProtected">
                    Unable to add dependent.
                </span>
                <span v-else>{{ errorMessage }}</span>
            </p>
            <span
                v-if="isErrorProtected"
                data-testid="condensed-error-contact-message"
            >
                Please contact
                <a href="mailto:HealthGateway@gov.bc.ca"
                    >HealthGateway@gov.bc.ca</a
                >.
            </span>
            <span v-else data-testid="not-condensed-error-contact-message">
                If you continue to have issues, please contact
                <a href="mailto:HealthGateway@gov.bc.ca"
                    >HealthGateway@gov.bc.ca</a
                >.
            </span>
        </b-alert>
        <form data-testid="newDependentModalText">
            <b-row class="mb-2">
                <b-col class="col-12 col-sm-6 mb-2">
                    <label for="firstName">First and Middle Names</label>
                    <b-form-input
                        id="firstName"
                        v-model.trim="dependent.firstName"
                        data-testid="firstNameInput"
                        class="dependentCardInput"
                        placeholder="John Alexander"
                        autofocus
                        :state="ValidationUtil.isValid(v$.dependent.firstName)"
                        @blur.native="v$.dependent.firstName.$touch()"
                    />
                    <b-form-invalid-feedback
                        :state="ValidationUtil.isValid(v$.dependent.firstName)"
                    >
                        Given names are required
                    </b-form-invalid-feedback>
                </b-col>
                <b-col class="col-12 col-sm-6 mb-2">
                    <label for="lastName">Last Name</label>
                    <b-form-input
                        id="lastName"
                        v-model.trim="dependent.lastName"
                        data-testid="lastNameInput"
                        class="dependentCardInput"
                        placeholder="Doe"
                        :state="ValidationUtil.isValid(v$.dependent.lastName)"
                        @blur.native="v$.dependent.lastName.$touch()"
                    />
                    <b-form-invalid-feedback
                        :state="ValidationUtil.isValid(v$.dependent.lastName)"
                    >
                        Last name is required
                    </b-form-invalid-feedback>
                </b-col>
                <b-col class="col-12 col-sm-6 col-lg-4 mb-2">
                    <label for="dateOfBirth">Date of Birth</label>
                    <div>
                        <DatePickerComponent
                            id="dateOfBirth"
                            :value="dependent.dateOfBirth"
                            data-testid="dateOfBirthInput"
                            :max-date="maxBirthdate"
                            :state="
                                ValidationUtil.isValid(v$.dependent.dateOfBirth)
                            "
                            @blur="touchDateOfBirth()"
                            @is-date-valid="isDateOfBirthValid = $event"
                            @update:value="
                                (value) => (dependent.dateOfBirth = value)
                            "
                        />
                    </div>
                    <b-form-invalid-feedback
                        :state="
                            ValidationUtil.isValid(
                                v$.dependent.dateOfBirth,
                                v$.dependent.dateOfBirth.required
                            ) &&
                            ValidationUtil.isValid(
                                v$.dependent.dateOfBirth,
                                v$.dependent.dateOfBirth.minLength
                            ) &&
                            ValidationUtil.isValid(
                                v$.dependent.dateOfBirth,
                                v$.dependent.dateOfBirth.maxValue
                            )
                        "
                    >
                        Invalid date
                    </b-form-invalid-feedback>
                    <b-form-invalid-feedback
                        :state="
                            ValidationUtil.isValid(
                                v$.dependent.dateOfBirth,
                                v$.dependent.dateOfBirth.minValue
                            )
                        "
                    >
                        Dependent must be under the age of
                        {{ webClientConfig.maxDependentAge }}
                    </b-form-invalid-feedback>
                </b-col>
                <b-col class="col-12 col-sm-6 col-lg-4 mb-2">
                    <label for="phn">PHN</label>
                    <div>
                        <b-form-input
                            id="phn"
                            v-model="dependent.PHN"
                            v-mask="'#### ### ###'"
                            data-testid="phnInput"
                            class="dependentCardInput"
                            placeholder="1234 567 890"
                            :state="ValidationUtil.isValid(v$.dependent.PHN)"
                            @blur="v$.dependent.PHN.$touch()"
                        />
                    </div>
                    <b-form-invalid-feedback
                        data-testid="errorDependentAlreadyAdded"
                        :state="
                            ValidationUtil.isValid(
                                v$.dependent.PHN,
                                v$.dependent.PHN.isNew
                            )
                        "
                    >
                        This dependent has already been added
                    </b-form-invalid-feedback>
                    <b-form-invalid-feedback
                        v-if="
                            !ValidationUtil.isValid(
                                v$.dependent.PHN,
                                v$.dependent.PHN.isNew
                            )
                        "
                        :state="ValidationUtil.isValid(v$.dependent.PHN)"
                    >
                        Valid PHN is required
                    </b-form-invalid-feedback>
                </b-col>
            </b-row>
            <b-checkbox
                id="termsCheckbox"
                v-model="accepted"
                data-testid="termsCheckbox"
                :state="ValidationUtil.isValid(v$.accepted)"
            >
                <p>
                    By providing the child’s name, date of birth, and personal
                    health number, I declare that I am the child’s guardian and
                    that I have the authority to request and receive health
                    information respecting the child from third parties.
                </p>
                <p>
                    If I either: (a) cease to be guardian of this child; (b) or
                    lose the right to request or receive health information from
                    third parties respecting this child, I will remove them as a
                    dependent under my Health Gateway account immediately.
                </p>
                <p class="mb-0">
                    I understand that I will no longer be able to access my
                    child’s health records once they are 12 years of age.
                </p>
            </b-checkbox>
        </form>
        <template #modal-footer>
            <b-row>
                <div class="mr-2">
                    <hg-button
                        data-testid="cancelRegistrationBtn"
                        variant="secondary"
                        @click="hideModal"
                        >Cancel</hg-button
                    >
                </div>
                <div>
                    <hg-button
                        data-testid="registerDependentBtn"
                        variant="primary"
                        @click.prevent="handleOk"
                        >Register Dependent</hg-button
                    >
                </div>
            </b-row>
        </template>
        <LoadingComponent :is-loading="isLoading" />
    </b-modal>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.dependentCardDateInput {
    color: #e0e0e0;
}

::placeholder {
    color: #e0e0e0;
}
</style>
