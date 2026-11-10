import { describe, expect, it } from "@jest/globals";

import { EntryType } from "@/constants/entryType";
import { MedicationTimelineEntry } from "@/models/timeline/medicationTimelineEntry";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { medicationStatementsFixture } from "@/test/fixtures";
import { formatDate } from "@/utils/dateUtil";

describe("MedicationTimelineEntry (medicationTimelineEntry.ts port)", () => {
    const [rx, assessment] = medicationStatementsFixture();

    it("maps a prescription: dispensed date (date part), title, pharmacy address, directions", () => {
        const e = new MedicationTimelineEntry(rx);
        expect(e.type).toBe(EntryType.Medication);
        expect(e.id).toBe("rx1");
        expect(formatDate(e.date)).toBe("2024-Feb-10");
        expect(e.title).toBe("Amoxicillin");
        expect(e.subtitle).toBe("500 mg capsule");
        expect(e.pharmacy.address).toBe("1 Main St Unit 2, Victoria BC");
        expect(e.directions).toBe("Take one daily");
        expect(e.isPharmacistAssessment).toBe(false);
        expect(e.comments).toBeNull();
    });

    it("a pharmacist assessment: N/A practitioner and directions, outcome flags", () => {
        const e = new MedicationTimelineEntry(assessment);
        expect(e.isPharmacistAssessment).toBe(true);
        expect(e.practitionerSurname).toBe("N/A");
        expect(e.directions).toBe("N/A");
        expect(e.prescriptionProvided).toBe(true);
        expect(e.redirectedToHealthCareProvider).toBe(true);
    });

    it("keyword search covers practitioner, title and subtitle", () => {
        const e = new MedicationTimelineEntry(rx);
        for (const k of ["smith", "AMOXI", "capsule"])
            expect(
                e.filterApplies(
                    TimelineFilterBuilder.create().withKeyword(k).build()
                )
            ).toBe(true);
        expect(
            e.filterApplies(
                TimelineFilterBuilder.create().withKeyword("pharmacy").build()
            )
        ).toBe(false);
    });
});
