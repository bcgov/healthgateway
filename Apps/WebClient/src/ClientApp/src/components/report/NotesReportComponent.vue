<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import UserNote from "@/models/userNote";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IReportService } from "@/services/interfaces";

interface UserNoteRow {
    date: string;
    title: string;
    note: string;
}

@Component
export default class NotesReportComponent extends Vue {
    @Prop({ required: true })
    hdid!: string;

    @Prop() private filter!: ReportFilter;

    @Action("retrieveNotes", { namespace: "note" })
    retrieveNotes!: (params: { hdid: string }) => Promise<void>;

    @Getter("notesAreLoading", { namespace: "note" })
    notesAreLoading!: boolean;

    @Getter("notes", { namespace: "note" })
    notes!: UserNote[];

    private logger!: ILogger;

    private readonly headerClass = "note-report-table-header";

    private get isLoading(): boolean {
        return this.notesAreLoading;
    }

    private get visibleRecords(): UserNote[] {
        let records = this.notes.filter((record) =>
            this.filter.allowsDate(record.journalDate)
        );
        records.sort((a, b) => {
            const firstDate = new DateWrapper(a.journalDate);
            const secondDate = new DateWrapper(b.journalDate);

            if (firstDate.isBefore(secondDate)) {
                return 1;
            }

            if (firstDate.isAfter(secondDate)) {
                return -1;
            }

            return 0;
        });

        return records;
    }

    private get isEmpty(): boolean {
        return this.visibleRecords.length === 0;
    }

    private get items(): UserNoteRow[] {
        return this.visibleRecords.map<UserNoteRow>((x) => ({
            date: DateWrapper.format(x.journalDate),
            title: x.title,
            note: x.text,
        }));
    }

    @Watch("isLoading")
    @Emit()
    private onIsLoadingChanged(): boolean {
        return this.isLoading;
    }

    @Watch("isEmpty")
    @Emit()
    private onIsEmptyChanged(): boolean {
        return this.isEmpty;
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveNotes({ hdid: this.hdid }).catch((err) =>
            this.logger.error(`Error loading user note data: ${err}`)
        );
    }

    private mounted(): void {
        this.onIsEmptyChanged();
    }

    public async generateReport(
        reportFormatType: ReportFormatType,
        headerData: ReportHeader
    ): Promise<RequestResult<Report>> {
        const reportService = container.get<IReportService>(
            SERVICE_IDENTIFIER.ReportService
        );

        return reportService.generateReport({
            data: {
                header: headerData,
                records: this.items,
            },
            template: TemplateType.Notes,
            type: reportFormatType,
        });
    }

    private fields: ReportField[] = [
        {
            key: "date",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "user-note-date" },
            thStyle: { width: "10%" },
        },
        {
            key: "title",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "user-note-title" },
            thStyle: { width: "30%" },
        },
        {
            key: "note",
            thClass: this.headerClass,
            thStyle: { width: "60%" },
            tdClass: "text-left",
        },
    ];
}
</script>

<template>
    <div>
        <div>
            <section>
                <b-row v-if="isEmpty && !isLoading">
                    <b-col>No records found.</b-col>
                </b-row>

                <b-table
                    v-if="!isEmpty || isLoading"
                    :striped="true"
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

.note-report-table-header {
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
