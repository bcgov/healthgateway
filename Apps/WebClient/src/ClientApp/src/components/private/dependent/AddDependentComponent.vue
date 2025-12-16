<script setup lang="ts">
import { useVuelidate } from "@vuelidate/core";
import { helpers, minLength, required, sameAs } from "@vuelidate/validators";
import { Duration } from "luxon";
import { vMaska } from "maska/vue";
import { computed, nextTick, ref, watch } from "vue";

import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgDatePickerComponent from "@/components/common/HgDatePickerComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import TooManyRequestsComponent from "@/components/error/TooManyRequestsComponent.vue";
import { ActionType } from "@/constants/actionType";
import { Loader } from "@/constants/loader";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import AddDependentRequest from "@/models/addDependentRequest";
import { DateWrapper, IDateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { Action, Text, Type } from "@/plugins/extensions";
import { IDependentService, ITrackingService } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { useDependentStore } from "@/stores/dependent";
import { useErrorStore } from "@/stores/error";
import { useLoadingStore } from "@/stores/loading";
import { useUserStore } from "@/stores/user";
import { phnMask } from "@/utility/masks";
import ValidationUtil from "@/utility/validationUtil";

interface Props {
    disabled?: boolean;
}
withDefaults(defineProps<Props>(), {
    disabled: false,
});

const emit = defineEmits<{
    (e: "handle-submit"): void;
}>();

defineExpose({ hideDialog });

const emptyDependent = {
    firstName: "",
    lastName: "",
    dateOfBirth: "",
    PHN: "",
};

const phnMaskaOptions = {
    mask: phnMask,
    eager: true,
};

const maxBirthdate = DateWrapper.today();

const dependentService = container.get<IDependentService>(
    SERVICE_IDENTIFIER.DependentService
);
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);
const configStore = useConfigStore();
const dependentStore = useDependentStore();
const errorStore = useErrorStore();
const loadingStore = useLoadingStore();
const userStore = useUserStore();

const isVisible = ref(false);
const errorMessage = ref("");
const errorType = ref<ActionType | null>(null);
const accepted = ref(false);
const dependent = ref<AddDependentRequest>({ ...emptyDependent });

const hasValidationErrors = computed(() => {
    v$.value.accepted.$touch();
    return v$.value.$invalid;
});
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
    dependentStore.dependents.some(
        (d) =>
            d.dependentInformation.PHN ===
            dependent.value.PHN.replace(/\s/g, "")
    )
);
const isLoading = computed(() => loadingStore.isLoading(Loader.AddDependent));
const minBirthdate = computed<IDateWrapper>(() =>
    DateWrapper.today().subtract(
        Duration.fromObject({ years: configStore.webConfig.maxDependentAge })
    )
);
const validations = computed(() => ({
    dependent: {
        firstName: {
            required: helpers.withMessage(
                "Given names are required.",
                required
            ),
        },
        lastName: {
            required: helpers.withMessage("Last name is required.", required),
        },
        dateOfBirth: {
            required: helpers.withMessage("Invalid date.", required),
            minLength: helpers.withMessage("Invalid date.", minLength(10)),
            minValue: helpers.withMessage(
                `Dependent must be under the age of
                        ${configStore.webConfig.maxDependentAge}.`,
                (value: string) =>
                    DateWrapper.fromIsoDate(value).isAfterOrSame(
                        minBirthdate.value
                    )
            ),
            maxValue: helpers.withMessage("Invalid Date.", (value: string) =>
                DateWrapper.fromIsoDate(value).isBeforeOrSame(
                    DateWrapper.today()
                )
            ),
        },
        PHN: {
            required: helpers.withMessage("Valid PHN is required.", required),
            minLength: helpers.withMessage(
                "Valid PHN is required.",
                minLength(12)
            ),
            validPersonalHealthNumber: helpers.withMessage(
                "Personal Health Number must be valid.",
                ValidationUtil.validatePhn
            ),
            isNew: helpers.withMessage(
                "This dependent has already been added.",
                () => !isDependentAlreadyAdded.value
            ),
        },
    },
    accepted: { isChecked: sameAs(true) },
}));

