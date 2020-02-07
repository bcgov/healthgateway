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
      v-if="entry.type === type"
      :key="key"
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

@Component({
  components: {
    MedicationComponent: MedicationTimelineComponent
  }
})
export default class EntrycardTimelineComponent extends Vue {
  @Prop() key!: string;
  @Prop() datekey!: string;
  @Prop() entry!: TimelineEntry;
  @Prop() index!: number;
  private type: EntryType = EntryType.Medication;
}
</script>
