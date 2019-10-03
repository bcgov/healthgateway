<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

$radius: 15px;

#pageTitle {
  color: $primary;
}
#pageTitle hr {
  border-top: 2px solid $primary;
}

.dateEntry {
  padding: 20px;
}

.dateBreakLine {
  border-top: dashed 2px $primary;
}
.date {
  padding-top: 0px;
  color: $primary;
  font-size: 1.3em;
}

.entryHeading {
  border-radius: 25px;
}

.entryCard {
  padding-left: 70px;
  padding-top: 10px;
}

.entryTitle {
  background-color: $soft_background;
  color: $primary;
  padding: 13px 15px;
  font-weight: bold;
}

.icon {
  background-color: $primary;
  color: white;
  text-align: center;
  padding: 10px 0;
  border-radius: $radius 0px 0px $radius;
}

.leftPane {
  width: 60px;
}

.detailsButton {
  padding: 0px;
}

.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
  display: none;
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
    <b-container id="example-1">
      <!--TODO: The DIN might not be unique. Instead we need to pass the ID of this statement-->
      <b-row v-for="(entry, index) in timelineEntries" :key="entry.id">
        <b-col class="dateEntry">
          <b-row>
            <b-col cols="auto">
              <div class="date">
                {{ getHeadingDate(entry.date) }}
              </div>
            </b-col>
            <b-col>
              <hr class="dateBreakLine" />
            </b-col>
          </b-row>
          <b-row class="entryCard">
            <b-col>
              <b-row class="entryHeading">
                <b-col class="icon leftPane" cols="0">
                  <i :class="'fas fa-2x ' + getEntryIcon(entry)"></i>
                </b-col>
                <b-col class="entryTitle">
                  {{ entry.title }}
                </b-col>
              </b-row>
              <b-row>
                <b-col class="leftPane" cols="0"> </b-col>
                <b-col>
                  <b-row>
                    <b-col>
                      {{ entry.description }}
                    </b-col>
                  </b-row>
                  <b-row>
                    <b-col>
                      <b-btn
                        v-b-toggle="'entryDetails-' + index"
                        variant="link"
                        class="detailsButton"
                      >
                        <span class="when-opened">
                          <i class="fa fa-chevron-down" aria-hidden="true"></i
                        ></span>
                        <span class="when-closed">
                          <i class="fa fa-chevron-up" aria-hidden="true"></i
                        ></span>
                        View Details
                      </b-btn>
                      <b-collapse :id="'entryDetails-' + index">
                        The details of the Entry go here
                      </b-collapse>
                    </b-col>
                  </b-row>
                </b-col>
              </b-row>
            </b-col>
          </b-row>
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
import * as moment from "moment";

const namespace: string = "user";

@Component({
  components: {
    LoadingComponent
  }
})
export default class TimelineComponent extends Vue {
  @Getter("user", { namespace }) user: User;
  private timelineEntries: TimelineEntry[] = [];
  private isLoading: boolean = false;
  private hasErrors: boolean = false;
  private sortyBy: string = "date";
  private sortDesc: boolean = false;

  private types = EntryType;

  mounted() {
    this.isLoading = true;
    const medicationService: IMedicationService = container.get(
      SERVICE_IDENTIFIER.MedicationService
    );
    console.log(this.timelineEntries);
    medicationService
      .getPatientMedicationStatemens(this.user.hdid)
      .then(results => {
        for (let result of results) {
          this.timelineEntries.push(new TimelineEntry(result));
        }

        //this.timelineEntries = results;
        console.log(this.timelineEntries);
        console.log(this.timelineEntries[0].date);
      })
      .catch(err => {
        this.hasErrors = true;
        console.log(err);
      })
      .finally(() => {
        this.isLoading = false;
      });
  }

  private getHeadingDate(date: Date): string {
    return moment(date).format("ll");
  }

  private getEntryIcon(entry: TimelineEntry): string {
    let iconClass = "fa-times";
    switch (entry.type) {
      case EntryType.Medication:
        iconClass = "fa-prescription";
        break;
      case EntryType.Laboratory:
        iconClass = "fa-flask";
        break;
    }
    return iconClass;
  }
}
</script>
