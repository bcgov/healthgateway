<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.column-wrapper {
    border: 1px;
}

.dateBreakLine {
    border-top: dashed 2px $primary;
}

.date {
    padding-top: 0px;
    color: $primary;
    font-size: 1.3em;
}

.sticky-offset {
    top: 70px;
    background-color: white;
    z-index: 2;
}

.sticky-line {
    top: 139px;
    background-color: white;
    border-bottom: solid $primary 2px;
    margin-top: -2px;
    z-index: 1;
    @media (max-width: 575px) {
        top: 193px;
    }
}

.noTimelineEntriesText {
    font-size: 1.5rem;
    color: #6c757d;
}
</style>
<template>
    <div>
        <b-row class="no-print sticky-top sticky-offset">
            <b-col class="py-2">
                <b-pagination-nav
                    v-show="!timelineIsEmpty"
                    v-model="currentPage"
                    :link-gen="linkGen"
                    :number-of-pages="numberOfPages"
                    first-number
                    last-number
                    next-text="Next"
                    prev-text="Prev"
                    use-router
                ></b-pagination-nav>
            </b-col>
            <b-col class="py-2 col-12 col-sm-auto order-first order-sm-last">
                <slot name="month-list-toggle"></slot>
            </b-col>
        </b-row>
        <b-row v-if="!timelineIsEmpty" class="sticky-top sticky-line" />
        <b-row id="listControls" class="no-print">
            <b-col>
                Displaying {{ getVisibleCount() }} out of
                {{ totalEntries }} records
            </b-col>
        </b-row>
        <div id="timeData">
            <b-row v-for="dateGroup in dateGroups" :key="dateGroup.key">
                <b-col cols="auto">
                    <div
                        :id="dateGroup.key"
                        :ref="dateGroup.key"
                        class="date"
                        tabindex="1"
                    >
                        {{ getHeadingDate(dateGroup.date) }}
                    </div>
                </b-col>
                <b-col>
                    <hr class="dateBreakLine" />
                </b-col>
                <EntryCardComponent
                    v-for="(entry, index) in dateGroup.entries"
                    :key="entry.type + '-' + entry.id"
                    :datekey="dateGroup.key"
                    :entry="entry"
                    :index="index"
                />
            </b-row>
        </div>
        <div v-if="timelineIsEmpty" class="text-center pt-2">
            <b-row>
                <b-col>
                    <img
                        class="mx-auto d-block"
                        src="@/assets/images/timeline/empty-state.png"
                        width="200"
                        height="auto"
                        alt="..."
                    />
                </b-col>
            </b-row>
            <b-row>
                <b-col>
                    <p class="text-center pt-2 noTimelineEntriesText">
                        No Timeline Entries
                    </p>
                </b-col>
            </b-row>
        </div>
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";
import moment from "moment";
import EventBus from "@/eventbus";
import TimelineEntry from "@/models/timelineEntry";
import EntryCardTimelineComponent from "@/components/timeline/entrycard.vue";
import { EventMessageName } from "@/constants/eventMessageName";

interface DateGroup {
    key: string;
    date: Date;
    entries: TimelineEntry[];
}

@Component({
    components: {
        EntryCardComponent: EntryCardTimelineComponent,
    },
})
export default class LinearTimelineComponent extends Vue {
    @Prop() private timelineEntries!: TimelineEntry[];
    @Prop({ default: 0 }) private totalEntries!: number;
    @Prop() private isVisible!: boolean;

    @Prop() private filterText!: string;
    @Prop() private filterTypes!: string[];

    private filteredTimelineEntries: TimelineEntry[] = [];
    private visibleTimelineEntries: TimelineEntry[] = [];

    private windowWidth: number = 0;
    private currentPage: number = 1;
    private hasErrors: boolean = false;

    private eventBus = EventBus;

    @Watch("filterText")
    @Watch("filterTypes")
    private applyTimelineFilter() {
        this.filteredTimelineEntries = this.timelineEntries.filter((entry) =>
            entry.filterApplies(this.filterText, this.filterTypes)
        );
    }

    @Watch("timelineEntries")
    private refreshEntries() {
        this.applyTimelineFilter();
    }

