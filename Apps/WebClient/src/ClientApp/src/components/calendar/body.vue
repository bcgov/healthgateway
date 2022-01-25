<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faClipboardList,
    faEdit,
    faFlask,
    faPills,
    faSyringe,
    faUserMd,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { Action } from "vuex-class";

import Covid19LaboratoryOrderTimelineEntry from "@/models/covid19LaboratoryOrderTimelineEntry";
import { DateWrapper } from "@/models/dateWrapper";
import EncounterTimelineEntry from "@/models/encounterTimelineEntry";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import MedicationRequestTimelineEntry from "@/models/medicationRequestTimelineEntry";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import TimelineEntry, { DateGroup, EntryType } from "@/models/timelineEntry";

import { CalendarEntry, CalendarWeek } from "./models";

library.add(faClipboardList, faEdit, faFlask, faPills, faSyringe, faUserMd);

@Component({})
export default class CalendarBodyComponent extends Vue {
    @Action("setLinearView", { namespace: "timeline" }) setLinearView!: (
        isLinearView: boolean
    ) => void;

    @Action("setSelectedDate", { namespace: "timeline" }) setSelectedDate!: (
        date: DateWrapper
    ) => void;

    @Prop() readonly currentMonth!: DateWrapper;
    @Prop() readonly dateGroups!: DateGroup[];
    @Prop() readonly weekNames!: string[];
    @Prop() readonly monthNames!: string[];
    @Prop() readonly firstDay!: number;

    private eventLimit = 4;

    private isHovering = false;
    private hoveringEvent: CalendarEntry | null = null;

    private monthData: CalendarWeek[] = [];

    @Watch("currentMonth")
    private onCurrentMonthUpdate() {
        // Make sure it does not attempt to update if there is no data
        if (this.dateGroups.length === 0) {
            return;
        }

        this.monthData = this.getMonthCalendar(this.currentMonth);
    }

    private getMonthCalendar(monthDate: DateWrapper): CalendarWeek[] {
        let firstMonthDate = monthDate.startOf("month");
        let curWeekDay = firstMonthDate.weekday();

        // begin date of this table may be some day of last month
        let dateIndex = firstMonthDate.subtract({
            day: curWeekDay + this.firstDay,
        });

        let calendar: CalendarWeek[] = [];

        let today = new DateWrapper();
        for (let perWeek = 0; perWeek < 6; perWeek++) {
            let week: CalendarWeek = {
                id: dateIndex.fromEpoch(),
                days: [],
            };

            for (let perDay = 0; perDay < 7; perDay++) {
                let dayEvent = {
                    id: dateIndex.fromEpoch() + "-" + perDay,
                    monthDay: dateIndex.day(),
                    isToday: today.isSame(dateIndex, "day"),
                    isCurMonth: dateIndex.month() === monthDate.month(),
                    weekDay: perDay,
                    date: dateIndex,
                    events: this.slotEvents(dateIndex),
                };
                week.days.push(dayEvent);

                dateIndex = dateIndex.add({ day: 1 });
            }

            calendar.push(week);
        }
        return calendar;
    }

    private slotEvents(date: DateWrapper): CalendarEntry[] {
        //TODO: It should do this computation once instead of every single time

        // find all events start from this date
        let dateGroup: DateGroup = this.dateGroups.find((d) =>
            date.isSame(d.date, "day")
        ) || {
            key: "",
            date: new DateWrapper(),
            entries: [],
        };

        let thisDayEvents: TimelineEntry[] = dateGroup.entries;

        let groups = thisDayEvents.reduce<Record<string, TimelineEntry[]>>(
            (groups, entry: TimelineEntry) => {
                const entryType = entry.type;
                // Create a new group if it the type doesnt exist in the map
                if (!groups[entryType]) {
                    groups[entryType] = [];
                }
                groups[entryType].push(entry);
                return groups;
            },
            {}
        );

        let index = 0;
        return Object.keys(groups).map<CalendarEntry>((type: string) => {
            index++;
            return {
                id: date.fromEpoch() + "-type-" + type,
                cellIndex: index,
                type: type as EntryType,
                entries: groups[type].sort(
                    (a: TimelineEntry, b: TimelineEntry) =>
                        a.type > b.type ? 1 : a.type < b.type ? -1 : 0
                ),
            };
        });
    }

    private getIcon(event: CalendarEntry) {
        if (event.type == EntryType.Medication) {
            return "pills";
        }
        if (event.type == EntryType.Immunization) {
            return "syringe";
        }
        if (event.type == EntryType.Covid19LaboratoryOrder) {
            return "flask";
        }
        if (event.type == EntryType.Note) {
            return "edit";
        }
        if (event.type == EntryType.Encounter) {
            return "user-md";
        }
        if (event.type == EntryType.MedicationRequest) {
            return "clipboard-list";
        }
        return "";
    }

