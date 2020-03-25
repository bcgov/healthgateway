<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.column-wrapper {
  border: 1px;
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

.btn-light {
  border-color: $primary;
  color: $primary;
}
</style>
<template>
  <div>
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <b-row class="my-3 fluid justify-content-md-center">
      <b-col class="col-12 col-md-2 col-lg-3 column-wrapper">
        <b-row>
          <b-col class="col-0 col-xs-4 col-lg-2">&nbsp;</b-col>
          <b-col class="col-12 col-xs-8 col-lg-8">
            <b-button
              v-if="config.modules['Note'] == true"
              variant="light"
              class="w-100 visible-lg-block"
              :disabled="isAddingNote"
              @click="isAddingNote = true"
            >
              <font-awesome-icon icon="edit" aria-hidden="true" />
              Add Note
            </b-button>
          </b-col>
          <b-col class="col-0 col-xs-0 col-lg-2">&nbsp;</b-col>
        </b-row>
      </b-col>
      <b-col id="timeline" class="col-12 col-md-8 col-lg-6 column-wrapper">
        <b-alert :show="hasErrors" dismissible variant="danger">
          <h4>Error</h4>
          <span>An unexpected error occured while processing the request.</span>
        </b-alert>
        <b-alert :show="hasNewTermsOfService" dismissible variant="info">
          <h4>Updated Terms of Service</h4>
          <span>
            The Terms of Service have been updated since your last login. You
            can review them
            <router-link
              id="termsOfServiceLink"
              variant="primary"
              to="/termsOfService"
            >
              here</router-link
            >.
          </span>
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
                :icon="searchIcon"
                class="form-control-feedback"
                fixed-width
              ></font-awesome-icon>
              <b-form-input
                v-model="filterText"
                type="text"
                placeholder=""
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
            <b-row v-if="isAddingNote" class="pb-5">
              <b-col>
                <NoteTimelineComponent
                  :is-add-mode="true"
                  @on-edit-close="isAddingNote = false"
                  @on-note-added="onNoteAdded"
                />
              </b-col>
            </b-row>
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
                @on-change="onCardUpdated"
                @on-remove="onCardRemoved"
                @on-edit="onCardEdit"
                @on-close="onCardClose"
              />
            </b-row>
          </div>
        </div>
      </b-col>
      <b-col class="col-3 col-md-2 col-lg-3 column-wrapper">
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
import {
  IMedicationService,
  IImmunizationService,
  IUserNoteService
} from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ResultType } from "@/constants/resulttype";
import User from "@/models/user";
import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import MedicationStatement from "@/models/medicationStatement";
import moment from "moment";
import LoadingComponent from "@/components/loading.vue";
import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import EntryCardTimelineComponent from "@/components/timeline/entrycard.vue";
import HealthlinkSidebarComponent from "@/components/timeline/healthlink.vue";
import NoteTimelineComponent from "@/components/timeline/note.vue";
import FeedbackComponent from "@/components/feedback.vue";
import { faSearch, IconDefinition } from "@fortawesome/free-solid-svg-icons";
import UserNote from "@/models/userNote";
import { WebClientConfiguration } from "@/models/configData";
import RequestResult from "@/models/requestResult";

const namespace: string = "user";

interface DateGroup {
  date: string;
  entries: any;
}

// Register the router hooks with their names
Component.registerHooks(["beforeRouteLeave"]);

@Component({
  components: {
    LoadingComponent,
    ProtectiveWordComponent,
    EntryCardComponent: EntryCardTimelineComponent,
    HealthlinkComponent: HealthlinkSidebarComponent,
    FeedbackComponent,
    NoteTimelineComponent
  }
})
export default class TimelineComponent extends Vue {
  @Getter("user", { namespace }) user: User;
  @Getter("webClient", { namespace: "config" }) config: WebClientConfiguration;

  private filterText: string = "";
  private timelineEntries: TimelineEntry[] = [];
  private visibleTimelineEntries: TimelineEntry[] = [];
  private isMedicationLoading: boolean = false;
  private isImmunizationLoading: boolean = false;
  private isNoteLoading: boolean = false;
  private hasErrors: boolean = false;
  private sortyBy: string = "date";
  private sortDesc: boolean = true;
  private protectiveWordAttempts: number = 0;
  private isAddingNote: boolean = false;
  private editIdList: string[] = [];
  private unsavedChangesText: string =
    "You have unsaved changes. Are you sure you want to leave?";

  @Ref("protectiveWordModal")
  readonly protectiveWordModal: ProtectiveWordComponent;

  mounted() {
    this.fetchMedicationStatements();
    this.fetchImmunizations();
    this.fetchNotes();

    window.addEventListener("beforeunload", this.onBrowserClose);
  }

