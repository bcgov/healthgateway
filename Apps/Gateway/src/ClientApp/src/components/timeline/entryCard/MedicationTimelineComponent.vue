<script setup lang="ts">
import { computed } from "vue";

import EntryCardTimelineComponent from "@/components/timeline/entryCard/EntrycardTimelineComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import PhoneUtil from "@/utility/phoneUtil";

interface Props {
    entry: MedicationTimelineEntry;
    index: number;
    datekey: string;
    isMobileDetails?: boolean;
    commentsAreEnabled?: boolean;
}
withDefaults(defineProps<Props>(), {
    isMobileDetails: false,
    commentsAreEnabled: false,
});

const entryIcon = computed<string | undefined>(
    () => entryTypeMap.get(EntryType.Medication)?.icon
);

function formatPhone(phoneNumber: string | undefined): string {
    return PhoneUtil.formatPhone(phoneNumber);
}
</script>

<template>
    <EntryCardTimelineComponent
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        :title="entry.title"
        :subtitle="entry.subtitle"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
    >
        <div slot="details-body">
            <div class="my-2" data-testid="medication-practitioner">
                <strong>Practitioner:</strong>
                {{ entry.practitionerSurname }}
            </div>
            <hr v-if="!entry.isPharmacistAssessment" class="invisible" />
            <div v-if="!entry.isPharmacistAssessment">
                <div class="my-2" data-testid="medication-quantity">
                    <strong>Quantity:</strong>
                    {{ entry.medication.quantity }}
                </div>
                <div class="my-2" data-testid="medication-strength">
                    <strong>Strength:</strong>
                    {{ entry.medication.strength }}
                    {{ entry.medication.strengthUnit }}
                </div>
                <div class="my-2" data-testid="medication-form">
                    <strong>Form:</strong>
                    {{ entry.medication.form }}
                </div>
                <div class="my-2" data-testid="medication-manufacturer">
                    <strong>Manufacturer:</strong>
                    {{ entry.medication.manufacturer }}
                </div>
            </div>
            <div class="my-2" data-testid="medication-din-pin">
                <strong>{{ entry.medication.isPin ? "PIN" : "DIN" }}:</strong>
                {{ entry.medication.din }}
            </div>
            <hr v-if="!entry.isPharmacistAssessment" class="invisible" />
            <div>
                <div class="my-2" data-testid="medication-pharmacy-name">
                    <strong v-if="entry.isPharmacistAssessment">
                        Pharmacy:
                    </strong>
                    <strong v-else>Filled At:</strong>
                    {{ entry.pharmacy.name }}
                </div>
                <div class="my-2" data-testid="medication-pharmacy-address">
                    <strong>Address:</strong>
                    {{ entry.pharmacy.address }}
                </div>
                <div v-if="!entry.isPharmacistAssessment">
                    <div
                        v-if="entry.pharmacy.phoneNumber !== ''"
                        class="my-2"
                        data-testid="medication-pharmacy-phone"
                    >
                        <strong>Phone number:</strong>
                        {{ formatPhone(entry.pharmacy.phoneNumber) }}
                    </div>
                    <div
                        v-if="entry.pharmacy.faxNumber !== ''"
                        class="my-2"
                        data-testid="medication-pharmacy-fax"
                    >
                        <strong>Fax:</strong>
                        {{ formatPhone(entry.pharmacy.faxNumber) }}
                    </div>
                </div>

                <div class="border border-dark mt-4 p-2 small">
                    <div v-if="entry.isPharmacistAssessment">
                        <div data-testid="pharmacist-outcome">
                            <strong>Outcome: </strong>
                            <span
                                data-testid="pharmacist-prescription-provided"
                            >
                                <span v-if="entry.prescriptionProvided">
                                    Prescription provided
                                </span>
                                <span v-else>Prescription not provided</span>
                            </span>
                        </div>
                        <div
                            v-if="entry.redirectedToHealthCareProvider"
                            data-testid="pharmacist-redirected-to-provider"
                            class="pt-2"
                        >
                            Advised the patient to seek medical attention from a
                            physician or other health care professional
                        </div>
                    </div>
                    <div v-else data-testid="medication-directions">
                        <div>
                            <strong>Directions for Use:</strong>
                        </div>
                        <div class="pt-2">
                            {{ entry.directions }}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </EntryCardTimelineComponent>
</template>