const v$ = useVuelidate(validations, { dependent, accepted });

function addDependent(): void {
    loadingStore.applyLoader(
        Loader.AddDependent,
        "addDependent",
        dependentService
            .addDependent(userStore.hdid, {
                ...dependent.value,
                firstName: dependent.value.firstName.replace(/\s+/g, " "),
                lastName: dependent.value.lastName.replace(/\s+/g, " "),
                PHN: dependent.value.PHN.replace(/\D/g, ""),
            })
            .then(async () => {
                errorType.value = null;

                await nextTick();

                hideDialog();
                emit("handle-submit");
            })
            .catch((err: ResultError) => {
                if (err.statusCode === 429) {
                    setTooManyRequestsError("addDependentDialog");
                } else {
                    errorMessage.value = err.message;
                    errorType.value = err.actionCode ?? null;
                }
            })
    );
}

function clear(): void {
    dependent.value = { ...emptyDependent };
    accepted.value = false;
    errorMessage.value = "";
    errorType.value = null;
    v$.value.$reset();
}

function handleSubmit(): void {
    v$.value.$reset();
    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.RegisterDependent,
        type: Type.Dependents,
    });
    addDependent();
}

function hideDialog(): void {
    clear();
    isVisible.value = false;
}

function setTooManyRequestsError(key: string): void {
    errorStore.setTooManyRequestsError(key);
}

function touchDateOfBirth(): void {
    v$.value.dependent.dateOfBirth.$touch();
}

function onAddDependentClick() {
    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.AddDependent,
        type: Type.Dependents,
    });
}

watch(() => dependent.value.dateOfBirth, touchDateOfBirth);
</script>

