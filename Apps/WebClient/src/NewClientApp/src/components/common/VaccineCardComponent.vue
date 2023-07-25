<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faChevronLeft,
    faChevronRight,
    faHandPointer,
} from "@fortawesome/free-solid-svg-icons";
import { computed, ref } from "vue";
import { VDialog } from "vuetify/lib/components/index.mjs";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import TooManyRequestsComponent from "@/components/error/TooManyRequestsComponent.vue";
import { VaccinationState } from "@/constants/vaccinationState";
import { DateWrapper } from "@/models/dateWrapper";
import { CustomBannerError } from "@/models/errors";
import VaccinationStatus from "@/models/vaccinationStatus";

library.add(faCheckCircle, faChevronLeft, faChevronRight, faHandPointer);

defineExpose({
    showDialog,
});

const emit = defineEmits<{
    (e: "click-previous-button"): void;
    (e: "click-next-button"): void;
}>();

interface Props {
    showGenericSaveInstructions: boolean;
    status?: VaccinationStatus;
    error?: CustomBannerError;
    isLoading?: boolean;
    includePreviousButton?: boolean;
    includeNextButton?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    status: undefined,
    error: undefined,
    isLoading: false,
    includePreviousButton: false,
    includeNextButton: false,
});

const isFullyVaccinated = computed(
    () => props.status?.state === VaccinationState.FullyVaccinated
);

const isPartiallyVaccinated = computed(
    () => props.status?.state === VaccinationState.PartiallyVaccinated
);

const isVaccinationNotFound = computed(
    () => props.status?.state === VaccinationState.NotFound
);

const visible = ref(false);

const name = computed(() =>
    [props.status?.firstname ?? "", props.status?.lastname ?? ""]
        .filter((name) => name?.length > 0)
        .join(" ")
);

const qrCodeUrl = computed<string | null>(() => {
    const qrCode = props.status?.qrCode;
    if (qrCode?.mediaType && qrCode?.encoding && qrCode?.data) {
        return `data:${qrCode.mediaType};${qrCode.encoding},${qrCode.data}`;
    } else {
        return null;
    }
});

const issuedDate = computed<string | undefined>(() => {
    if (props.status?.issueddate) {
        return new DateWrapper(props.status.issueddate, {
            hasTime: true,
        }).format("MMMM-dd-yyyy, HH:mm");
    }
    return undefined;
});

function onClickPreviousButton(): void {
    emit("click-previous-button");
}

function onClickNextButton(): void {
    emit("click-next-button");
}

function showDialog(): void {
    visible.value = true;
}

function hideDialog(): void {
    visible.value = false;
}
</script>

<template>
    <div>
        <div class="header bg-primary text-white">
            <div class="pa-4 pt-0 pt-sm-4">
                <h3 data-testid="formTitleVaccineCard" class="text-center">
                    BC Vaccine Card
                </h3>
                <v-divider
                    :thickness="2"
                    class="border-opacity-100 my-3"
                    color="accent"
                >
                </v-divider>

                <div class="text-center">{{ name }}</div>
            </div>
        </div>
        <v-row
            no-gutters
            class="vaccination-result"
            :class="{
                'bg-success': isFullyVaccinated,
                'bg-secondary': isPartiallyVaccinated,
                'bg-grey-darken-2': isVaccinationNotFound,
            }"
        >
            <v-col
                v-if="includePreviousButton"
                cols="auto"
                class="d-print-none"
            >
                <HgButtonComponent
                    class="h-100 bg-secondary px-4"
                    data-testid="vc-chevron-left-btn"
                    @click="onClickPreviousButton"
                >
                    <v-icon icon="chevron-left" size="large" />
                </HgButtonComponent>
            </v-col>
            <v-col>
                <div class="justify-content-between">
                    <div
                        class="pa-4 flex-grow-1 d-flex align-center justify-center flex-column text-center text-white"
                    >
                        <h2
                            v-if="isFullyVaccinated"
                            aria-label="Status Vaccinated"
                            data-testid="statusVaccinated"
                            class="d-flex align-items-center ma-0"
                        >
                            <v-icon
                                v-show="isFullyVaccinated"
                                icon="check-circle"
                                size="extra-large"
                                class="mr-2"
                            />
                            <span>Vaccinated</span>
                        </h2>
                        <h2
                            v-else-if="isPartiallyVaccinated"
                            aria-label="Status Partially Vaccinated"
                            data-testid="statusPartiallyVaccinated"
                            class="ma-0"
                        >
                            Partially Vaccinated
                        </h2>
                        <h2
                            v-else-if="isVaccinationNotFound"
                            aria-label="Status Not Found"
                            data-testid="statusNotFound"
                            class="ma-0"
                        >
                            Not Found
                        </h2>
                        <small
                            v-if="
                                issuedDate !== undefined &&
                                (isFullyVaccinated || isPartiallyVaccinated)
                            "
                            class="mt-4"
                        >
                            Issued on {{ issuedDate }}
                        </small>
                        <div
                            v-if="qrCodeUrl !== null && !isVaccinationNotFound"
                            aria-label="QR Code Image"
                            class="qr-code-container text-center"
                        >
                            <img
                                :src="qrCodeUrl"
                                class="d-sm-none small-qr-code img-fluid ma-2"
                                alt="BC Vaccine Card QR"
                                @click="showDialog()"
                            />
                            <img
                                :src="qrCodeUrl"
                                class="d-none d-sm-block small-qr-code img-fluid ma-2"
                                alt="BC Vaccine Card QR"
                            />
                            <p
                                class="d-sm-none d-print-none ma-0"
                                @click="showDialog()"
                            >
                                <v-icon icon="hand-pointer" class="mr-2" />
                                <span>Tap to zoom in</span>
                            </p>
                            <v-dialog
                                id="qr-code-dialog"
                                v-model="visible"
                                title="Present for scanning"
                                data-testid="qr-code-dialog"
                                persistent
                                no-click-animation
                            >
                                <v-card>
                                    <img
                                        :src="qrCodeUrl"
                                        class="big-qr-code img-fluid"
                                        alt="BC Vaccine Card QR"
                                    />
                                    <v-card-actions>
                                        <HgButtonComponent
                                            variant="secondary"
                                            aria-label="Close"
                                            data-testid="qr-code-close-dialog-btn"
                                            @click="hideDialog()"
                                        >
                                            Close Dialog
                                        </HgButtonComponent>
                                    </v-card-actions>
                                </v-card>
                            </v-dialog>
                        </div>
                    </div>
                </div>
            </v-col>
            <v-col v-if="includeNextButton" cols="auto" class="d-print-none">
                <HgButtonComponent
                    class="h-100 bg-secondary px-4"
                    data-testid="vc-chevron-right-btn"
                    @click="onClickNextButton"
                >
                    <v-icon icon="chevron-right" size="large" />
                </HgButtonComponent>
            </v-col>
        </v-row>
        <TooManyRequestsComponent location="vaccineCardComponent" />
        <div v-if="error !== undefined" class="container d-print-none">
            <v-alert
                v-if="error !== undefined"
                type="error"
                icon="circle-exclamation"
                variant="text"
            >
                <template #text>
                    <h4>Our Apologies</h4>
                    <div data-testid="errorTextDescription" class="pl-6">
                        We've found an issue and the Health Gateway team is
                        working hard to fix it.
                    </div>
                </template>
            </v-alert>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.header {
    border-top-left-radius: 0.25rem;
    border-top-right-radius: 0.25rem;
}

.vaccination-result {
    min-height: 348px;
}

.big-qr-code {
    width: 100%;
}
</style>