    private getBackground(event: CalendarEntry) {
        if (event.type == EntryType.Note) {
            return "note";
        } else {
            return "default";
        }
    }

    private eventClick(event: CalendarEntry, jsEvent: Event) {
        jsEvent.stopPropagation();
        this.setSelectedDate(event.entries[0].date);
        this.setLinearView(true);
    }

    private getEntryText(entry: TimelineEntry, type: EntryType): string {
        if (type == EntryType.Medication) {
            return (entry as MedicationTimelineEntry).medication.brandName;
        } else if (type == EntryType.Immunization) {
            return (entry as ImmunizationTimelineEntry).immunization.name;
        } else if (type == EntryType.Covid19LaboratoryOrder) {
            return (entry as Covid19LaboratoryOrderTimelineEntry).summaryTitle;
        } else if (type == EntryType.Note) {
            return (entry as NoteTimelineEntry).title;
        } else if (type == EntryType.Encounter) {
            return (entry as EncounterTimelineEntry).practitionerName;
        } else if (type == EntryType.MedicationRequest) {
            return (entry as MedicationRequestTimelineEntry).drugName || "";
        }

        return "N/A";
    }
}
</script>

<template>
    <div class="calendar-body">
        <b-row class="weeks">
            <b-col v-for="week in weekNames" :key="week" class="week px-0"
                ><strong>{{ week }}</strong></b-col
            >
        </b-row>
        <b-row class="dates">
            <b-col>
                <b-row
                    v-for="week in monthData"
                    :key="week.id"
                    class="week-row"
                >
                    <b-col
                        v-for="day in week.days"
                        :key="day.id"
                        class="day-cell p-0"
                        :class="{
                            today: day.isToday,
                            'not-cur-month': !day.isCurMonth,
                        }"
                    >
                        <b-row align-h="end" class="day-number m-1 p-0">{{
                            day.monthDay
                        }}</b-row>
                        <b-row class="event-box p-0 m-1">
                            <b-col
                                v-for="event in day.events"
                                v-show="event.cellIndex <= eventLimit"
                                :key="event.id"
                                cols="auto"
                                class="event-item"
                                :data-testid="'event-monthday-' + day.monthDay"
                                @click="eventClick(event, $event)"
                            >
                                <div
                                    :id="'event-' + event.id"
                                    class="icon"
                                    :class="getBackground(event)"
                                >
                                    <hg-icon
                                        :icon="getIcon(event)"
                                        size="medium"
                                        fixed-width
                                    />
                                </div>
                                <b-tooltip
                                    variant="secondary"
                                    placement="right-bottom"
                                    fallback-placement="clockwise"
                                    :target="'event-' + event.id"
                                    triggers="hover"
                                >
                                    <strong>
                                        {{ event.type }}
                                    </strong>
                                    <ul class="text-left pl-3">
                                        <li
                                            v-for="entry in event.entries"
                                            :key="entry.id"
                                        >
                                            {{
                                                getEntryText(entry, event.type)
                                            }}
                                        </li>
                                    </ul>
                                </b-tooltip>
                            </b-col>
                        </b-row>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
    </div>
</template>

<style lang="scss" scoped>
.row {
    margin: 0px;
    padding: 0px;
}

.col {
    margin: 0px;
    padding: 0px;
}
</style>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.calendar-body {
    .weeks {
        border-top: 1px solid #e0e0e0;
        border-bottom: 1px solid #e0e0e0;
        border-left: 1px solid #e0e0e0;
        .week {
            min-width: 35px;
            text-align: center;
            border-right: 1px solid #e0e0e0;
        }
    }
    .dates {
        position: relative;
        .week-row {
            border-left: 1px solid #e0e0e0;
            .day-cell {
                min-height: 110px;
                min-width: 35px;
                border-right: 1px solid #e0e0e0;
                border-bottom: 1px solid #e0e0e0;
                .day-number {
                    text-align: right;
                }
                &.today {
                    background-color: #fcf8e3;
                }
                &.not-cur-month {
                    .day-number {
                        color: rgba(0, 0, 0, 0.24);
                    }
                }
                .event-box {
                    .event-item {
                        cursor: pointer;
                        display: inline-block;
                        padding: 2px;
                        margin: 2px;
                        color: white !important;
                        .note {
                            background-color: $bcgold !important;
                        }
                        .default {
                            background-color: $primary !important;
                        }

                        .icon {
                            text-align: center;
                            border-radius: 50%;
                            padding: 0.375em 0.5em;
                            color: white;
                        }
                    }
                    /* Small Devices */
                    @media (max-width: 440px) {
                        .event-item {
                            padding: 1px;
                            margin: 1px;
                            .icon {
                                padding: 0.0625rem 0.265625rem;

                                .hg-icon {
                                    font-size: 0.875rem;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
</style>
