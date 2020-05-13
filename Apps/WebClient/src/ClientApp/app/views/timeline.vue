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
      <b-col class="col-12 col-md-1 col-lg-1 column-wrapper no-print"> </b-col>
      <b-col id="timeline" class="col-12 col-md-8 col-lg-6 column-wrapper">
        <b-alert
          :show="hasErrors"
          dismissible
          variant="danger"
          class="no-print"
        >
          <h4>Error</h4>
          <span>An unexpected error occured while processing the request.</span>
        </b-alert>
        <b-alert
          :show="hasNewTermsOfService"
          dismissible
          variant="info"
          class="no-print"
        >
          <h4>Updated Terms of Service</h4>
          <span>
            The Terms of Service have been updated since your last login. You
            can review them
            <router-link
              id="termsOfServiceLink"
              variant="primary"
              to="/termsOfService"
            >
              here </router-link
            >.
          </span>
        </b-alert>
        <b-alert
          :show="unverifiedEmail"
          dismissible
          variant="info"
          class="no-print"
        >
          <h4>Unverified email</h4>
          <span>
            Your email has not been verified. Please check your inbox or junk
            folder for an email from Health Gateway. You can also edit your
            profile or resend the email from the
            <router-link id="profilePageLink" variant="primary" to="/profile">
              profile page</router-link
            >.
          </span>
        </b-alert>
        <div id="pageTitle">
          <h1 id="subject">Health Care Timeline</h1>
          <hr />
        </div>
        <b-row class="no-print">
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
          </b-col>
        </b-row>
        <b-row align-h="start" class="no-print">
          <b-col v-if="isMedicationEnabled">
            <b-form-checkbox
              id="medicationFilter"
              v-model="filterTypes"
              name="medicationFilter"
              value="Medication"
            >
              Medications
            </b-form-checkbox>
          </b-col>
          <b-col v-if="isImmunizationEnabled">
            <b-form-checkbox
              id="immunizationFilter"
              v-model="filterTypes"
              name="immunizationFilter"
              value="Immunization"
            >
              Immunizations
            </b-form-checkbox>
          </b-col>
          <b-col v-if="isLaboratoryEnabled">
            <b-form-checkbox
              id="laboratoryFilter"
              v-model="filterTypes"
              name="laboratoryFilter"
              value="Laboratory"
            >
              Laboratory
            </b-form-checkbox>
          </b-col>
          <b-col v-if="isNoteEnabled">
            <b-form-checkbox
              id="notesFilter"
              v-model="filterTypes"
              name="notesFilter"
              value="Note"
            >
              Notes
            </b-form-checkbox>
          </b-col>
        </b-row>
        <br />
        <div v-if="!isLoading">
          <div id="listControlls" class="no-print">
            <b-row>
              <b-col>
                Displaying {{ getVisibleCount() }} out of
                {{ getTotalCount() }} records
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
      </b-col>
      <b-col class="col-3 col-md-2 col-lg-3 column-wrapper no-print">
        <HealthlinkComponent />
      </b-col>
    </b-row>
    <ProtectiveWordComponent
      ref="protectiveWordModal"
      :error="protectiveWordAttempts > 1"
      :is-loading="isLoading"
      @submit="onProtectiveWordSubmit"
      @cancel="onProtectiveWordCancel"
    />
    <CovidModalComponent
      ref="covidModal"
      :is-loading="isLoading"
      @submit="onCovidSubmit"
    />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Watch, Ref } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";
import moment from "moment";
import { Route } from "vue-router";
import EventBus from "@/eventbus";
import { WebClientConfiguration } from "@/models/configData";
import {
  IMedicationService,
  IImmunizationService,
  ILaboratoryService,
  IUserNoteService
} from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ResultType } from "@/constants/resulttype";
import User from "@/models/user";
import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import LaboratoryTimelineEntry from "@/models/laboratoryTimelineEntry";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import MedicationStatement from "@/models/medicationStatement";
import UserNote from "@/models/userNote";
import RequestResult from "@/models/requestResult";
import { faSearch, IconDefinition } from "@fortawesome/free-solid-svg-icons";

