<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ReportHeaderComponent from "@/components/report/header.vue";
import { DateWrapper } from "@/models/dateWrapper";
import Encounter from "@/models/encounter";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import PDFUtil from "@/utility/pdfUtil";

interface EncounterRow {
    date: string;
    specialty_description: string;
    practitioner: string;
    clinic_practitioner: string;
}

@Component({
    components: {
        ReportHeaderComponent,
    },
})
export default class MSPVisitsReportComponent extends Vue {
    @Prop() private filter!: ReportFilter;

    @Action("retrieve", { namespace: "encounter" })
    retrieveEncounters!: (params: { hdid: string }) => Promise<void>;

    @Getter("isLoading", { namespace: "encounter" })
    isLoading!: boolean;

    @Getter("patientEncounters", { namespace: "encounter" })
    patientEncounters!: Encounter[];

    @Getter("user", { namespace: "user" })
    private user!: User;

    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;

    private isPreview = true;

    private readonly headerClass = "encounter-report-table-header";

    private get visibleRecords(): Encounter[] {
        let records = this.patientEncounters.filter((record) => {
            return this.filter.allowsDate(record.encounterDate);
        });
        records.sort((a, b) => {
            const firstDate = new DateWrapper(a.encounterDate);
            const secondDate = new DateWrapper(b.encounterDate);

            return firstDate.isAfter(secondDate)
                ? 1
                : firstDate.isBefore(secondDate)
                ? -1
                : 0;
        });

        return records;
    }

    private get isEmpty() {
        return this.visibleRecords.length == 0;
    }

    private get items(): EncounterRow[] {
        return this.visibleRecords.map<EncounterRow>((x) => {
            return {
                date: DateWrapper.format(x.encounterDate),
                specialty_description: x.specialtyDescription,
                practitioner: x.practitionerName,
                clinic_practitioner: x.clinic.name,
            };
        });
    }

    @Watch("isLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLoading;
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveEncounters({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading encounter data: ${err}`);
        });
    }

    public async generatePdf(): Promise<void> {
        this.logger.debug("generating Health Visits PDF...");
        this.isPreview = false;

        return PDFUtil.generatePdf(
            "HealthGateway_HealthVisits.pdf",
            this.report
        ).then(() => {
            this.isPreview = true;
        });
    }

    private fields: ReportField[] = [
        {
            key: "date",
            thClass: this.headerClass,
        },
        {
            key: "specialty_description",
            thClass: this.headerClass,
        },
        {
            key: "practitioner",
            thClass: this.headerClass,
        },
        {
            key: "clinic_practitioner",
            label: "Clinic/Practitioner",
            thClass: this.headerClass,
        },
    ];
}
</script>

<template>
    <div>
        <div ref="report">
            <section class="pdf-item">
                <ReportHeaderComponent
                    v-show="!isPreview"
                    :filter="filter"
                    title="Health Gateway Health Visit History"
                />
                <b-row v-if="isEmpty && (!isLoading || !isPreview)">
                    <b-col>No records found.</b-col>
                </b-row>

                <b-table
                    v-if="!isEmpty || isLoading"
                    striped
                    :busy="isLoading"
                    :items="items"
                    :fields="fields"
                    class="table-style"
                >
                    <template #table-busy>
                        <content-placeholders>
                            <content-placeholders-text :lines="7" />
                        </content-placeholders>
                    </template>
                </b-table>
            </section>
        </div>
    </div>
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";
.encounter-report-table-header {
    color: $primary;
    font-size: 0.8rem;
}
</style>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.table-style {
    font-size: 0.6rem;
    text-align: center;
}
</style>