  beforeRouteLeave(to, from, next) {
    if (
      (this.isAddingNote || this.editIdList.length > 0) &&
      !confirm(this.unsavedChangesText)
    ) {
      return;
    }
    next();
  }

  private onBrowserClose(event: BeforeUnloadEvent) {
    if (this.isAddingNote || this.editIdList.length > 0) {
      event.returnValue = this.unsavedChangesText;
    }
  }

  private get unverifiedEmail(): boolean {
    return !this.user.verifiedEmail && this.user.hasEmail;
  }

  private get hasNewTermsOfService(): boolean {
    return this.user.hasTermsOfServiceUpdated;
  }

  private get searchIcon(): IconDefinition {
    return faSearch;
  }

  private get isLoading(): boolean {
    return (
      this.isMedicationLoading ||
      this.isImmunizationLoading ||
      this.isNoteLoading
    );
  }

  private fetchMedicationStatements(protectiveWord?: string) {
    const medicationService: IMedicationService = container.get(
      SERVICE_IDENTIFIER.MedicationService
    );
    this.isMedicationLoading = true;

    const isOdrEnabled = this.config.modules["MedicationHistory"];
    let promise: Promise<RequestResult<MedicationStatement[]>>;

    if (isOdrEnabled) {
      promise = medicationService.getPatientMedicationStatementHistory(
        this.user.hdid,
        protectiveWord
      );
    } else {
      promise = medicationService.getPatientMedicationStatements(
        this.user.hdid,
        protectiveWord
      );
    }

    promise
      .then(results => {
        if (results.resultStatus == ResultType.Success) {
          this.protectiveWordAttempts = 0;

          // Add the medication entries to the timeline list
          for (let result of results.resourcePayload) {
            this.timelineEntries.push(new MedicationTimelineEntry(result));
          }
          this.applyTimelineFilter();
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
        this.isMedicationLoading = false;
      });
  }

  private fetchImmunizations() {
    const immunizationService: IImmunizationService = container.get(
      SERVICE_IDENTIFIER.ImmunizationService
    );
    this.isImmunizationLoading = true;
    immunizationService
      .getPatientImmunizations(this.user.hdid)
      .then(results => {
        if (results.resultStatus == ResultType.Success) {
          // Add the immunization entries to the timeline list
          for (let result of results.resourcePayload) {
            this.timelineEntries.push(new ImmunizationTimelineEntry(result));
          }
          this.applyTimelineFilter();
        } else {
          console.log(
            "Error returned from the immunization call: " +
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
        this.isImmunizationLoading = false;
      });
  }

  private fetchNotes() {
    const noteService: IUserNoteService = container.get(
      SERVICE_IDENTIFIER.UserNoteService
    );
    this.isNoteLoading = true;
    noteService
      .getNotes()
      .then(results => {
        if (results.resultStatus == ResultType.Success) {
          // Add the immunization entries to the timeline list
          for (let result of results.resourcePayload) {
            this.timelineEntries.push(new NoteTimelineEntry(result));
          }
          this.applyTimelineFilter();
        } else {
          console.log(
            "Error returned from the note call: " + results.resultMessage
          );
          this.hasErrors = true;
        }
      })
      .catch(err => {
        this.hasErrors = true;
        console.log(err);
      })
      .finally(() => {
        this.isNoteLoading = false;
      });
  }

  private onNoteAdded(note: UserNote) {
    this.isAddingNote = false;
    if (note) {
      this.timelineEntries.push(new NoteTimelineEntry(note));
    }
  }

  private onCardRemoved(entry: TimelineEntry) {
    const index = this.timelineEntries.findIndex(e => e.id == entry.id);
    this.timelineEntries.splice(index, 1);
  }

  private onCardEdit(entry: TimelineEntry) {
    this.editIdList.push(entry.id);
  }

  private onCardClose(entry: TimelineEntry) {
    const index = this.editIdList.findIndex(e => e == entry.id);
    this.editIdList.splice(index, 1);
  }

  private onCardUpdated(entry: TimelineEntry) {
    const index = this.timelineEntries.findIndex(e => e.id == entry.id);
    this.timelineEntries.splice(index, 1);
    this.timelineEntries.push(entry);
  }

  private onProtectiveWordSubmit(value: string) {
    this.fetchMedicationStatements(value);
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
    if (!this.filterText) {
      this.visibleTimelineEntries = this.timelineEntries;
    } else {
      this.visibleTimelineEntries = this.timelineEntries.filter(entry =>
        entry.filterApplies(this.filterText)
      );
    }
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
        entries: groups[dateKey].sort((a, b) =>
          a.type > b.type ? 1 : a.type < b.type ? -1 : 0
        )
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
