<style lang="scss" scoped>
.error-template {
  padding: 40px 15px;
}
</style>

<template>
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
              v-b-toggle="'entryDetails-' + index + '-' + dateGroup.key"
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
            <b-collapse :id="'entryDetails-' + index + '-' + dateGroup.key">
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
</template>

<script lang="ts">
import Vue from "vue";
import PageError from "@/models/pageError";
import { Prop, Component } from "vue-property-decorator";

@Component
export default class MedicationTimelineComponent extends Vue {
  @Prop(PageError) error!: PageError;
}
</script>
