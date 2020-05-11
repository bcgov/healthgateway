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

.commentButton {
  border-radius: $radius;
}

.newComment {
  border-radius: $radius;
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
        {{ entry.testType }}
      </b-col>
    </b-row>
    <b-row class="my-2">
      <b-col class="leftPane"></b-col>
      <b-col>
        <b-row>
          <b-col>
            {{ entry.loincName }}
          </b-col>
        </b-row>
        <b-row>
          <b-col>
            <div class="d-flex flex-row-reverse">
              <b-btn
                v-b-toggle="'entryDetails-' + index + '-' + datekey"
                variant="link"
                class="detailsButton"
                @click="toggleDetails()"
              >
                <span class="when-opened">
                  <font-awesome-icon
                    icon="chevron-up"
                    aria-hidden="true"
                  ></font-awesome-icon
                ></span>
                <span class="when-closed">
                  <font-awesome-icon
                    icon="chevron-down"
                    aria-hidden="true"
                  ></font-awesome-icon
                ></span>
                <span v-if="detailsVisible">Hide Details</span>
                <span v-else>View Details</span>
              </b-btn>
            </div>
            <b-collapse :id="'entryDetails-' + index + '-' + datekey">
              <div>
                <div class="detailSection">
                  <div><strong>Id:</strong> {{ entry.id }}</div>
                  <div><strong>Phn:</strong> {{ entry.phn }}</div>
                  <div>
                    <strong>OrderingProviderIds:</strong>
                    {{ entry.orderingProviderIds }}
                  </div>
                  <div>
                    <strong>OrderingProviders:</strong>
                    {{ entry.orderingProviders }}
                  </div>
                  <div>
                    <strong>ReportingLab:</strong> {{ entry.reportingLab }}
                  </div>
                  <div><strong>Location:</strong> {{ entry.location }}</div>
                  <div><strong>OrmOrOru:</strong> {{ entry.ormOrOru }}</div>
                  <div>
                    <strong>MessageDateTime:</strong>
                    {{ formatDate(entry.messageDateTime) }}
                  </div>
                  <div><strong>MessageId:</strong> {{ entry.messageId }}</div>
                  <div>
                    <strong>AdditionalData:</strong> {{ entry.additionalData }}
                  </div>
                </div>
                <strong>LabResults:</strong>
                <div class="detailSection">
                  <div><strong>ResultId:</strong>{{ entry.labResultId }}</div>
                  <div><strong>TestType:</strong>{{ entry.testType }}</div>
                  <div><strong>OutOfRange:</strong>{{ entry.outOfRange }}</div>
                  <div>
                    <strong>CollectedDateTime:</strong
                    >{{ formatDate(entry.collectedDateTime) }}
                  </div>
                  <div><strong>TestStatus:</strong>{{ entry.testStatus }}</div>
                  <div>
                    <strong>ReceivedDateTime:</strong
                    >{{ formatDate(entry.receivedDateTime) }}
                  </div>
                  <div>
                    <strong>ResultDateTime:</strong
                    >{{ formatDate(entry.resultDateTime) }}
                  </div>
                  <div><strong>Loinc:</strong>{{ entry.loinc }}</div>
                  <div><strong>LoincName:</strong>{{ entry.loincName }}</div>
                </div>
                <div class="detailSection">
                  <div>
                    <strong>ResultDescription:</strong>
                    <p v-html="entry.resultDescription"></p>
                  </div>
                </div>
              </div>
            </b-collapse>
          </b-col>
        </b-row>
        <CommentSection :parent-entry="entry"></CommentSection>
      </b-col>
    </b-row>
  </b-col>
</template>

<script lang="ts">
import Vue from "vue";
import { Prop, Component } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";
import { faFlask, IconDefinition } from "@fortawesome/free-solid-svg-icons";
import LaboratoryTimelineEntry from "@/models/laboratoryTimelineEntry";
import { LaboratoryResult, LaboratoryReport } from "@/models/laboratory";
import CommentSectionComponent from "@/components/timeline/commentSection.vue";

@Component({
  components: {
    CommentSection: CommentSectionComponent
  }
})
export default class LaboratoryTimelineComponent extends Vue {
  @Prop() entry!: LaboratoryTimelineEntry;
  @Prop() index!: number;
  @Prop() datekey!: string;

  private hasErrors: boolean = false;
  private detailsVisible: boolean = false;

  private get entryIcon(): IconDefinition {
    return faFlask;
  }

  private toggleDetails(): void {
    this.detailsVisible = !this.detailsVisible;
    this.hasErrors = false;
  }

  private formatDate(date: Date): string {
    return new Date(Date.parse(date + "Z")).toLocaleString();
  }
}
</script>
