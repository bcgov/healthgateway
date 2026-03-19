<script setup lang="ts">
import { computed } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import TimelineEntryComponent from "@/components/private/timeline/TimelineEntryComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import MedicationTimelineEntry from "@/models/timeline/medicationTimelineEntry";
import { useTimelineStore } from "@/stores/timeline";
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

const timelineStore = useTimelineStore();

const cols = computed(() => timelineStore.columnCount);
const entryIcon = computed<string | undefined>(
    () => entryTypeMap.get(EntryType.Medication)?.icon
);

function formatIngredientStrength(
    strength?: string,
    strengthUnit?: string
): string {
    return `${strength ?? ""}${strengthUnit ?? ""}`;
}

function formatPhone(phoneNumber: string | undefined): string {
    return PhoneUtil.formatPhone(phoneNumber);
}
</script>

<template>
    <TimelineEntryComponent
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        icon-class="bg-primary"
        :title="entry.title"
        :subtitle="entry.subtitle"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
    >
        <template v-if="entry.isPharmacistAssessment">
            <v-row>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="medication-practitioner"
                        name="Practitioner"
                        name-class="font-weight-bold"
                        :value="entry.practitionerSurname"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="medication-din-pin"
                        :name="entry.medication.isPin ? 'PIN' : 'DIN'"
                        name-class="font-weight-bold"
                        :value="entry.medication.din"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="medication-pharmacy-name"
                        name="Pharmacy"
                        name-class="font-weight-bold"
                        :value="entry.pharmacy.name"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="medication-pharmacy-address"
                        name="Address"
                        name-class="font-weight-bold"
                        :value="entry.pharmacy.address"
                    />
                </v-col>
            </v-row>
            <v-divider class="my-4" />
            <div>
                <DisplayFieldComponent
                    data-testid="pharmacist-outcome"
                    name="Outcome"
                    name-class="font-weight-bold"
                    :value="
                        entry.prescriptionProvided
                            ? 'Prescription provided'
                            : 'Prescription not provided'
                    "
                />
                <p
                    v-if="entry.redirectedToHealthCareProvider"
                    data-testid="pharmacist-redirected-to-provider"
                >
                    Advised the patient to seek medical attention from a
                    physician or other health care professional
                </p>
            </div>
        </template>
        <template v-else>
            <v-row>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="medication-practitioner"
                        name="Practitioner"
                        name-class="font-weight-bold"
                        :value="entry.practitionerSurname"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="medication-din-pin"
                        :name="entry.medication.isPin ? 'PIN' : 'DIN'"
                        name-class="font-weight-bold"
                        :value="entry.medication.din"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="medication-quantity"
                        name="Quantity"
                        name-class="font-weight-bold"
                        :value="entry.medication.quantity?.toString()"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="medication-form"
                        name="Form"
                        name-class="font-weight-bold"
                        :value="entry.medication.form"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="medication-manufacturer"
                        name="Manufacturer"
                        name-class="font-weight-bold"
                        :value="entry.medication.manufacturer"
                    />
                </v-col>
            </v-row>
            <v-row
                v-if="entry.medication.activeIngredients?.length"
                class="mt-2"
            >
                <v-col cols="12">
                    <div class="font-weight-bold mb-4">Active ingredients</div>
                    <v-sheet max-width="720">
                        <v-table density="compact" class="border rounded">
                            <thead class="bg-grey-lighten-4">
                                <tr>
                                    <th class="text-left">Ingredient</th>
                                    <th class="text-left">Strength</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr
                                    v-for="(
                                        ingredient, ingredientIndex
                                    ) in entry.medication.activeIngredients"
                                    :key="ingredient.activeIngredientCode"
                                >
                                    <td
                                        :data-testid="`medication-ingredient-${ingredientIndex}`"
                                    >
                                        {{ ingredient.ingredient || "N/A" }}
                                    </td>
                                    <td
                                        class="text-left"
                                        :data-testid="`medication-strength-${ingredientIndex}`"
                                    >
                                        {{
                                            formatIngredientStrength(
                                                ingredient.strength,
                                                ingredient.strengthUnit
                                            )
                                        }}
                                    </td>
                                </tr>
                            </tbody>
                        </v-table>
                    </v-sheet>
                </v-col>
            </v-row>
            <v-row>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="medication-pharmacy-name"
                        name="Filled at"
                        name-class="font-weight-bold"
                        :value="entry.pharmacy.name"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="medication-pharmacy-address"
                        name="Address"
                        name-class="font-weight-bold"
                        :value="entry.pharmacy.address"
                    />
                </v-col>
                <v-col v-if="entry.pharmacy.phoneNumber" :cols="cols">
                    <DisplayFieldComponent
                        data-testid="medication-pharmacy-phone"
                        name="Phone Number"
                        name-class="font-weight-bold"
                        :value="formatPhone(entry.pharmacy.phoneNumber)"
                    />
                </v-col>
                <v-col v-if="entry.pharmacy.faxNumber" :cols="cols">
                    <DisplayFieldComponent
                        data-testid="medication-pharmacy-fax"
                        name="Fax"
                        name-class="font-weight-bold"
                        :value="formatPhone(entry.pharmacy.faxNumber)"
                    />
                </v-col>
            </v-row>
            <v-divider class="my-4" />
            <DisplayFieldComponent
                data-testid="medication-directions"
                name="Directions for use"
                name-class="font-weight-bold"
                :value="entry.directions"
            />
        </template>
    </TimelineEntryComponent>
</template>
