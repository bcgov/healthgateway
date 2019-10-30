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
  <b-row class="entryCard">
    <b-col>
      <b-row class="entryHeading">
        <b-col class="icon leftPane" cols="0">
          <i :class="'fas fa-2x ' + getEntryIcon(entry)"></i>
        </b-col>
        <b-col class="entryTitle">
          {{ entry.medicationSumary.brandName }}
        </b-col>
      </b-row>
      <b-row>
        <b-col class="leftPane" cols="0"> </b-col>
        <b-col>
          <b-row>
            <b-col>
              {{ entry.medicationSumary.genericName }}
            </b-col>
          </b-row>
          <b-row>
            <b-col>
              <b-btn
                v-b-toggle="'entryDetails-' + index + '-' + dateKey"
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
              <b-collapse :id="'entryDetails-' + index + '-' + dateKey">
                <b-col>
                  <div v-for="detail in entry.details" :key="detail.name">
                    <strong>{{ detail.name }}:</strong>
                    {{ detail.value }}
                  </div>
                </b-col>
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
import MedicationStatement from "@/models/medicationStatement";
import { Prop, Component } from "vue-property-decorator";

@Component
export default class MedicationTimelineComponent extends Vue {
  @Prop() entry!: MedicationStatement;
  @Prop() index!: number;
  @Prop() dateKey!: string;

  private getEntryIcon(entry: any): string {
    return "fa-pills";
  }
}
</script>