import LoadingComponent from "@/components/loading.vue";
import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import CovidModalComponent from "@/components/modal/covid.vue";
import EntryCardTimelineComponent from "@/components/timeline/entrycard.vue";
import HealthlinkSidebarComponent from "@/components/timeline/healthlink.vue";
import NoteTimelineComponent from "@/components/timeline/note.vue";
import { LaboratoryResult, LaboratoryReport } from "../models/laboratory";

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
    CovidModalComponent,
    EntryCardComponent: EntryCardTimelineComponent,
    HealthlinkComponent: HealthlinkSidebarComponent,
    NoteTimelineComponent
  }
})
export default class TimelineComponent extends Vue {
  @Getter("user", { namespace }) user!: User;
  @Action("getReports", { namespace: "laboratory" }) getLaboratoryReports!: ({
    hdid: string
  }: any) => Promise<RequestResult<LaboratoryReport[]>>;
  @Getter("webClient", { namespace: "config" }) config!: WebClientConfiguration;

  private filterText: string = "";
  private timelineEntries: TimelineEntry[] = [];
  private filteredTimelineEntries: TimelineEntry[] = [];
  private visibleTimelineEntries: TimelineEntry[] = [];
  private isMedicationLoading: boolean = false;
  private isImmunizationLoading: boolean = false;
  private isLaboratoryLoading: boolean = false;
  private isNoteLoading: boolean = false;
  private windowWidth: number = 0;
  private currentPage: number = 1;
  private hasErrors: boolean = false;
  private idleLogoutWarning: boolean = false;
  private protectiveWordAttempts: number = 0;
  private isAddingNote: boolean = false;
  private cardEditedId: string | undefined;
  private unsavedChangesText: string =
    "You have unsaved changes. Are you sure you want to leave?";

  private filterTypes: string[] = [];

  @Ref("protectiveWordModal")
  readonly protectiveWordModal!: ProtectiveWordComponent;
  @Ref("covidModal")
  readonly covidModal!: CovidModalComponent;

  private created() {
    window.addEventListener("resize", this.handleResize);
    this.handleResize();
  }

  private mounted() {
    this.initializeFilters();
    this.fetchMedicationStatements();
    this.fetchImmunizations();
    this.fetchLaboratoryResults();
    this.fetchNotes();
    window.addEventListener("beforeunload", this.onBrowserClose);
    let self = this;
    EventBus.$on("timelineCreateNote", function() {
      self.isAddingNote = true;
    });
    EventBus.$on("timelinePrintView", function() {
      self.printRecords();
    });
    EventBus.$on("idleLogoutWarning", function(isVisible: boolean) {
      self.idleLogoutWarning = isVisible;
    });
  }

  private beforeRouteLeave(to: Route, from: Route, next: any) {
    if (
      !this.idleLogoutWarning &&
      (this.isAddingNote || this.cardEditedId) &&
      !confirm(this.unsavedChangesText)
    ) {
      return;
    }
    next();
  }

  private destroyed() {
    window.removeEventListener("handleResize", this.handleResize);
  }

  private onBrowserClose(event: BeforeUnloadEvent) {
    if (!this.idleLogoutWarning && (this.isAddingNote || this.cardEditedId)) {
      event.returnValue = this.unsavedChangesText;
    }
  }

  private handleResize() {
    this.windowWidth = window.innerWidth;
  }

