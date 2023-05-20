<script setup lang="ts">
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import { ResultError } from "@/models/errors";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

const store = useStore();

interface Props {
    hdid: string;
}
const props = defineProps<Props>();

function retrieveMedications(params: {
    hdid: string;
    protectiveWord?: string;
}): Promise<RequestResult<MedicationStatementHistory[]>> {
    return store.dispatch("medication/retrieveMedications", params);
}

const medicationsAreLoading = computed<boolean>(() =>
    store.getters["medication/medicationsAreLoading"](props.hdid)
);

const medicationsAreProtected = computed<boolean>(() =>
    store.getters["medication/medicationsAreProtected"](props.hdid)
);

const protectiveWordAttempts = computed<number>(() =>
    store.getters["medication/protectiveWordAttempts"](props.hdid)
);

const protectiveWord = ref("");
const isDismissed = ref(false);

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

const isVisible = computed<boolean>(() => {
    return (
        medicationsAreProtected.value &&
        !medicationsAreLoading.value &&
        !isDismissed.value
    );
});

const error = computed<boolean>(() => {
    return protectiveWordAttempts.value > 1;
});

function handleOk(bvModalEvt: Event): void {
    // Prevent modal from closing
    bvModalEvt.preventDefault();
    fetchMedications();
}

function fetchMedications(): void {
    retrieveMedications({
        hdid: props.hdid,
        protectiveWord: protectiveWord.value,
    }).catch((err: ResultError) =>
        logger.error("Error retrieving medications: " + JSON.stringify(err))
    );
}
</script>

<template>
    <b-modal
        id="protective-word-modal"
        v-model="isVisible"
        data-testid="protectiveWordModal"
        title="Restricted PharmaNet Records"
        header-bg-variant="primary"
        header-text-variant="light"
        centered
    >
        <b-row>
            <b-col>
                <form @submit.stop.prevent="handleOk">
                    <b-row>
                        <b-col cols="8">
                            <label for="protectiveWord-input"
                                >Protective Word
                            </label>
                            <b-form-input
                                id="protectiveWord-input"
                                v-model="protectiveWord"
                                data-testid="protectiveWordInput"
                                type="password"
                                required
                            />
                        </b-col>
                    </b-row>
                    <b-row
                        v-if="error"
                        data-testid="protectiveWordModalErrorText"
                    >
                        <b-col>
                            <span class="text-danger"
                                >Invalid protective word. Try again.</span
                            >
                        </b-col>
                    </b-row>
                </form>
            </b-col>
        </b-row>
        <template #modal-footer>
            <b-row>
                <b-col>
                    <b-row>
                        <b-col>
                            <hg-button
                                data-testid="protectiveWordContinueBtn"
                                size="lg"
                                variant="primary"
                                :disabled="!protectiveWord"
                                @click="handleOk($event)"
                            >
                                Continue
                            </hg-button>
                        </b-col>
                    </b-row>
                    <br />
                    <b-row data-testid="protectiveWordModalText">
                        <b-col>
                            <small>
                                Please enter the protective word required to
                                access these restricted PharmaNet records.
                            </small>
                        </b-col>
                    </b-row>
                    <b-row data-testid="protectiveWordModalMoreInfoText">
                        <b-col>
                            <small>
                                For more information visit
                                <a
                                    data-testid="protectiveWordModalRulesHREF"
                                    href="https://www2.gov.bc.ca/gov/content/health/health-drug-coverage/pharmacare-for-bc-residents/pharmanet/protective-word-for-a-pharmanet-record"
                                    >protective-word-for-a-pharmanet-record</a
                                >
                            </small>
                        </b-col>
                    </b-row>
                </b-col>
            </b-row>
        </template>
    </b-modal>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.modal-footer {
    justify-content: flex-start;

    button {
        padding: 5px 20px 5px 20px;
    }
}
</style>
