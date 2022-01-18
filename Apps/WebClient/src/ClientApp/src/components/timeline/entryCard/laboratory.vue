<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faDownload, faFlask } from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Getter } from "vuex-class";

import LaboratoryResultDescriptionComponent from "@/components/laboratory/laboratoryResultDescription.vue";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryReport } from "@/models/laboratory";
import LaboratoryTimelineEntry from "@/models/laboratoryTimelineEntry";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILaboratoryService, ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

import EntrycardTimelineComponent from "./entrycard.vue";

library.add(faDownload, faFlask);

@Component({
    components: {
        MessageModalComponent,
        EntryCard: EntrycardTimelineComponent,
        LaboratoryResultDescriptionComponent,
    },
})
export default class LaboratoryTimelineComponent extends Vue {
    @Prop() entry!: LaboratoryTimelineEntry;
    @Prop() index!: number;
    @Prop() datekey!: string;
    @Prop() isMobileDetails!: boolean;
    @Getter("user", { namespace: "user" }) user!: User;

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

    private laboratoryService!: ILaboratoryService;

    private isLoadingDocument = false;
    private logger!: ILogger;

    private get reportAvailable(): boolean {
        return this.entry.reportAvailable;
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.laboratoryService = container.get<ILaboratoryService>(
            SERVICE_IDENTIFIER.LaboratoryService
        );
    }

    private formatDate(date: DateWrapper): string {
        return date.format("yyyy-MMM-dd, t");
    }

    private getOutcomeClasses(outcome: string): string[] {
        switch (outcome?.toUpperCase()) {
            case "NEGATIVE":
                return ["text-success"];
            case "POSITIVE":
                return ["text-danger"];
            default:
                return [];
        }
    }

    private showConfirmationModal(): void {
        this.messageModal.showModal();
    }

    private getReport() {
        SnowPlow.trackEvent({
            action: "download_report",
            text: "COVID Test PDF",
        });

        this.isLoadingDocument = true;
        this.laboratoryService
            .getReportDocument(this.entry.id, this.user.hdid)
            .then((result) => {
                let dateString =
                    this.entry.displayDate.format("YYYY_MM_DD-HH_mm");
                let report: LaboratoryReport = result.resourcePayload;
                fetch(
                    `data:${report.mediaType};${report.encoding},${report.data}`
                )
                    .then((response) => response.blob())
                    .then((blob) => {
                        saveAs(blob, `COVID_Result_${dateString}.pdf`);
                    });
            })
            .catch((err) => {
                this.logger.error(err);
            })
            .finally(() => {
                this.isLoadingDocument = false;
            });
    }
}
</script>

<template>
    <EntryCard
        :card-id="index + '-' + datekey"
        entry-icon="flask"
        :title="entry.summaryTitle"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :has-attachment="reportAvailable"
    >
        <div v-if="entry.resultList.length === 1" slot="header-description">
            <strong
                v-show="entry.isTestResultReady"
                data-testid="laboratoryHeaderDescription"
            >
                <span>Result:</span>
                <span :class="getOutcomeClasses(entry.labResultOutcome)">
                    {{ entry.labResultOutcome }}
                </span>
            </strong>
        </div>
        <div slot="details-body">
            <div
                v-if="reportAvailable"
                data-testid="laboratoryReportAvailable"
                class="mt-2 mb-n1"
            >
                <b-spinner v-if="isLoadingDocument" class="mb-1" />
                <span v-else data-testid="laboratoryReport">
                    <strong class="align-bottom d-inline-block pb-1">
                        Report:
                    </strong>
                    <hg-button
                        v-if="entry.isTestResultReady"
                        variant="link"
                        class="p-1 ml-1"
                        @click="showConfirmationModal()"
                    >
                        <hg-icon
                            icon="download"
                            size="medium"
                            square
                            aria-hidden="true"
                        />
                    </hg-button>
                </span>
            </div>
            <div class="my-2">
                <div data-testid="laboratoryReportingLab">
                    <strong>Reporting Lab:</strong>
                    {{ entry.reportingLab }}
                </div>
            </div>
            <div v-for="result in entry.resultList" :key="result.id">
                <hr />
                <div data-testid="laboratoryTestType" class="my-2">
                    <strong
                        v-if="
                            result.isTestResultReady &&
                            entry.resultList.length > 1
                        "
                        data-testid="laboratoryTestResult"
                    >
                        <span>Result:</span>
                        <span
                            :class="getOutcomeClasses(entry.labResultOutcome)"
                        >
                            {{ entry.labResultOutcome }}
                        </span>
                    </strong>
                </div>
                <div data-testid="laboratoryTestType" class="my-2">
                    <strong>Test Type:</strong>
                    {{ result.testType }}
                </div>
                <div data-testid="laboratoryTestStatus" class="my-2">
                    <strong>Test Status:</strong>
                    {{ result.testStatus }}
                </div>
                <div class="my-2">
                    <strong>Collection Date:</strong>
                    {{ formatDate(result.collectedDateTime) }}
                </div>
                <div class="my-2">
                    <strong>Result Date:</strong>
                    {{ formatDate(result.resultDateTime) }}
                </div>
                <div v-if="result.resultDescription.length > 0" class="my-2">
                    <strong>Result Description:</strong>
                    <LaboratoryResultDescriptionComponent
                        :description="result.resultDescription"
                        :link="result.resultLink"
                    />
                </div>
            </div>
            <MessageModalComponent
                ref="messageModal"
                title="Sensitive Document Download"
                message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
                @submit="getReport"
            />
        </div>
    </EntryCard>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.col {
    padding: 0px;
    margin: 0px;
}
.row {
    padding: 0;
    margin: 0px;
}
</style>
