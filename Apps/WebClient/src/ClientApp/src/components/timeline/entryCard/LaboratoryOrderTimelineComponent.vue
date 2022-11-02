<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faDownload } from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import LaboratoryOrderTimelineEntry from "@/models/laboratoryOrderTimelineEntry";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILaboratoryService, ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

import EntrycardTimelineComponent from "./EntrycardTimelineComponent.vue";

library.add(faDownload);

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        MessageModalComponent,
        EntryCard: EntrycardTimelineComponent,
    },
};

@Component(options)
export default class LaboratoryOrderTimelineComponent extends Vue {
    @Prop() entry!: LaboratoryOrderTimelineEntry;
    @Prop() index!: number;
    @Prop() datekey!: string;
    @Prop() isMobileDetails!: boolean;
    @Getter("user", { namespace: "user" }) user!: User;

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

    @Action("addError", { namespace: "errorBanner" })
    addError!: (params: {
        errorType: ErrorType;
        source: ErrorSourceType;
        traceId: string | undefined;
    }) => void;

    @Action("setTooManyRequestsError", { namespace: "errorBanner" })
    setTooManyRequestsError!: (params: { key: string }) => void;

    private laboratoryService!: ILaboratoryService;

    private isLoadingDocument = false;
    private logger!: ILogger;

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.laboratoryService = container.get<ILaboratoryService>(
            SERVICE_IDENTIFIER.LaboratoryService
        );
    }

    private get entryIcon(): string | undefined {
        return entryTypeMap.get(EntryType.LaboratoryOrder)?.icon;
    }

    private formatDate(date: DateWrapper): string {
        return date.format("yyyy-MMM-dd, t");
    }

    private getStatusInfoId(labPdfId: string, index: number): string {
        const isModalIndicator: string = this.isMobileDetails ? "1" : "0";
        return `laboratory-test-status-info-${labPdfId}-${index}-${isModalIndicator}`;
    }

    private showConfirmationModal(): void {
        this.messageModal.showModal();
    }

    private getReport(): void {
        SnowPlow.trackEvent({
            action: "download_report",
            text: "Laboratory Report PDF",
        });

        this.isLoadingDocument = true;
        this.laboratoryService
            .getReportDocument(this.entry.id, this.user.hdid, false)
            .then((result) => {
                const dateString =
                    this.entry.timelineDateTime.format("yyyy_MM_dd-HH_mm");
                const report = result.resourcePayload;
                fetch(
                    `data:${report.mediaType};${report.encoding},${report.data}`
                )
                    .then((response) => response.blob())
                    .then((blob) =>
                        saveAs(blob, `Laboratory_Report_${dateString}.pdf`)
                    );
            })
            .catch((err: ResultError) => {
                this.logger.error(err.resultMessage);
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Download,
                        source: ErrorSourceType.LaboratoryReport,
                        traceId: err.traceId,
                    });
                }
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
        :title="entry.commonName"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :has-attachment="entry.reportAvailable"
    >
        <div slot="header-description">
            <span>Order Status: </span>
            <strong data-testid="laboratory-header-order-status">
                {{ entry.orderStatus }}
            </strong>
        </div>
        <div slot="details-body">
            <div class="my-2">
                <div data-testid="laboratory-collection-date">
                    <strong>Collection Date: </strong>
                    <span
                        v-if="entry.collectionDateTime !== undefined"
                        data-testid="laboratory-collection-date-value"
                    >
                        {{ formatDate(entry.collectionDateTime) }}
                    </span>
                </div>
            </div>
            <div class="my-2">
                <div data-testid="laboratory-ordering-provider">
                    <strong>Ordering Provider: </strong>
                    <span>{{ entry.orderingProvider }}</span>
                </div>
            </div>
            <div class="my-2">
                <div data-testid="laboratory-reporting-lab">
                    <strong>Reporting Lab: </strong>
                    <span>{{ entry.reportingLab }}</span>
                </div>
            </div>
            <div class="my-2">
                <div data-testid="reporting-lab-information-text">
                    <span>Find resources about your lab tests.</span>
                    <hg-button
                        :id="`resources-${index}-${datekey}`"
                        aria-label="Other Resources"
                        href="#"
                        variant="link"
                        data-testid="other-resources-info-button"
                        class="shadow-none align-baseline p-0 ml-1"
                    >
                        <hg-icon icon="info-circle" size="small" />
                    </hg-button>
                    <b-popover
                        :target="`resources-${index}-${datekey}`"
                        triggers="hover focus"
                        placement="topright"
                        boundary="viewport"
                        data-testid="other-resources-info-popover"
                    >
                        <p>
                            Use these websites to learn about specific types of
                            lab tests:
                        </p>
                        <ul class="mb-0">
                            <li>
                                <a
                                    href="https://www.healthlinkbc.ca/tests-treatments-medications/medical-tests"
                                    target="_blank"
                                    rel="noopener"
                                >
                                    HealthLink BC
                                </a>
                            </li>
                            <li>
                                <a
                                    href="https://www.mayocliniclabs.com/"
                                    target="_blank"
                                    rel="noopener"
                                >
                                    Mayo Clinic Laboratories
                                </a>
                            </li>
                            <li>
                                <a
                                    href="https://www.mypathologyreport.ca/"
                                    target="_blank"
                                    rel="noopener"
                                >
                                    For pathology tests (like a biopsy)
                                </a>
                            </li>
                        </ul>
                    </b-popover>
                </div>
            </div>
            <div
                v-if="entry.reportAvailable"
                class="my-3"
                data-testid="laboratory-report-available"
            >
                <hg-button
                    data-testid="laboratory-report-download-btn"
                    variant="secondary"
                    :disabled="isLoadingDocument"
                    @click="showConfirmationModal()"
                >
                    <b-spinner v-if="isLoadingDocument" class="mr-1" small />
                    <hg-icon
                        v-else
                        icon="download"
                        size="medium"
                        square
                        aria-hidden="true"
                        class="mr-1"
                    />
                    <span>Download Full Report</span>
                </hg-button>
            </div>
            <b-table-lite
                :items="entry.tests"
                :fields="['testName', 'result', 'status']"
                sticky-header
                head-variant="light"
                class="my-2"
                data-testid="laboratoryResultTable"
            >
                <template #cell(result)="data">
                    <span>
                        {{ data.value }}
                    </span>
                </template>
                <template #head(result)="data">
                    <span>{{ data.label }}</span>
                    <hg-button
                        :id="`result-info-${index}-${datekey}`"
                        aria-label="Other Resources"
                        href="#"
                        variant="link"
                        data-testid="result-info-button"
                        class="shadow-none align-baseline p-0 ml-1"
                    >
                        <hg-icon icon="info-circle" size="small" />
                    </hg-button>
                    <b-popover
                        :target="`result-info-${index}-${datekey}`"
                        triggers="hover focus"
                        placement="topright"
                        boundary="viewport"
                        data-testid="result-info-popover"
                    >
                        <p>
                            Follow the instructions from your health care
                            provider. When needed, they can explain what your
                            results mean. Remember:
                        </p>
                        <ul class="mb-0">
                            <li>
                                <strong>Ranges</strong> are different between
                                laboratories
                            </li>
                            <li>
                                <strong>“Out of range”</strong> results may be
                                <strong>normal</strong> for you
                            </li>
                        </ul>
                    </b-popover>
                </template>
                <template #cell(status)="data">
                    <span class="mr-1">{{ data.value }}</span>
                    <hg-button
                        v-if="data.item.statusInfo.length > 0"
                        :id="getStatusInfoId(entry.labPdfId, data.index)"
                        aria-label="Status Information"
                        href="#"
                        variant="link"
                        data-testid="laboratory-test-status-info-button"
                        class="shadow-none p-0 mt-n1"
                    >
                        <hg-icon icon="info-circle" size="small" />
                    </hg-button>
                    <b-popover
                        v-if="data.item.statusInfo.length > 0"
                        :target="getStatusInfoId(entry.labPdfId, data.index)"
                        triggers="hover focus"
                        :placement="isMobileDetails ? 'bottom' : 'left'"
                        boundary="viewport"
                        :data-testid="`${getStatusInfoId(
                            entry.labPdfId,
                            data.index
                        )}-popover`"
                    >
                        <p
                            v-for="(paragraph, index) in data.item.statusInfo"
                            :key="index"
                            :class="{
                                'mb-0':
                                    index + 1 === data.item.statusInfo.length,
                            }"
                        >
                            {{ paragraph }}
                        </p>
                    </b-popover>
                </template>
            </b-table-lite>
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

.infoIcon {
    color: $aquaBlue;
}
</style>
