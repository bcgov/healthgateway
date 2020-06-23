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
          Displaying {{ getVisibleCount() }} out of {{ totalEntries }} records
        </b-col>
      </b-row>
    </div>
    <div id="timeData">
      <b-row v-for="dateGroup in dateGroups" :key="dateGroup.key">
        <b-col cols="auto">
          <div class="date">{{ getHeadingDate(dateGroup.date) }}</div>
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
  @Prop() private totalEntries: number = 0;

  private windowWidth: number = 0;
  private currentPage: number = 1;
  private hasErrors: boolean = false;
  private visibleTimelineEntries: TimelineEntry[] = [];

  private filterTypes: string[] = [];

  private created() {
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
    if (this.timelineEntries.length > this.numberOfEntriesPerPage) {
      result = Math.ceil(
        this.timelineEntries.length / this.numberOfEntriesPerPage
      );
    }
    return result;
  }

  private getHeadingDate(date: Date): string {
    return moment(date).format("ll");
  }

  @Watch("currentPage")
  @Watch("numberOfEntriesPerPage")
  @Watch("timelineEntries")
  private calculateVisibleEntries() {
    // Handle the current page being beyond the max number of pages
    if (this.currentPage > this.numberOfPages) {
      this.currentPage = this.numberOfPages;
    }
    // Get the section of the array that contains the paginated section
    let lowerIndex = (this.currentPage - 1) * this.numberOfEntriesPerPage;
    let upperIndex = Math.min(
      this.currentPage * this.numberOfEntriesPerPage,
      this.timelineEntries.length
    );
    this.visibleTimelineEntries = this.timelineEntries.slice(
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
        date: groups[dateKey][0].date,
        entries: groups[dateKey].sort((a: TimelineEntry, b: TimelineEntry) =>
          a.type > b.type ? 1 : a.type < b.type ? -1 : 0
        ),
      };
    });
    return this.sortGroup(groupArrays);
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
}
</script>
