<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

$radius: 15px;

.timelineCard {
  border-radius: $radius $radius $radius $radius;
  border-color: $soft_background;
  border-style: solid;
  border-width: 2px;
}

.entryTitle {
  background-color: $soft_background;
  color: $primary;
  padding: 13px 15px;
  font-weight: bold;
  margin-right: -1px;
  border-radius: 0px $radius 0px 0px;
}

.icon {
  background-color: $primary;
  color: white;
  text-align: center;
  padding: 10px 0;
  border-radius: $radius 0px 0px 0px;
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
  <b-col class="timelineCard">
    <b-row class="entryHeading">
      <b-col class="icon leftPane">
        <font-awesome-icon :icon="entryIcon" size="2x"></font-awesome-icon>
      </b-col>
      <b-col class="entryTitle">
        {{ entry.immunization.name }}
      </b-col>
    </b-row>
    <b-row class="my-2">
      <b-col class="leftPane"></b-col>
      <b-col>
        <b-row>
          <b-col>
            {{ entry.immunization.agents }}
          </b-col>
        </b-row>
        <CommentSection :parent-entry="entry"></CommentSection>
      </b-col>
    </b-row>
  </b-col>
</template>

<script lang="ts">
import Vue from "vue";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import { Prop, Component } from "vue-property-decorator";
import { IUserCommentService } from "@/services/interfaces";
import CommentSectionComponent from "@/components/timeline/commentSection.vue";

import { faSyringe, IconDefinition } from "@fortawesome/free-solid-svg-icons";

@Component({
  components: {
    CommentSection: CommentSectionComponent
  }
})
export default class ImmunizationTimelineComponent extends Vue {
  @Prop() entry!: ImmunizationTimelineEntry;
  @Prop() index!: number;
  @Prop() datekey!: string;

  private get entryIcon(): IconDefinition {
    return faSyringe;
  }
}
</script>
