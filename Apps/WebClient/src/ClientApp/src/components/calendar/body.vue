<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import TimelineEntry, { EntryType, DateGroup } from "@/models/timelineEntry";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import LaboratoryTimelineEntry from "@/models/laboratoryTimelineEntry";
import EncounterTimelineEntry from "@/models/encounterTimelineEntry";
import EventBus, { EventMessageName } from "@/eventbus";
import { CalendarEntry, CalendarWeek } from "./models";
import { DateWrapper } from "@/models/dateWrapper";

@Component({})
export default class CalendarBodyComponent extends Vue {
    @Prop() currentMonth!: DateWrapper;
    @Prop() dateGroups!: DateGroup[];
    @Prop() weekNames!: string[];
    @Prop() monthNames!: string[];
    @Prop() firstDay!: number;
    @Prop() private isVisible!: boolean;

    private eventLimit = 4;

    private isHovering = false;
    private hoveringEvent: CalendarEntry | null = null;
    private eventBus = EventBus;

    private monthData: CalendarWeek[] = [];

    @Watch("currentMonth")
    private onCurrentMonthUpdate() {
        // Make sure it does not attempt to update if there is no data
        if (this.dateGroups.length === 0) {
            return;
        }

        this.monthData = this.getMonthCalendar(this.currentMonth);
        if (this.isVisible) {
            let dateGroup: DateGroup = this.dateGroups.find((d) =>
                this.currentMonth.isSame(d.date, "month")
            ) as DateGroup;
            this.eventBus.$emit(
                EventMessageName.CalendarMonthUpdated,
                dateGroup.entries[0].date
            );
        }
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

        let groups = thisDayEvents.reduce<Record<number, TimelineEntry[]>>(
            (groups, entry) => {
                // Create a new group if it the type doesnt exist in the map
                if (!groups[entry.type]) {
                    groups[entry.type] = [];
                }
                groups[entry.type].push(entry);
                return groups;
            },
            {}
        );

        let index = 0;
        let groupArrays = Object.keys(groups).map<CalendarEntry>((typeKey) => {
            index++;
            return {
                id: date.fromEpoch() + "-type-" + typeKey,
                cellIndex: index,
                type: Number(typeKey),
                entries: groups[
                    Number(typeKey)
                ].sort((a: TimelineEntry, b: TimelineEntry) =>
                    a.type > b.type ? 1 : a.type < b.type ? -1 : 0
                ),
            };
        });

        return groupArrays;
    }

    private getIcon(event: CalendarEntry) {
        if (event.type == EntryType.Medication) {
            return "pills";
        }
        if (event.type == EntryType.Immunization) {
            return "syringe";
        }
        if (event.type == EntryType.Laboratory) {
            return "flask";
        }
        if (event.type == EntryType.Note) {
            return "edit";
        }
        if (event.type == EntryType.Encounter) {
            return "user-md";
        }
        return "";
    }

    private getBackground(event: CalendarEntry) {
        if (event.type == EntryType.Medication) {
            return "medication";
        } else if (event.type == EntryType.Immunization) {
            return "immunization";
        } else if (event.type == EntryType.Laboratory) {
            return "laboratory";
        } else if (event.type == EntryType.Note) {
            return "note";
        } else if (event.type == EntryType.Encounter) {
            return "encounter";
        }

        return "";
    }

    private eventClick(event: CalendarEntry, jsEvent: Event) {
        jsEvent.stopPropagation();
        this.eventBus.$emit(
            EventMessageName.CalendarDateEventClick,
            event.entries[0].date
        );
    }

    private getTypeName(type: EntryType): string {
        return EntryType[type];
    }

    private getEntryText(entry: TimelineEntry, type: EntryType): string {
        if (type == EntryType.Medication) {
            return (entry as MedicationTimelineEntry).medication.brandName;
        } else if (type == EntryType.Immunization) {
            return (entry as ImmunizationTimelineEntry).immunization.name;
        } else if (type == EntryType.Laboratory) {
            return (entry as LaboratoryTimelineEntry).summaryTitle;
        } else if (type == EntryType.Note) {
            return (entry as NoteTimelineEntry).title;
        } else if (type == EntryType.Encounter) {
            return (entry as EncounterTimelineEntry).practitionerName;
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
                                @click="eventClick(event, $event)"
                            >
                                <div
                                    :id="'event-' + event.id"
                                    class="icon"
                                    :class="getBackground(event)"
                                >
                                    <font-awesome-icon :icon="getIcon(event)" />
                                </div>
                                <b-tooltip
                                    variant="secondary"
                                    placement="right-bottom"
                                    fallback-placement="clockwise"
                                    :target="'event-' + event.id"
                                    triggers="hover"
                                >
                                    <strong>
                                        {{ getTypeName(event.type) }}
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

<style lang="scss">
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
                        .medication {
                            background-color: $primary !important;
                        }
                        .laboratory {
                            background-color: $primary !important;
                        }
                        .note {
                            background-color: $bcgold !important;
                        }
                        .immunization {
                            background-color: $primary !important;
                        }
                        .encounter {
                            background-color: $primary !important;
                        }

                        .icon {
                            text-align: center;
                            border-radius: 50%;
                            width: 34px;
                            height: 34px;
                            line-height: 34px;
                            color: white;
                            font-size: 20px;
                        }
                    }
                    /* Small Devices */
                    @media (max-width: 440px) {
                        .event-item {
                            padding: 1px;
                            margin: 1px;
                            .icon {
                                width: 30px;
                                height: 30px;
                                line-height: 30px;
                                font-size: 18px;
                            }
                        }
                    }
                }
            }
        }
    }
}
</style>