<template>
    <v-dialog
        id="add-dependent-dialog"
        v-model="isVisible"
        data-testid="add-dependent-dialog"
        scrollable
        width="700px"
        persistent
        no-click-animation
    >
        <template #activator="slotProps">
            <HgButtonComponent
                id="add-dependent-button"
                data-testid="add-dependent-button"
                variant="secondary"
                v-bind="slotProps.props"
                :disabled="disabled"
                prepend-icon="user-plus"
                text="Add dependent"
                @click="onAddDependentClick"
            />
        </template>
        <v-card>
            <v-card-title class="bg-primary text-white px-0">
                <v-toolbar
                    title="Dependent Registration"
                    density="compact"
                    color="primary"
                >
                    <HgIconButtonComponent
                        id="add-dependent-dialog-close-button"
                        data-testid="add-dependent-dialog-close-button"
                        icon="fas fa-close"
                        @click="hideDialog"
                    />
                </v-toolbar>
            </v-card-title>
            <v-card-text class="pa-4" data-testid="new-dependent-modal-form">
                <TooManyRequestsComponent location="addDependentDialog" />
                <HgAlertComponent
                    v-if="isError"
                    data-testid="dependent-error-banner"
                    type="error"
                    variant="outlined"
                >
                    <template #text>
                        <p
                            data-testid="dependent-error-text"
                            class="text-body-1"
                        >
                            <span v-if="isErrorDataMismatch">
                                The information you entered does not match our
                                records. Please try again with the exact given
                                and last names on the BC Services Card.
                            </span>
                            <span v-else-if="isErrorNoHdid">
                                Please ensure you are using a current
                                <a
                                    href="https://www2.gov.bc.ca/gov/content/governments/government-id/bc-services-card"
                                    target="_blank"
                                    rel="noopener"
                                    class="text-link"
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
                            class="text-body-1"
                        >
                            Please contact
                            <a
                                href="mailto:HealthGateway@gov.bc.ca"
                                class="text-link"
                                >HealthGateway@gov.bc.ca</a
                            >.
                        </span>
                        <span
                            v-else
                            data-testid="not-condensed-error-contact-message"
                            class="text-body-1"
                        >
                            If you continue to have issues, please contact
                            <a
                                href="mailto:HealthGateway@gov.bc.ca"
                                class="text-link"
                                >HealthGateway@gov.bc.ca</a
                            >.
                        </span>
                    </template>
                </HgAlertComponent>
                <v-row>
                    <v-col cols="12" sm="6">
                        <label for="firstName">First and Middle Names</label>
                        <v-text-field
                            id="firstName"
                            v-model.trim="dependent.firstName"
                            data-testid="dependent-first-name-input"
                            label="First and Middle Names"
                            autofocus
                            clearable
                            type="text"
                            :error-messages="
                                ValidationUtil.getErrorMessages(
                                    v$.dependent.firstName
                                )
                            "
                            @blur="v$.dependent.firstName.$touch()"
                        />
                    </v-col>
                    <v-col cols="12" sm="6">
                        <label for="lastName">Last Name</label>
                        <v-text-field
                            id="lastName"
                            v-model.trim="dependent.lastName"
                            data-testid="dependent-last-name-input"
                            label="Last Name"
                            clearable
                            type="text"
                            :error-messages="
                                ValidationUtil.getErrorMessages(
                                    v$.dependent.lastName
                                )
                            "
                            @blur="v$.dependent.lastName.$touch()"
                        />
                    </v-col>
                    <v-col cols="12" sm="6">
                        <label for="dateOfBirth">Date of Birth</label>
                        <HgDatePickerComponent
                            id="dateOfBirth"
                            v-model="dependent.dateOfBirth"
                            label="Date of Birth"
                            data-testid="dependent-date-of-birth-input"
                            :max-date="maxBirthdate"
                            :error-messages="
                                ValidationUtil.getErrorMessages(
                                    v$.dependent.dateOfBirth
                                )
                            "
                            @blur="v$.dependent.dateOfBirth.$touch()"
                        />
                    </v-col>
                    <v-col cols="12" sm="6">
                        <label for="phn">PHN</label>
                        <v-text-field
                            id="phn"
                            v-model="dependent.PHN"
                            v-maska="phnMaskaOptions"
                            label="PHN"
                            clearable
                            type="text"
                            data-testid="dependent-phn-input"
                            :error-messages="
                                ValidationUtil.getErrorMessages(
                                    v$.dependent.PHN
                                )
                            "
                            @blur="v$.dependent.PHN.$touch()"
                        />
                    </v-col>
                </v-row>
                <v-row>
                    <v-col>
                        <v-checkbox
                            id="termsCheckbox"
                            v-model="accepted"
                            data-testid="dependent-terms-checkbox"
                            density="compact"
                            color="primary"
                            class="text-body-1 checkbox-top"
                            hide-details
                            :error="
                                ValidationUtil.isValid(v$.dependent) &&
                                !ValidationUtil.isValid(v$.accepted)
                            "
                        >
                            <template #label>
                                <div class="ml-1">
                                    <p>
                                        By providing the child’s name, date of
                                        birth, and personal health number, I
                                        declare that I am the child’s guardian
                                        and that I have the authority to request
                                        and receive health information
                                        respecting the child from third parties.
                                    </p>
                                    <p>
                                        If I either: (a) cease to be guardian of
                                        this child; (b) or lose the right to
                                        request or receive health information
                                        from third parties respecting this
                                        child, I will remove them as a dependent
                                        under my Health Gateway account
                                        immediately.
                                    </p>
                                    <p class="mb-0">
                                        I understand that I will no longer be
                                        able to access my child’s health records
                                        once they are 12 years of age.
                                    </p>
                                </div>
                            </template>
                        </v-checkbox>
                    </v-col>
                </v-row>
            </v-card-text>
            <v-card-actions class="justify-end border-t-sm pa-4">
                <HgButtonComponent
                    data-testid="cancel-dependent-registration-btn"
                    variant="secondary"
                    text="Cancel"
                    @click.prevent="hideDialog"
                />
                <HgButtonComponent
                    data-testid="register-dependent-btn"
                    variant="primary"
                    :loading="isLoading"
                    :disabled="hasValidationErrors"
                    text="Register Dependent"
                    @click.prevent="handleSubmit"
                />
            </v-card-actions>
        </v-card>
        <LoadingComponent :is-loading="isLoading" />
    </v-dialog>
</template>

<style scope>
.checkbox-top.v-input .v-input__control .v-selection-control {
    align-items: start !important;
}
</style>
