<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faDownload, faMicroscope } from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/genericMessage.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryReport } from "@/models/laboratory";
import LaboratoryOrderTimelineEntry from "@/models/laboratoryOrderTimelineEntry";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILaboratoryService, ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

import EntrycardTimelineComponent from "./entrycard.vue";

library.add(faDownload, faMicroscope);

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

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.laboratoryService = container.get<ILaboratoryService>(
            SERVICE_IDENTIFIER.LaboratoryService
        );
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

    private showConfirmationModal(): void {
        this.messageModal.showModal();
    }

    private getReport() {
        SnowPlow.trackEvent({
            action: "download_report",
            text: "Laboratory Report PDF",
        });

        this.isLoadingDocument = true;
        this.laboratoryService
            .getReportDocument(this.entry.id, this.user.hdid, false)
            .then((result) => {
                let dateString =
                    this.entry.timelineDateTime.format("YYYY_MM_DD-HH_mm");
                let report: LaboratoryReport = result.resourcePayload;
                fetch(
                    `data:${report.mediaType};${report.encoding},${report.data}`
                )
                    .then((response) => response.blob())
                    .then((blob) => {
                        saveAs(blob, `Laboratory_Report_${dateString}.pdf`);
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
        entry-icon="microscope"
        :title="entry.commonName"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :has-attachment="entry.reportAvailable"
    >
        <div slot="header-description">
            <span>Number of Tests: </span>
            <strong data-testid="laboratoryHeaderResultCount">
                {{ entry.tests.length }}
            </strong>
        </div>
        <div slot="details-body">
            <div
                v-if="entry.reportAvailable"
                data-testid="laboratoryReportAvailable"
                class="mt-2 mb-n1"
            >
                <b-spinner v-if="isLoadingDocument" class="mb-1" />
                <span v-else data-testid="laboratoryReport">
                    <strong class="align-bottom d-inline-block pb-1">
                        Detailed Report:
                    </strong>
                    <hg-button
                        data-testid="laboratory-report-download-btn"
                        variant="link"
                        class="p-1 ml-1"
                        @click="showConfirmationModal()"
                    >
                        <hg-icon
                            icon="download"
                            size="medium"
                            square
                            aria-hidden="true"
                            class="mr-1"
                        />
                        <span>{{ entry.downloadLabel }}</span>
                    </hg-button>
                </span>
            </div>
            <div class="my-2">
                <div data-testid="laboratoryCollectionDate">
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
                <div data-testid="laboratoryOrderingProvider">
                    <strong>Ordering Provider: </strong>
                    <span>{{ entry.orderingProvider }}</span>
                </div>
            </div>
            <div class="my-2">
                <div data-testid="laboratoryReportingLab">
                    <strong>Reporting Lab: </strong>
                    <span>{{ entry.reportingLab }}</span>
                </div>
            </div>
            <b-table-lite
                :items="entry.tests"
                sticky-header
                head-variant="light"
                class="mt-4 mb-2"
                data-testid="laboratoryResultTable"
            >
                <template #cell(result)="data">
                    <strong :class="getResultClasses(data.value)">
                        {{ data.value }}
                    </strong>
                </template>
                <template #head(result)="data">
                    <span>{{ data.label }}</span>
                    <span
                        :id="`popover-info${index}-${datekey}`"
                        class="infoIcon ml-2"
                        tabindex="0"
                    >
                        <hg-icon icon="info-circle" size="medium" />
                    </span>
                    <b-popover
                        :target="`popover-info${index}-${datekey}`"
                        placement="top"
                        triggers="hover focus"
                        custom-class="p-2"
                    >
                        Laboratory tests provide a partial picture of your
                        health. To interpret these results, clinicians must
                        combine these tests results with your other health
                        information. Visit the
                        <router-link to="/faq">FAQ</router-link> page to learn
                        more.
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
