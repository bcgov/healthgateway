<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faDownload } from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import LaboratoryOrderTimelineEntry from "@/models/laboratoryOrderTimelineEntry";
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
    },
})
export default class LaboratoryOrderTimelineComponent extends Vue {
    @Prop() entry!: LaboratoryOrderTimelineEntry;
    @Prop() index!: number;
    @Prop() datekey!: string;
    @Prop() isMobileDetails!: boolean;
    @Getter("user", { namespace: "user" }) user!: User;

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

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

    private getResultClasses(result: string): string[] {
        switch (result?.toUpperCase()) {
            case "OUT OF RANGE":
                return ["text-danger"];
            case "IN RANGE":
                return ["text-success"];
            default:
                return [];
        }
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
            .catch((err) => this.logger.error(err))
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
                    <span
                        ><router-link to="/faq">Find resources</router-link> to
                        learn about your lab test and what the results
                        mean.</span
                    >
                </div>
            </div>
            <b-row class="my-3">
                <b-col />
                <b-col
                    v-if="entry.reportAvailable"
                    data-testid="laboratory-report-available"
                    cols="auto"
                >
                    <hg-button
                        data-testid="laboratory-report-download-btn"
                        variant="secondary"
                        :disabled="isLoadingDocument"
                        @click="showConfirmationModal()"
                    >
                        <b-spinner
                            v-if="isLoadingDocument"
                            class="mr-1"
                            small
                        />
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
                </b-col>
            </b-row>
            <b-table-lite
                :items="entry.tests"
                :fields="['testName', 'result', 'status']"
                sticky-header
                head-variant="light"
                class="my-2"
                data-testid="laboratoryResultTable"
            >
                <template #cell(result)="data">
                    <strong :class="getResultClasses(data.value)">
                        {{ data.value }}
                    </strong>
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
