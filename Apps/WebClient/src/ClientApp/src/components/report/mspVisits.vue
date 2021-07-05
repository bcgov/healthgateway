<script lang="ts">
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import Encounter from "@/models/encounter";
import PatientData from "@/models/patientData";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportType, TemplateType } from "@/models/reportRequest";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger, IReportService } from "@/services/interfaces";

interface EncounterRow {
    date: string;
    specialty_description: string;
    practitioner: string;
    clinic_practitioner: string;
}

@Component
export default class MSPVisitsReportComponent extends Vue {
    @Prop() private filter!: ReportFilter;

    @Getter("patientData", { namespace: "user" })
    patientData!: PatientData;

    @Action("retrieve", { namespace: "encounter" })
    retrieveEncounters!: (params: { hdid: string }) => Promise<void>;

    @Getter("isLoading", { namespace: "encounter" })
    isLoading!: boolean;

    @Getter("patientEncounters", { namespace: "encounter" })
    patientEncounters!: Encounter[];

    @Getter("user", { namespace: "user" })
    private user!: User;

    private logger!: ILogger;

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

    private get headerData(): ReportHeader {
        return {
            phn: this.patientData.personalhealthnumber,
            dateOfBirth: this.formatDate(this.patientData.birthdate || ""),
            name: this.patientData
                ? this.patientData.firstname + " " + this.patientData.lastname
                : "",
            datePrinted: this.formatDate(new DateWrapper().toISO()),
            filterText: this.filterText,
        };
    }

    private formatDate(date: string): string {
        return new DateWrapper(date).format();
    }

    private get filterText(): string {
        if (!this.filter.hasDateFilter()) {
            return "";
        }

        const start = this.filter.startDate
            ? ` from ${this.formatDate(this.filter.startDate)}`
            : "";
        const end = this.filter.endDate
            ? this.formatDate(this.filter.endDate)
            : this.formatDate(new DateWrapper().toISO());
        return `Displaying records${start} up to ${end}`;
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
        const reportService: IReportService = container.get<IReportService>(
            SERVICE_IDENTIFIER.ReportService
        );

        return reportService
            .generateReport({
                data: {
                    header: this.headerData,
                    records: this.items,
                },
                template: TemplateType.Encounter,
                type: ReportType.PDF,
            })
            .then((result) => {
                const downloadLink = `data:application/pdf;base64,${result.resourcePayload.data}`;
                fetch(downloadLink).then((res) => {
                    res.blob().then((blob) => {
                        saveAs(blob, result.resourcePayload.fileName);
                    });
                });
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
                <b-row v-if="isEmpty && !isLoading">
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
    color: $heading_color;
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
