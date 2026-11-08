import { describe, expect, it } from "@jest/globals";

import { EntryType } from "@/constants/entryType";
import type { ImmunizationEvent } from "@/models/immunization";
import { ImmunizationTimelineEntry } from "@/models/timeline/immunizationTimelineEntry";
import { DateGroup } from "@/models/timeline/timelineEntry";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";

const event = (
    overrides: Partial<ImmunizationEvent> = {}
): ImmunizationEvent => ({
    id: "i1",
    status: "Completed",
    dateOfImmunization: "2021-07-14T00:00:00",
    providerOrClinic: "Clinic B",
    immunization: {
        name: "COVID-19 mRNA",
        immunizationAgents: [
            {
                code: "a",
                name: "Agent A",
                lotNumber: "LOT42",
                productName: "Comirnaty",
            },
        ],
    },
    ...overrides,
});

describe("ImmunizationTimelineEntry (immunizationTimelineEntry.ts port)", () => {
    it("maps the view model: empty lot / product become null, the forecast due date is yyyy-MM-dd", () => {
        const entry = new ImmunizationTimelineEntry(
            event({
                immunization: {
                    name: "Td",
                    immunizationAgents: [
                        {
                            code: "b",
                            name: "Agent B",
                            lotNumber: "",
                            productName: "",
                        },
                    ],
                },
                providerOrClinic: "",
                forecast: {
                    createDate: "2021-07-14",
                    status: "Eligible",
                    displayName: "COVID-19 Non-replicating Viral Vector",
                    eligibleDate: "2021-08-01",
                    dueDate: "2021-08-11T00:00:00",
                },
            })
        );
        expect(entry.type).toBe(EntryType.Immunization);
        expect(entry.immunization.immunizationAgents[0]).toEqual({
            code: "b",
            name: "Agent B",
            lotNumber: null,
            productName: null,
        });
        expect(entry.immunization.providerOrClinic).toBeNull();
        expect(entry.immunization.forecast?.dueDate).toBe("2021-08-11");
        expect(entry.comments).toBeNull();
    });

    it("keyword search covers name, agents (lot, name, product) and provider, case-insensitively", () => {
        const entry = new ImmunizationTimelineEntry(event());
        for (const keyword of [
            "covid",
            "lot42",
            "agent a",
            "COMIRNATY",
            "clinic b",
        ])
            expect(
                entry.filterApplies(
                    TimelineFilterBuilder.create().withKeyword(keyword).build()
                )
            ).toBe(true);
        expect(
            entry.filterApplies(
                TimelineFilterBuilder.create().withKeyword("flu").build()
            )
        ).toBe(false);
    });

    it("entry type and inclusive date range filters", () => {
        const entry = new ImmunizationTimelineEntry(event());
        const b = () => TimelineFilterBuilder.create();
        expect(
            entry.filterApplies(
                b().withEntryTypes([EntryType.Medication]).build()
            )
        ).toBe(false);
        expect(
            entry.filterApplies(
                b().withEntryType(EntryType.Immunization).build()
            )
        ).toBe(true);
        expect(
            entry.filterApplies(
                b()
                    .withStartDate("2021-07-14")
                    .withEndDate("2021-07-14")
                    .build()
            )
        ).toBe(true);
        expect(
            entry.filterApplies(b().withStartDate("2021-07-15").build())
        ).toBe(false);
        expect(entry.filterApplies(b().withEndDate("2021-07-13").build())).toBe(
            false
        );
    });
});

describe("TimelineFilter", () => {
    it("is active with any of keyword, dates or types; createFromFilter copies it", () => {
        expect(TimelineFilterBuilder.buildEmpty().hasActiveFilter()).toBe(
            false
        );
        const f = TimelineFilterBuilder.create()
            .withStartDate("2020-01-01")
            .build();
        expect(f.hasActiveFilter()).toBe(true);
        expect(
            TimelineFilterBuilder.createFromFilter(f).build().startDate
        ).toBe("2020-01-01");
    });
});

describe("DateGroup", () => {
    it("groups entries with the same timestamp and sorts groups newest first", () => {
        const a = new ImmunizationTimelineEntry(event({ id: "a" }));
        const b = new ImmunizationTimelineEntry(event({ id: "b" }));
        const c = new ImmunizationTimelineEntry(
            event({ id: "c", dateOfImmunization: "2022-01-01T00:00:00" })
        );
        const groups = DateGroup.sortGroups(DateGroup.createGroups([a, b, c]));
        expect(groups.map((g) => g.entries.map((e) => e.id))).toEqual([
            ["c"],
            ["a", "b"],
        ]);
        expect(DateGroup.createGroups([])).toEqual([]);
    });
});