  private linkGen(pageNum: number) {
    return `?page=${pageNum}`;
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
      this.isLaboratoryLoading ||
      this.isNoteLoading
    );
  }

  private get isMedicationEnabled(): boolean {
    return (
      this.config.modules["MedicationHistory"] ||
      this.config.modules["Medication"]
    );
  }

  private get isImmunizationEnabled(): boolean {
    return this.config.modules["Immunization"];
  }

  private get isLaboratoryEnabled(): boolean {
    return (
      this.config.modules["Laboratory"] ||
      this.config.modules["CovidLabResults"]
    );
  }

  private get isNoteEnabled(): boolean {
    return this.config.modules["Note"];
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
        this.filteredTimelineEntries.length / this.numberOfEntriesPerPage
      );
    }
    return result;
  }

  private initializeFilters(): void {
    if (this.isMedicationEnabled) {
      this.filterTypes.push("Medication");
    }
    if (this.isImmunizationEnabled) {
      this.filterTypes.push("Immunization");
    }
    if (this.isLaboratoryEnabled) {
      this.filterTypes.push("Laboratory");
    }
    if (this.isNoteEnabled) {
      this.filterTypes.push("Note");
    }
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
          this.sortEntries();
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

  private onCovidSubmit() {
    this.filterTypes = ["Laboratory"];
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
          this.sortEntries();
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

  private fetchLaboratoryResults() {
    const laboratoryService: ILaboratoryService = container.get(
      SERVICE_IDENTIFIER.LaboratoryService
    );
    this.isLaboratoryLoading = true;
    this.getLaboratoryReports({ hdid: this.user.hdid })
      .then(results => {
        if (results.resultStatus == ResultType.Success) {
          // Add the laboratory entries to the timeline list
          for (let result of results.resourcePayload) {
            this.timelineEntries.push(new LaboratoryTimelineEntry(result));
          }
          this.sortEntries();
          this.applyTimelineFilter();

          if (results.resourcePayload.length > 0) {
            this.covidModal.showModal();
          }
        } else {
          console.log(
            "Error returned from the laboratory call: " + results.resultMessage
          );
          this.hasErrors = true;
        }
      })
      .catch(err => {
        this.hasErrors = true;
        console.log(err);
      })
      .finally(() => {
        this.isLaboratoryLoading = false;
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
          this.sortEntries();
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
      this.sortEntries();
      this.applyTimelineFilter();
    }
  }

  private onCardRemoved(entry: TimelineEntry) {
    const index = this.timelineEntries.findIndex(e => e.id == entry.id);
    this.timelineEntries.splice(index, 1);
  }

  private onCardEdit(entry: TimelineEntry) {
    this.cardEditedId = entry.id;
  }

  private onCardClose(entry: TimelineEntry) {
    this.cardEditedId = undefined;
  }

  private onCardUpdated(entry: TimelineEntry) {
    const index = this.timelineEntries.findIndex(e => e.id == entry.id);
    this.timelineEntries.splice(index, 1);
    this.timelineEntries.push(entry);
    this.cardEditedId = undefined;
    this.sortEntries();
    this.applyTimelineFilter();
  }

  private onProtectiveWordSubmit(value: string) {
    this.fetchMedicationStatements(value);
  }

  private onProtectiveWordCancel() {
    // Does nothing as it won't be able to fetch pharmanet data.
    console.log("protective word cancelled");
  }

  private getHeadingDate(date: Date): string {
    return moment(date).format("ll");
  }

  @Watch("filterText")
  @Watch("filterTypes")
  private applyTimelineFilter() {
    this.filteredTimelineEntries = this.timelineEntries.filter(entry =>
      entry.filterApplies(this.filterText, this.filterTypes)
    );
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
      a.date > b.date ? -1 : a.date < b.date ? 1 : 0
    );
    return groupArrays;
  }

  private getVisibleCount(): number {
    return this.visibleTimelineEntries.length;
  }

  private getTotalCount(): number {
    return this.timelineEntries.length;
  }

  private sortEntries() {
    this.timelineEntries.sort((a, b) =>
      a.date > b.date ? -1 : a.date < b.date ? 1 : 0
    );
  }

  private printRecords() {
    window.print();
  }
}
</script>
