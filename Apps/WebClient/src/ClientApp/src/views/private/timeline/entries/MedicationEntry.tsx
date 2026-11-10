import { View } from "react-native";

import { AppText, DisplayField } from "@/components/common";
import { EntryType, getEntryTypeDetails } from "@/constants/entryType";
import type { MedicationTimelineEntry } from "@/models/timeline/medicationTimelineEntry";
import { formatPhone } from "@/utils/validation";

import { TimelineEntryCard } from "../TimelineEntryCard";
import type { TimelineEntryProps } from "./entryProps";

/** One full-width v-col (timelineStore.columnCount is 12). */
function Field(props: React.ComponentProps<typeof DisplayField>) {
    return (
        <View className="py-3">
            <DisplayField nameClass="font-bold" {...props} />
        </View>
    );
}

const Divider = () => <View className="h-px bg-border-divider my-4" />;

const formatIngredientStrength = (strength?: string, strengthUnit?: string) =>
    `${strength ?? ""}${strengthUnit ?? ""}`;

/** Port of entry/MedicationTimelineComponent.vue */
export function MedicationEntry({
    entry: timelineEntry,
    commentsAreEnabled = false,
    isMobileDetails = false,
    testID,
}: TimelineEntryProps) {
    const entry = timelineEntry as MedicationTimelineEntry;
    const { medication, pharmacy } = entry;
    const dinName = medication.isPin ? "PIN" : "DIN";
    return (
        <TimelineEntryCard
            testID={testID}
            entry={entry}
            entryIcon={getEntryTypeDetails(EntryType.Medication)?.icon}
            iconClass="bg-primary"
            title={entry.title}
            subtitle={entry.subtitle}
            isMobileDetails={isMobileDetails}
            allowComment={commentsAreEnabled}
        >
            {entry.isPharmacistAssessment ? (
                <View>
                    <View className="-my-3">
                        <Field
                            testID="medication-practitioner"
                            name="Practitioner"
                            value={entry.practitionerSurname}
                        />
                        <Field
                            testID="medication-din-pin"
                            name={dinName}
                            value={medication.din}
                        />
                        <Field
                            testID="medication-pharmacy-name"
                            name="Pharmacy"
                            value={pharmacy.name}
                        />
                        <Field
                            testID="medication-pharmacy-address"
                            name="Address"
                            value={pharmacy.address}
                        />
                    </View>
                    <Divider />
                    <View>
                        <DisplayField
                            testID="pharmacist-outcome"
                            nameClass="font-bold"
                            name="Outcome"
                            value={
                                entry.prescriptionProvided
                                    ? "Prescription provided"
                                    : "Prescription not provided"
                            }
                        />
                        {entry.redirectedToHealthCareProvider ? (
                            <AppText testID="pharmacist-redirected-to-provider">
                                Advised the patient to seek medical attention
                                from a physician or other health care
                                professional
                            </AppText>
                        ) : null}
                    </View>
                </View>
            ) : (
                <View>
                    <View className="-my-3">
                        <Field
                            testID="medication-practitioner"
                            name="Practitioner"
                            value={entry.practitionerSurname}
                        />
                        <Field
                            testID="medication-din-pin"
                            name={dinName}
                            value={medication.din}
                        />
                        <Field
                            testID="medication-quantity"
                            name="Quantity"
                            value={medication.quantity?.toString()}
                        />
                        <Field
                            testID="medication-form"
                            name="Form"
                            value={medication.form}
                        />
                        <Field
                            testID="medication-manufacturer"
                            name="Manufacturer"
                            value={medication.manufacturer}
                        />
                    </View>
                    {medication.activeIngredients?.length ? (
                        <View className="mt-5 py-3">
                            <AppText className="font-bold mb-4">
                                Active ingredients
                            </AppText>
                            {/* v-sheet max-width 720 › v-table compact, bordered and rounded */}
                            <View className="max-w-[720px] border border-border-light rounded overflow-hidden">
                                <View className="flex-row bg-table-header min-h-10 items-center">
                                    <AppText
                                        variant="body-medium"
                                        className="flex-1 px-4"
                                    >
                                        Ingredient
                                    </AppText>
                                    <AppText
                                        variant="body-medium"
                                        className="flex-1 px-4"
                                    >
                                        Strength
                                    </AppText>
                                </View>
                                {medication.activeIngredients.map(
                                    (ingredient, i) => (
                                        <View
                                            key={`${ingredient.activeIngredientCode}-${i}`}
                                            className="flex-row min-h-[36px] items-center border-t border-border-divider"
                                        >
                                            <AppText
                                                testID={`medication-ingredient-${i}`}
                                                variant="body-medium"
                                                className="flex-1 px-4"
                                            >
                                                {ingredient.ingredient || "N/A"}
                                            </AppText>
                                            <AppText
                                                testID={`medication-strength-${i}`}
                                                variant="body-medium"
                                                className="flex-1 px-4"
                                            >
                                                {formatIngredientStrength(
                                                    ingredient.strength,
                                                    ingredient.strengthUnit
                                                )}
                                            </AppText>
                                        </View>
                                    )
                                )}
                            </View>
                        </View>
                    ) : null}
                    <View className="py-3">
                        <Field
                            testID="medication-pharmacy-name"
                            name="Filled at"
                            value={pharmacy.name}
                        />
                        <Field
                            testID="medication-pharmacy-address"
                            name="Address"
                            value={pharmacy.address}
                        />
                        {pharmacy.phoneNumber ? (
                            <Field
                                testID="medication-pharmacy-phone"
                                name="Phone Number"
                                value={formatPhone(pharmacy.phoneNumber)}
                            />
                        ) : null}
                        {pharmacy.faxNumber ? (
                            <Field
                                testID="medication-pharmacy-fax"
                                name="Fax"
                                value={formatPhone(pharmacy.faxNumber)}
                            />
                        ) : null}
                    </View>
                    <Divider />
                    <DisplayField
                        testID="medication-directions"
                        nameClass="font-bold"
                        name="Directions for use"
                        value={entry.directions}
                    />
                </View>
            )}
        </TimelineEntryCard>
    );
}
