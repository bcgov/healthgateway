<script setup lang="ts">
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/shared/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/shared/HgIconButtonComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { ILogger } from "@/services/interfaces";
import { useMedicationStore } from "@/stores/medication";

interface Props {
    hdid: string;
}
const props = defineProps<Props>();

const emit = defineEmits<{
    (e: "abort"): void;
}>();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const medicationStore = useMedicationStore();

const protectiveWord = ref("");
const forceClosed = ref(false);

const medicationsAreLoading = computed(() =>
    medicationStore.medicationsAreLoading(props.hdid)
);

const medicationsAreProtected = computed(() =>
    medicationStore.medicationsAreProtected(props.hdid)
);

const protectiveWordAttempts = computed(() =>
    medicationStore.protectiveWordAttempts(props.hdid)
);

const errorMessages = computed(() =>
    protectiveWordAttempts.value > 1
        ? ["Invalid protective word. Try again."]
        : []
);

const isVisible = computed(
    () =>
        !forceClosed.value &&
        medicationsAreProtected.value &&
        !medicationsAreLoading.value
);

function handleOk(): void {
    fetchMedications();
}

function handleClose(): void {
    forceClosed.value = true;
    emit("abort");
}

function fetchMedications(): void {
    medicationStore
        .retrieveMedications(props.hdid, protectiveWord.value)
        .catch((err: ResultError) =>
            logger.error("Error retrieving medications: " + JSON.stringify(err))
        );
}
</script>

<template>
    <v-dialog
        id="protective-word-modal"
        v-model="isVisible"
        data-testid="protectiveWordModal"
        persistent
        no-click-animation
    >
        <div class="d-flex justify-center">
            <v-card max-width="700px">
                <v-card-title class="bg-primary px-0">
                    <v-toolbar
                        title="Restricted PharmaNet Records"
                        density="compact"
                        color="primary"
                    >
                        <HgIconButtonComponent
                            data-testid="protectiveWordCloseButton"
                            icon="close"
                            @click.prevent="handleClose()"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text class="text-body-1 pa-3">
                    <v-form @submit.stop.prevent="handleOk">
                        <v-text-field
                            id="protectiveWord-input"
                            v-model="protectiveWord"
                            label="Protective Word"
                            data-testid="protectiveWordInput"
                            type="password"
                            :error-messages="errorMessages"
                            required
                        />
                    </v-form>
                </v-card-text>
                <v-card-actions
                    class="d-flex flex-column align-start border-t-sm pa-4"
                >
                    <HgButtonComponent
                        data-testid="protectiveWordContinueBtn"
                        variant="primary"
                        :disabled="!protectiveWord"
                        text="Continue"
                        class="mb-3"
                        @click.prevent="handleOk()"
                    />
                    <p>
                        <small data-testid="protectiveWordModalText">
                            Please enter the protective word required to access
                            these restricted PharmaNet records.
                        </small>
                    </p>
                    <p>
                        <small data-testid="protectiveWordModalMoreInfoText">
                            For more information visit
                            <a
                                data-testid="protectiveWordModalRulesHREF"
                                href="https://www2.gov.bc.ca/gov/content/health/health-drug-coverage/pharmacare-for-bc-residents/pharmanet/protective-word-for-a-pharmanet-record"
                            >
                                protective-word-for-a-pharmanet-record
                            </a>
                        </small>
                    </p>
                </v-card-actions>
            </v-card>
        </div>
    </v-dialog>
</template>
