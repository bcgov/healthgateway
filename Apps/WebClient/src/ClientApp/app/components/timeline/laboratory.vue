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
        {{ entry.summaryTestType }}
      </b-col>
    </b-row>
    <b-row class="my-2">
      <b-col class="leftPane"></b-col>
      <b-col>
        <b-row>
          <b-col cols="8">
            {{ entry.summaryDescription }}
          </b-col>
          <b-col> <strong>Status:</strong> {{ entry.summaryStatus }} </b-col>
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
                  <div>
                    <strong>Ordering Providers:</strong>
                    {{ entry.orderingProviders }}
                  </div>
                  <div>
                    <strong>Reporting Lab:</strong> {{ entry.reportingLab }}
                  </div>
                  <div><strong>Location:</strong> {{ entry.location }}</div>
                </div>

                <div class="detailSection">
                  <strong>Results:</strong>
                  <div
                    v-for="result in entry.resultList"
                    :key="result.id"
                    class="border p-1"
                  >
                    <div><strong>Test Type:</strong> {{ result.testType }}</div>
                    <div>
                      <strong>Out Of Range:</strong> {{ result.outOfRange }}
                    </div>
                    <div>
                      <strong>Test Status:</strong> {{ result.testStatus }}
                    </div>
                    <div>
                      <strong>Result Description:</strong>
                      <p v-html="result.resultDescription"></p>
                    </div>
                    <div>
                      <strong>Collected Date Time:</strong>
                      {{ formatDate(result.collectionDateTime) }}
                    </div>

                    <div>
                      <strong>Received Date Time:</strong>
                      {{ formatDate(result.receivedDateTime) }}
                    </div>
                    <div>
                      <strong>Report Document:</strong>
                      <b-btn variant="link" @click="getDocument(result)">
                        <font-awesome-icon
                          icon="file-download"
                          aria-hidden="true"
                          size="1x"
                        />
                      </b-btn>
                      <b-spinner v-if="isLoadingDocument"></b-spinner>
                    </div>
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
import LaboratoryTimelineEntry, {
  LaboratoryResultViewModel
} from "@/models/laboratoryTimelineEntry";
import { LaboratoryResult, LaboratoryReport } from "@/models/laboratory";
import CommentSectionComponent from "@/components/timeline/commentSection.vue";
import { ILaboratoryService } from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import User from "@/models/user";
import moment from "moment";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faFileDownload } from "@fortawesome/free-solid-svg-icons";
library.add(faFileDownload);

@Component({
  components: {
    CommentSection: CommentSectionComponent
  }
})
export default class LaboratoryTimelineComponent extends Vue {
  @Prop() entry!: LaboratoryTimelineEntry;
  @Prop() index!: number;
  @Prop() datekey!: string;

  private laboratoryService!: ILaboratoryService;

  private hasErrors: boolean = false;
  private detailsVisible: boolean = false;
  private isLoadingDocument: boolean = false;

  mounted() {
    this.laboratoryService = container.get<ILaboratoryService>(
      SERVICE_IDENTIFIER.LaboratoryService
    );
  }

  private get entryIcon(): IconDefinition {
    return faFlask;
  }

  private toggleDetails(): void {
    this.detailsVisible = !this.detailsVisible;
    this.hasErrors = false;
  }

  private formatDate(date: Date): string {
    return moment(date).format("lll");
  }

  private getDocument(report: LaboratoryResultViewModel) {
    this.isLoadingDocument = true;
    this.laboratoryService
      .getReportDocument(report.id)
      .then(result => {
        const link = document.createElement("a");
        let dateString = moment(report.resultDateTime)
          .local()
          .format("YYYY_MM_DD-HH_mm");
        link.href = result.resourcePayload.base64Pdf;
        link.download = `COVID_Result_${dateString}.pdf`;
        link.click();
        URL.revokeObjectURL(link.href);
      })
      .catch(() => {
        this.hasErrors = true;
      })
      .finally(() => {
        this.isLoadingDocument = false;
      });
  }
}
</script>