    @Watch("visibleTimelineEntries")
    private onVisibleEntriesUpdate() {
        if (this.visibleTimelineEntries.length > 0) {
            if (this.isVisible) {
                this.eventBus.$emit(
                    "timelinePageUpdate",
                    new Date(this.visibleTimelineEntries[0].date)
                );
            }
        }
    }

    private created() {
        let self = this;
        this.eventBus.$on("calendarDateEventClick", function (eventDate: Date) {
            self.setPageFromDate(eventDate);
            // Wait for next render cycle until the pages have been calculated and displayed
            self.$nextTick().then(function () {
                const date = new Date(eventDate).setHours(0, 0, 0, 0);
                let container: HTMLElement[] = self.$refs[
                    date
                ] as HTMLElement[];
                container[0].focus();
            });
        });

        this.eventBus.$on(EventMessageName.CalendarMonthUpdated, function (
            firstEntryDate: Date
        ) {
            self.setPageFromDate(firstEntryDate);
        });

        window.addEventListener("resize", this.handleResize);
        this.handleResize();
    }

    private destroyed() {
        window.removeEventListener("handleResize", this.handleResize);
    }

    private handleResize() {
        this.windowWidth = window.innerWidth;
    }

    private linkGen(pageNum: number) {
        return `?page=${pageNum}`;
    }

    private get numberOfEntriesPerPage(): number {
        if (this.windowWidth < 576) {
            // xs
            return 7;
        } else if (this.windowWidth < 768) {
            // s
            return 9;
        } else if (this.windowWidth < 992) {
            // m
            return 11;
        } else if (this.windowWidth < 1200) {
            // l
            return 13;
        } // else, xl
        return 15;
    }

    private get numberOfPages(): number {
        let result = 1;
        if (this.filteredTimelineEntries.length > this.numberOfEntriesPerPage) {
            result = Math.ceil(
                this.filteredTimelineEntries.length /
                    this.numberOfEntriesPerPage
            );
        }
        return result;
    }

    private getHeadingDate(date: Date): string {
        return moment(date).format("ll");
    }

    @Watch("currentPage")
    @Watch("numberOfEntriesPerPage")
    @Watch("filteredTimelineEntries")
    private calculateVisibleEntries() {
        // Handle the current page being beyond the max number of pages
        if (this.currentPage > this.numberOfPages) {
            this.currentPage = this.numberOfPages;
        }
        // Get the section of the array that contains the paginated section
        let lowerIndex = (this.currentPage - 1) * this.numberOfEntriesPerPage;
        let upperIndex = Math.min(
            this.currentPage * this.numberOfEntriesPerPage,
            this.filteredTimelineEntries.length
        );
        this.visibleTimelineEntries = this.filteredTimelineEntries.slice(
            lowerIndex,
            upperIndex
        );
    }

    private get dateGroups(): DateGroup[] {
        if (this.visibleTimelineEntries.length === 0) {
            return [];
        }
        let groups = this.visibleTimelineEntries.reduce<
            Record<string, TimelineEntry[]>
        >((groups, entry) => {
            // Get the string version of the date and get the date
            const date = new Date(entry.date).setHours(0, 0, 0, 0);

            // Create a new group if it the date doesnt exist in the map
            if (!groups[date]) {
                groups[date] = [];
            }
            groups[date].push(entry);
            return groups;
        }, {});
        let groupArrays = Object.keys(groups).map<DateGroup>((dateKey) => {
            return {
                key: dateKey,
                date: new Date(groups[dateKey][0].date),
                entries: groups[
                    dateKey
                ].sort((a: TimelineEntry, b: TimelineEntry) =>
                    a.type > b.type ? 1 : a.type < b.type ? -1 : 0
                ),
            };
        });
        return groupArrays;
    }

    private sortGroup(groupArrays: DateGroup[]) {
        groupArrays.sort((a, b) =>
            a.date > b.date ? -1 : a.date < b.date ? 1 : 0
        );
        return groupArrays;
    }

    private getVisibleCount(): number {
        return this.visibleTimelineEntries.length;
    }

    private setPageFromDate(eventDate: Date) {
        let index = this.filteredTimelineEntries.findIndex(
            (entry) => entry.date === eventDate
        );
        this.currentPage = Math.floor(index / this.numberOfEntriesPerPage) + 1;
    }

    private get timelineIsEmpty(): boolean {
        return this.filteredTimelineEntries.length == 0;
    }
}
</script>
