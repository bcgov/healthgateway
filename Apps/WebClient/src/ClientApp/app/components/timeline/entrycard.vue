<style lang="scss" scoped>
.entryCard {
  width: 100%;
  padding-left: 50px;
  padding-top: 10px;
  padding-bottom: 10px;
}
</style>

<template>
  <b-row class="entryCard">
    <MedicationComponent
      v-if="entry.type === EntryType.Medication"
      :key="entry.id"
      :datekey="datekey"
      :entry="entry"
      :index="index"
    />
    <ImmunizationComponent
      v-if="entry.type === EntryType.Immunization"
      :key="entry.id"
      :datekey="datekey"
      :entry="entry"
      :index="index"
    />
  </b-row>
</template>

<script lang="ts">
import Vue from "vue";

import TimelineEntry, { EntryType } from "@/models/timelineEntry";

import { Prop, Component } from "vue-property-decorator";
import MedicationTimelineComponent from "./medication.vue";
import ImmunizationTimelineComponent from "./immunization.vue";

@Component({
  components: {
    MedicationComponent: MedicationTimelineComponent,
    ImmunizationComponent: ImmunizationTimelineComponent
  }
})
export default class EntrycardTimelineComponent extends Vue {
  @Prop() datekey!: string;
  @Prop() entry!: TimelineEntry;
  @Prop() index!: number;

  get EntryType(): EntryType {
    return EntryType;
  }
}
</script>
