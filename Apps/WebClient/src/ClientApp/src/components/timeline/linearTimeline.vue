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
</style>
<template>
    <div>
        <div id="listControlls" class="no-print">
            <b-row>
                <b-col>
                    Displaying {{ getVisibleCount() }} out of
                    {{ totalEntries }} records
                </b-col>
            </b-row>
        </div>
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
            <b-row class="no-print">
                <b-col>
                    <b-pagination-nav
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

    private created() {
        let self = this;
        this.eventBus.$on("calendarDateEventClick", function (eventDate: Date) {
            self.setPageFromDate(eventDate);
        });

        this.eventBus.$on(
            EventMessageName.TimelineCurrentDateUpdated,
            function (currentDate: Date) {
                self.goToPageByCurrentDate(currentDate);
            }
        );

        window.addEventListener("resize", this.handleResize);
        this.handleResize();
    }

    private goToPageByCurrentDate(currentDate: Date) {
        const selectedYearMonth = moment(currentDate).format("YYYY-MM");
        let currentPage = 1;
        let currentEntryIndex = 1;
        let i = 0;
        let foundLastEntryOfSelectedMonth = false;
        const timelineEntriesLenght = this.timelineEntries.length;
        // scan from the most recent entries to the older ones
        while (i < timelineEntriesLenght && !foundLastEntryOfSelectedMonth) {
            let entry = this.timelineEntries[i];
            if (
                entry.date !== undefined &&
                entry.date.toString().indexOf(selectedYearMonth) == 0
            ) {
                console.log("found the last entry of the selected month");
                foundLastEntryOfSelectedMonth = true;
            } else {
                currentEntryIndex++;
                if (currentEntryIndex > this.numberOfEntriesPerPage) {
                    currentEntryIndex = 1;
                    currentPage++;
                }
            }
            i++;
        }
        if (foundLastEntryOfSelectedMonth) {
            // If found entry is at the bottom of the to-go page, move to the next page
            if (
                currentEntryIndex > this.numberOfEntriesPerPage - 3 &&
                currentPage < this.numberOfPages
            ) {
                currentPage++;
            }
            this.currentPage = currentPage;
        }
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

        let self = this;
        // Wait for next render cycle until the pages have been calculated and displayed
        this.$nextTick().then(function () {
            const date = new Date(eventDate).setHours(0, 0, 0, 0);
            let container: HTMLElement[] = self.$refs[date] as HTMLElement[];
            container[0].focus();
        });
    }
}
</script>
