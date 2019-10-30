<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#pageTitle {
  color: $primary;
}
#pageTitle hr {
  border-top: 2px solid $primary;
}

.sortContainer {
  text-align: right;
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
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <b-alert :show="hasErrors" dismissible variant="danger">
      <h4>Error</h4>
      <span>An unexpected error occured while processing the request.</span>
    </b-alert>
    <div id="pageTitle">
      <h1 id="subject">
        Health Care Timeline
      </h1>
      <hr />
    </div>
    <div id="listControlls">
      <b-row>
        <b-col>
          Displaying {{ getVisibleCount() }} out of
          {{ getTotalCount() }} records
        </b-col>
        <b-col cols="auto">
          <b-row
            :class="{ descending: sortDesc, ascending: !sortDesc }"
            class="text-right sortContainer"
          >
            <b-btn variant="link" @click="toggleSort()">
              Date
              <span v-show="sortDesc" name="descending">
                (Newest)
                <i class="fa fa-chevron-down" aria-hidden="true"></i
              ></span>
              <span v-show="!sortDesc" name="ascending">
                (Oldest)
                <i class="fa fa-chevron-up" aria-hidden="true"></i
              ></span>
            </b-btn>
          </b-row>
        </b-col>
      </b-row>
    </div>
    <b-container id="timeline">
      <b-row v-for="dateGroup in dateGroups" :key="dateGroup.key">
        <b-col>
          <b-row>
            <b-col cols="auto">
              <div class="date">
                {{ getHeadingDate(dateGroup.date) }}
              </div>
            </b-col>
            <b-col>
              <hr class="dateBreakLine" />
            </b-col>
          </b-row>

          <MedicationComponent
            v-for="(entry, index) in dateGroup.entries"
            :key="entry.id"
            :date_key="dateGroup.key"
            :entry="entry"
            :index="index"
          />
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";
import { IMedicationService } from "@/services/interfaces";
import LoadingComponent from "@/components/loading.vue";
import container from "@/inversify.config";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import User from "@/models/user";
import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import MedicationStatement from "@/models/medicationStatement";
import MedicationTimelineComponent from "@/components/timeline/medication.vue";
import moment from "moment";

const namespace: string = "user";

interface DateGroup {
  date: string;
  entries: any;
}

@Component({
  components: {
    LoadingComponent,
    MedicationComponent: MedicationTimelineComponent
  }
})
export default class TimelineComponent extends Vue {
  @Getter("user", { namespace }) user: User;
  private timelineEntries: MedicationStatement[] = [];
  private isLoading: boolean = false;
  private hasErrors: boolean = false;
  private sortyBy: string = "date";
  private sortDesc: boolean = true;

  mounted() {
    this.isLoading = true;
    const medicationService: IMedicationService = container.get(
      SERVICE_IDENTIFIER.MedicationService
    );
    medicationService
      .getPatientMedicationStatemens(this.user.hdid)
      .then(results => {
        console.log(results);
        if (!results.errorMessage) {
          // Add the medication entries to the timeline list
          for (let result of results.resourcePayload) {
            this.timelineEntries.push(result);
          }
        } else {
          console.log(
            "Error returned from the medication statements call: " +
              results.errorMessage
          );
        }
      })
      .catch(err => {
        this.hasErrors = true;
        console.log(err);
      })
      .finally(() => {
        this.isLoading = false;
      });
  }

  private toggleSort(): void {
    this.sortDesc = !this.sortDesc;
  }

  private getHeadingDate(date: Date): string {
    return moment(date).format("ll");
  }

  private get dateGroups(): DateGroup[] {
    if (this.timelineEntries.length === 0) {
      return [];
    }

    let groups = this.timelineEntries.reduce((groups, entry) => {
      // Get the string version of the date and get the date
      //const date = (entry.date).split("T")[0];
      const date = new Date(entry.date).setHours(0, 0, 0, 0);

      // Create a new group if it the date doesnt exist in the map
      if (!groups[date]) {
        groups[date] = [];
      }

      groups[date].push(entry);
      return groups;
    }, {});

    let groupArrays = Object.keys(groups).map(dateKey => {
      return {
        key: dateKey,
        date: groups[dateKey][0].date,
        entries: groups[dateKey]
      };
    });
    return this.sortGroup(groupArrays);
  }

  private sortGroup(groupArrays) {
    groupArrays.sort((a, b) =>
      a.date > b.date ? 1 : a.date < b.date ? -1 : 0
    );

    if (this.sortDesc) {
      groupArrays.reverse();
    }

    return groupArrays;
  }

  private getVisibleCount(): number {
    return this.timelineEntries.length;
  }

  private getTotalCount(): number {
    // TODO: The model needs to have pagination
    return this.timelineEntries.length;
  }
}
</script>
