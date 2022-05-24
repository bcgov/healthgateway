<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faDownload } from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Getter } from "vuex-class";

import Covid19LaboratoryTestDescriptionComponent from "@/components/laboratory/Covid19LaboratoryTestDescriptionComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import Covid19LaboratoryOrderTimelineEntry from "@/models/covid19LaboratoryOrderTimelineEntry";
import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryReport } from "@/models/laboratory";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILaboratoryService, ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

import EntrycardTimelineComponent from "./EntrycardTimelineComponent.vue";

library.add(faDownload);

@Component({
    components: {
        MessageModalComponent,
        EntryCard: EntrycardTimelineComponent,
        Covid19LaboratoryTestDescriptionComponent,
    },
})
export default class Covid19LaboratoryOrderTimelineComponent extends Vue {
    @Prop() entry!: Covid19LaboratoryOrderTimelineEntry;
    @Prop() index!: number;
    @Prop() datekey!: string;
    @Prop() isMobileDetails!: boolean;
    @Getter("user", { namespace: "user" }) user!: User;

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

    private laboratoryService!: ILaboratoryService;

    private isLoadingDocument = false;
    private logger!: ILogger;

    private get entryIcon(): string | undefined {
        return entryTypeMap.get(EntryType.Covid19LaboratoryOrder)?.icon;
    }

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
            .getReportDocument(this.entry.id, this.user.hdid, true)
            .then((result) => {
                let dateString =
                    this.entry.displayDate.format("yyyy_MM_dd-HH_mm");
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
        :entry-icon="entryIcon"
        :title="entry.summaryTitle"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :has-attachment="reportAvailable"
    >
        <div v-if="entry.tests.length === 1" slot="header-description">
            <strong
                v-show="entry.resultReady"
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
                        v-if="entry.resultReady"
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
            <div
                v-for="(test, index) in entry.tests"
                :key="test.id"
                :data-testid="`laboratoryTestBlock-${index}`"
            >
                <hr />
                <div data-testid="laboratoryTestType" class="my-2">
                    <strong
                        v-if="test.resultReady && entry.tests.length > 1"
                        data-testid="laboratoryTestResult"
                    >
                        <span>Result:</span>
                        <span :class="getOutcomeClasses(test.labResultOutcome)">
                            {{ test.labResultOutcome }}
                        </span>
                    </strong>
                </div>
                <div data-testid="laboratoryTestType" class="my-2">
                    <strong>Test Type:</strong>
                    {{ test.testType }}
                </div>
                <div
                    :data-testid="`laboratoryTestStatus-${index}`"
                    class="my-2"
                >
                    <strong>Test Status:</strong>
                    {{ test.testStatus }}
                </div>
                <div class="my-2">
                    <strong>Collection Date:</strong>
                    {{ formatDate(test.collectedDateTime) }}
                </div>
                <div class="my-2">
                    <strong>Result Date:</strong>
                    {{ formatDate(test.resultDateTime) }}
                </div>
                <div
                    v-if="test.resultDescription.length > 0"
                    class="my-2"
                    :data-testid="`laboratoryResultDescription-${index}`"
                >
                    <strong>Result Description:</strong>
                    <Covid19LaboratoryTestDescriptionComponent
                        :description="test.resultDescription"
                        :link="test.resultLink"
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
