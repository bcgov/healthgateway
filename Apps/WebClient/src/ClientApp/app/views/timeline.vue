<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.column-wrapper {
  border: 1px; //red solid;
}

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

.has-filter .form-control {
  padding-left: 2.375rem;
}

.has-filter .form-control-feedback {
  position: absolute;
  z-index: 2;
  display: block;
  width: 2.375rem;
  height: 2.375rem;
  line-height: 2.375rem;
  text-align: center;
  pointer-events: none;
  color: #aaa;
  padding: 12px;
}
</style>
<template>
  <div>
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <b-row class="my-3 fluid justify-content-md-center">
      <b-col class="col-3 column-wrapper"></b-col>
      <b-col id="timeline" class="col-12 col-lg-6 column-wrapper">
        <b-alert :show="hasErrors" dismissible variant="danger">
          <h4>Error</h4>
          <span>An unexpected error occured while processing the request.</span>
        </b-alert>
        <b-alert :show="unverifiedEmail" dismissible variant="info">
          <h4>Unverified email</h4>
          <span>
            Your email has not been verified. Please check your inbox or junk
            folder for an email from Health Gateway.
          </span>
        </b-alert>
        <div id="pageTitle">
          <h1 id="subject">Health Care Timeline</h1>
          <hr />
        </div>
        <b-row>
          <b-col>
            <div class="form-group has-filter">
              <font-awesome-icon
                :icon="getIcon()"
                class="form-control-feedback"
                fixed-width
              ></font-awesome-icon>
              <b-form-input
                v-model="filterText"
                type="text"
                placeholder="Filter"
                maxlength="50"
                debounce="250"
              ></b-form-input>
            </div>
            <br />
          </b-col>
        </b-row>
        <div v-if="!isLoading">
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
                      <font-awesome-icon
                        icon="chevron-down"
                        aria-hidden="true"
                      ></font-awesome-icon>
                    </span>
                    <span v-show="!sortDesc" name="ascending">
                      (Oldest)
                      <font-awesome-icon
                        icon="chevron-up"
                        aria-hidden="true"
                      ></font-awesome-icon>
                    </span>
                  </b-btn>
                </b-row>
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
              <MedicationComponent
                v-for="(entry, index) in dateGroup.entries"
                :key="entry.id"
                :datekey="dateGroup.key"
                :entry="entry"
                :index="index"
              />
            </b-row>
          </div>
        </div>
      </b-col>
      <b-col class="col-3 column-wrapper">
        <HealthlinkComponent />
      </b-col>
    </b-row>
    <ProtectiveWordComponent
      ref="protectiveWordModal"
      :error="protectiveWordAttempts > 1"
      @submit="onProtectiveWordSubmit"
      @cancel="onProtectiveWordCancel"
    />
    <FeedbackComponent />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Watch, Ref } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";
import { IMedicationService } from "@/services/interfaces";
import container from "@/inversify.config";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import { ResultType } from "@/constants/resulttype";
import User from "@/models/user";
import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import MedicationStatement from "@/models/medicationStatement";
import moment from "moment";
import LoadingComponent from "@/components/loading.vue";
import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import MedicationTimelineComponent from "@/components/timeline/medication.vue";
import HealthlinkSidebarComponent from "@/components/timeline/healthlink.vue";
import FeedbackComponent from "@/components/feedback.vue";
import { faSearch, IconDefinition } from "@fortawesome/free-solid-svg-icons";

const namespace: string = "user";

interface DateGroup {
  date: string;
  entries: any;
}

@Component({
  components: {
    LoadingComponent,
    ProtectiveWordComponent,
    MedicationComponent: MedicationTimelineComponent,
    HealthlinkComponent: HealthlinkSidebarComponent,
    FeedbackComponent
  }
})
export default class TimelineComponent extends Vue {
  @Getter("user", { namespace }) user: User;

  private filterText: string = "";
  private timelineEntries: TimelineEntry[] = [];
  private visibleTimelineEntries: TimelineEntry[] = [];
  private isLoading: boolean = false;
  private hasErrors: boolean = false;
  private sortyBy: string = "date";
  private sortDesc: boolean = true;
  private protectiveWordAttempts: number = 0;

  @Ref("protectiveWordModal")
  readonly protectiveWordModal: ProtectiveWordComponent;

  mounted() {
    this.fechMedicationStatements();
  }

  get unverifiedEmail(): boolean {
    return !this.user.verifiedEmail && this.user.hasEmail;
  }

  private getIcon(): IconDefinition {
    return faSearch;
  }

  private fechMedicationStatements(protectiveWord?: string) {
    const medicationService: IMedicationService = container.get(
      SERVICE_IDENTIFIER.MedicationService
    );
    this.isLoading = true;
    medicationService
      .getPatientMedicationStatements(this.user.hdid, protectiveWord)
      .then(results => {
        if (results.resultStatus == ResultType.Success) {
          this.protectiveWordAttempts = 0;

          // Add the medication entries to the timeline list
          for (let result of results.resourcePayload) {
            this.timelineEntries.push(new MedicationTimelineEntry(result));
          }
          if (!this.filterText) {
            this.visibleTimelineEntries = this.timelineEntries;
          } else {
            this.applyTimelineFilter();
          }
        } else if (results.resultStatus == ResultType.Protected) {
          this.protectiveWordModal.showModal();
          this.protectiveWordAttempts++;
        } else {
          console.log(
            "Error returned from the medication statements call: " +
              results.resultMessage
          );
          this.hasErrors = true;
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

  private onProtectiveWordSubmit(value: string) {
    this.fechMedicationStatements(value);
  }

  private onProtectiveWordCancel() {
    // Does nothing as it won't be able to fetch pharmanet data.
    console.log("protective word cancelled");
  }

  private toggleSort(): void {
    this.sortDesc = !this.sortDesc;
  }

  private getHeadingDate(date: Date): string {
    return moment(date).format("ll");
  }

  @Watch("filterText")
  private applyTimelineFilter() {
    this.visibleTimelineEntries = this.timelineEntries.filter(entry =>
      entry.filterApplies(this.filterText)
    );
  }
  private get dateGroups(): DateGroup[] {
    if (this.visibleTimelineEntries.length === 0) {
      return [];
    }

    let groups = this.visibleTimelineEntries.reduce((groups, entry) => {
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
    return this.visibleTimelineEntries.length;
  }

  private getTotalCount(): number {
    return this.timelineEntries.length;
  }
}
</script>
