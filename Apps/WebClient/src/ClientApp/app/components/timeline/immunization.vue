<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

$radius: 15px;

.entryHeading {
  border-radius: 25px;
}

.entryCard {
  width: 100%;
  padding-left: 50px;
  padding-top: 10px;
  padding-bottom: 10px;
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
  max-width: 60px;
}

.detailsButton {
  padding: 0px;
}

.detailSection {
  margin-top: 15px;
}

.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
  display: none;
}
</style>

<template>
  <b-row class="entryCard">
    <b-col>
      <b-row class="entryHeading">
        <b-col class="icon leftPane">
          <font-awesome-icon
            :icon="getEntryIcon(entry)"
            size="2x"
          ></font-awesome-icon>
        </b-col>
        <b-col class="entryTitle">
          {{ entry.immunization.name }}
        </b-col>
      </b-row>
      <b-row>
        <b-col class="leftPane"></b-col>
        <b-col>
          <b-row>
            <b-col>
              {{ entry.immunization.immunizationAgentDisplay }}
            </b-col>
          </b-row>
          <b-row>
            <b-col>
              <b-btn
                v-b-toggle="'entryDetails-' + index + '-' + datekey"
                variant="link"
                class="detailsButton"
                @click="toggleDetails(entry)"
              >
                <span class="when-opened">
                  <font-awesome-icon
                    icon="chevron-down"
                    aria-hidden="true"
                  ></font-awesome-icon
                ></span>
                <span class="when-closed">
                  <font-awesome-icon
                    icon="chevron-up"
                    aria-hidden="true"
                  ></font-awesome-icon
                ></span>
                <span v-if="detailsVisible">Hide Details</span>
                <span v-else>View Details</span>
              </b-btn>
              <b-collapse :id="'entryDetails-' + index + '-' + datekey">
                <div>
                  <div class="detailSection">
                    <div>
                      <strong>Agent Code:</strong>
                      {{ entry.immunization.immunizationAgentCode }}
                    </div>
                  </div>
                  <div class="detailSection">
                    <div>
                      <strong>Status:</strong>
                      {{ entry.immunization.status }}
                    </div>
                  </div>
                </div>
                <div v-else-if="isLoading">
                  <div class="d-flex align-items-center">
                    <strong>Loading...</strong>
                    <b-spinner class="ml-5"></b-spinner>
                  </div>
                </div>
                <div v-else-if="hasErrors" class="pt-1">
                  <b-alert :show="hasErrors" variant="danger">
                    <h5>Error</h5>
                    <span
                      >An unexpected error occured while processing the
                      request.</span
                    >
                  </b-alert>
                </div>
              </b-collapse>
            </b-col>
          </b-row>
        </b-col>
      </b-row>
    </b-col>
  </b-row>
</template>

<script lang="ts">
import Vue from "vue";
// import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import { Prop, Component } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";

import { faVaccine, IconDefinition } from "@fortawesome/free-solid-svg-icons";

@Component
export default class ImmunizationTimelineComponent extends Vue {
  @Prop() entry!: any;
  @Prop() index!: number;
  @Prop() datekey!: string;
  private isLoading: boolean = false;
  private hasErrors: boolean = false;

  private detailsVisible = false;

  private getEntryIcon(entry: any): IconDefinition {
    return faVaccine;
  }

  private toggleDetails(immunizationEntry: any): void {
    this.detailsVisible = !this.detailsVisible;
    this.hasErrors = false;
  }
}
</script>
