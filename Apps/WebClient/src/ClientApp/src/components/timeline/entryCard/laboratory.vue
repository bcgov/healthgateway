<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faFileDownload,
    faFlask,
    IconDefinition,
} from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/genericMessage.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryReport } from "@/models/laboratory";
import LaboratoryTimelineEntry from "@/models/laboratoryTimelineEntry";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILaboratoryService, ILogger } from "@/services/interfaces";

import EntrycardTimelineComponent from "./entrycard.vue";
library.add(faFileDownload);

@Component({
    components: {
        MessageModalComponent,
        EntryCard: EntrycardTimelineComponent,
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

    private get entryIcon(): IconDefinition {
        return faFlask;
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

    private showConfirmationModal(): void {
        this.messageModal.showModal();
    }

    private getReport() {
        this.isLoadingDocument = true;
        this.laboratoryService
            .getReportDocument(this.entry.id, this.user.hdid)
            .then((result) => {
                let dateString = this.entry.displayDate.format(
                    "YYYY_MM_DD-HH_mm"
                );
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
        <div slot="header-description">
            <strong
                v-show="entry.isTestResultReady"
                data-testid="laboratoryHeaderDescription"
            >
                Result:
                <span :class="entry.labResultOutcome"
                    >{{ entry.labResultOutcome }}
                </span></strong
            >
        </div>

        <div slot="details-body">
            <div v-if="reportAvailable" data-testid="laboratoryReportAvailable">
                <b-spinner v-if="isLoadingDocument"></b-spinner>
                <span v-else data-testid="laboratoryReport">
                    <strong>Report:</strong>
                    <b-btn
                        v-if="entry.isTestResultReady"
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
            </div>
            <div class="detailSection">
                {{ entry.summaryDescription }}
            </div>

            <div class="detailSection">
                <div>
                    <strong>Ordering Providers:</strong>
                    {{ entry.orderingProviders }}
                </div>
                <div data-testid="laboratoryReportingLab">
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
                    <div data-testid="laboratoryTestType">
                        <strong>Test Type:</strong>
                        {{ result.testType }}
                    </div>
                    <div>
                        <strong>Out Of Range:</strong>
                        {{ result.outOfRange }}
                    </div>
                    <div data-testid="laboratoryTestStatus">
                        <strong>Test Status:</strong>
                        {{ result.testStatus }}
                    </div>
                    <div class="my-2">
                        <strong>Result Description:</strong>
                        <p v-html="result.resultDescription"></p>
                    </div>
                    <div>
                        <strong>Collected Date Time:</strong>
                        {{ formatDate(result.collectedDateTime) }}
                    </div>

                    <div>
                        <strong>Received Date Time:</strong>
                        {{ formatDate(result.receivedDateTime) }}
                    </div>
                </div>
            </div>

            <div class="detailSection">
                <div>
                    <strong>What to expect next</strong>
                    <p>
                        If you receive a
                        <strong>positive</strong> COVID-19 result:
                    </p>
                    <ul>
                        <li>
                            You and the people you live with need to
                            self-isolate now.
                        </li>
                        <li>
                            Public health will contact you in 2 to 3 days with
                            further instructions.
                        </li>
                        <li>
                            If you are a health care worker, please notify your
                            employer of your positive result.
                        </li>
                        <li>
                            Monitor your health and contact a health care
                            provider or call 8-1-1 if you are concerned about
                            your symptoms.
                        </li>
                        <li>
                            Go to
                            <a
                                href="http://www.bccdc.ca/results"
                                target="blank_"
                                >www.bccdc.ca/results</a
                            >
                            for more information about your test result.
                        </li>
                    </ul>
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

.detailSection {
    margin-top: 15px;
}

span.Positive {
    color: red;
}
span.Negative {
    color: green;
}
</style>
