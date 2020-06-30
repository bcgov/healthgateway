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

.commentSection {
    padding-left: 0px;
    padding-right: 0px;
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
                <font-awesome-icon
                    :icon="entryIcon"
                    size="2x"
                ></font-awesome-icon>
            </b-col>
            <b-col class="entryTitle">
                <b-row class="justify-content-between">
                    <b-col cols="auto">
                        <strong>{{ entry.summaryTestType }}</strong>
                    </b-col>
                    <b-col cols="auto" class="text-muted">
                        <strong> Status: {{ entry.summaryStatus }}</strong>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
        <b-row class="my-2">
            <b-col class="leftPane"></b-col>
            <b-col>
                <b-row class="justify-content-between">
                    <b-col cols="8">
                        {{ entry.summaryDescription }}
                    </b-col>
                    <b-col v-if="reportAvailiable" cols="auto" class="pr-0">
                        <b-spinner v-if="isLoadingDocument"></b-spinner>
                        <span v-else>
                            <strong>Report:</strong>
                            <b-btn
                                variant="link"
                                @click="showConfirmationModal()"
                            >
                                <font-awesome-icon
                                    icon="file-download"
                                    aria-hidden="true"
                                    size="1x"
                                />
                            </b-btn>
                        </span>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <div class="d-flex flex-row-reverse">
                            <b-btn
                                v-b-toggle="
                                    'entryDetails-' + index + '-' + datekey
                                "
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
                        <b-collapse
                            :id="'entryDetails-' + index + '-' + datekey"
                            v-model="detailsVisible"
                        >
                            <div>
                                <div class="detailSection">
                                    <div>
                                        <strong>Ordering Providers:</strong>
                                        {{ entry.orderingProviders }}
                                    </div>
                                    <div>
                                        <strong>Reporting Lab:</strong>
                                        {{ entry.reportingLab }}
                                    </div>
                                    <div>
                                        <strong>Location:</strong>
                                        {{ entry.location }}
                                    </div>
                                </div>

                                <div class="detailSection">
                                    <strong>Results:</strong>
                                    <div
                                        v-for="result in entry.resultList"
                                        :key="result.id"
                                        class="border p-1"
                                    >
                                        <div>
                                            <strong>Test Type:</strong>
                                            {{ result.testType }}
                                        </div>
                                        <div>
                                            <strong>Out Of Range:</strong>
                                            {{ result.outOfRange }}
                                        </div>
                                        <div>
                                            <strong>Test Status:</strong>
                                            {{ result.testStatus }}
                                        </div>
                                        <div class="my-2">
                                            <strong>Result Description:</strong>
                                            <p
                                                v-html="
                                                    result.resultDescription
                                                "
                                            ></p>
                                        </div>
                                        <div>
                                            <strong
                                                >Collected Date Time:</strong
                                            >
                                            {{
                                                formatDate(
                                                    result.collectionDateTime
                                                )
                                            }}
                                        </div>

                                        <div>
                                            <strong>Received Date Time:</strong>
                                            {{
                                                formatDate(
                                                    result.receivedDateTime
                                                )
                                            }}
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </b-collapse>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
        <b-row>
            <b-col class="commentSection">
                <CommentSection :parent-entry="entry"></CommentSection>
            </b-col>
        </b-row>
        <MessageModalComponent
            ref="messageModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="getReport"
        />
    </b-col>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Action, Getter, State } from "vuex-class";
import { IconDefinition, faFlask } from "@fortawesome/free-solid-svg-icons";
import LaboratoryTimelineEntry, {
    LaboratoryResultViewModel,
} from "@/models/laboratoryTimelineEntry";
import { LaboratoryOrder, LaboratoryReport } from "@/models/laboratory";
import CommentSectionComponent from "@/components/timeline/commentSection.vue";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
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
        MessageModalComponent,
        CommentSection: CommentSectionComponent,
    },
})
export default class LaboratoryTimelineComponent extends Vue {
    @Prop() entry!: LaboratoryTimelineEntry;
    @Prop() index!: number;
    @Prop() datekey!: string;
    @Getter("user", { namespace: "user" }) user!: User;

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

    private laboratoryService!: ILaboratoryService;

    private hasErrors: boolean = false;
    private detailsVisible: boolean = false;
    private isLoadingDocument: boolean = false;

    private mounted() {
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

    private showConfirmationModal(): void {
        this.messageModal.showModal();
    }

    private getReport() {
        this.isLoadingDocument = true;
        this.laboratoryService
            .getReportDocument(this.entry.id, this.user.hdid)
            .then((result) => {
                const link = document.createElement("a");
                let dateString = moment(this.entry.displayDate)
                    .local()
                    .format("YYYY_MM_DD-HH_mm");
                let report: LaboratoryReport = result.resourcePayload;
                link.href = `data:${report.mediaType};${report.encoding},${report.data}`;
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

    private get reportAvailiable(): boolean {
        return this.entry.reportAvailable;
    }
}
</script>
