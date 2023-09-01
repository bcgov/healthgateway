<script setup lang="ts">
import { computed, ref } from "vue";
import { VDialog } from "vuetify/lib/components/index.mjs";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import TooManyRequestsComponent from "@/components/error/TooManyRequestsComponent.vue";
import { VaccinationState } from "@/constants/vaccinationState";
import { DateWrapper } from "@/models/dateWrapper";
import { CustomBannerError } from "@/models/errors";
import VaccinationStatus from "@/models/vaccinationStatus";

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

const emit = defineEmits<{
    (e: "click-previous-button"): void;
    (e: "click-next-button"): void;
}>();

defineExpose({ showDialog });

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
        .filter((name) => name.length > 0)
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
    <div class="bg-primary text-white rounded-t pa-4">
        <h1
            data-testid="formTitleVaccineCard"
            class="text-h5 font-weight-bold text-center"
        >
            BC Vaccine Card
        </h1>
        <v-divider
            class="border-opacity-100 my-3"
            color="accent"
            :thickness="2"
            role="presentation"
        />
        <p class="text-body-1 text-center">{{ name }}</p>
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
        <v-col v-if="includePreviousButton" cols="auto" class="d-print-none">
            <HgButtonComponent
                data-testid="vc-chevron-left-btn"
                class="py-2 h-100 rounded-0"
                variant="transparent"
                size="x-small"
                @click="onClickPreviousButton"
            >
                <v-icon icon="chevron-left" size="x-large" />
            </HgButtonComponent>
        </v-col>
        <v-col
            class="pa-4 d-flex align-center justify-center flex-column text-center text-white"
        >
            <h2
                v-if="isFullyVaccinated"
                aria-label="Status Vaccinated"
                data-testid="statusVaccinated"
                class="text-h5 font-weight-bold"
            >
                <v-icon icon="check-circle" class="mr-2" />
                <span>Vaccinated</span>
            </h2>
            <h2
                v-else-if="isPartiallyVaccinated"
                aria-label="Status Partially Vaccinated"
                data-testid="statusPartiallyVaccinated"
                class="text-h5 font-weight-bold"
            >
                Partially Vaccinated
            </h2>
            <h2
                v-else-if="isVaccinationNotFound"
                aria-label="Status Not Found"
                data-testid="statusNotFound"
                class="text-h5 font-weight-bold"
            >
                Not Found
            </h2>
            <div
                v-if="
                    issuedDate && (isFullyVaccinated || isPartiallyVaccinated)
                "
                class="mt-4 text-body-2"
            >
                Issued on {{ issuedDate }}
            </div>
            <div
                v-if="qrCodeUrl !== null && !isVaccinationNotFound"
                aria-label="QR Code Image"
                class="text-center"
            >
                <img
                    :src="qrCodeUrl"
                    class="d-none d-sm-block ma-2"
                    alt="BC Vaccine Card QR"
                />
                <img
                    :src="qrCodeUrl"
                    class="d-sm-none ma-2"
                    alt="BC Vaccine Card QR"
                    @click="showDialog()"
                />
                <p
                    class="d-sm-none d-print-none text-body-1"
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
                        <v-card-title class="px-0">
                            <v-toolbar
                                title="Present for scanning"
                                density="compact"
                                color="white"
                            >
                                <HgIconButtonComponent
                                    icon="close"
                                    aria-label="Close"
                                    @click="hideDialog"
                                />
                            </v-toolbar>
                        </v-card-title>
                        <v-card-text class="pa-0">
                            <img
                                :src="qrCodeUrl"
                                class="big-qr-code img-fluid"
                                alt="BC Vaccine Card QR"
                            />
                        </v-card-text>
                        <v-card-actions class="justify-center border-t-sm pa-4">
                            <HgButtonComponent
                                variant="secondary"
                                data-testid="qr-code-close-dialog-btn"
                                text="Close Dialog"
                                @click="hideDialog"
                            />
                        </v-card-actions>
                    </v-card>
                </v-dialog>
            </div>
        </v-col>
        <v-col v-if="includeNextButton" cols="auto" class="d-print-none">
            <HgButtonComponent
                data-testid="vc-chevron-right-btn"
                variant="transparent"
                class="py-2 h-100 rounded-0"
                size="x-small"
                @click="onClickNextButton"
            >
                <v-icon icon="chevron-right" size="x-large" />
            </HgButtonComponent>
        </v-col>
    </v-row>
    <TooManyRequestsComponent
        class="mx-4 mt-4"
        location="vaccineCardComponent"
    />
    <v-alert
        v-if="error !== undefined"
        class="d-print-none mx-4 mt-4"
        closable
        close-label="Close"
        type="error"
        title="Our Apologies"
        text="An unexpected error occured while processing the request, please try again."
        variant="outlined"
        border
    >
        <template #text>
            <p data-testid="errorTextDescription" class="text-body-1">
                We've found an issue and the Health Gateway team is working hard
                to fix it.
            </p>
        </template>
    </v-alert>
</template>

<style lang="scss" scoped>
.vaccination-result {
    min-height: 354px;
}

.big-qr-code {
    width: 100%;
}
</style>
