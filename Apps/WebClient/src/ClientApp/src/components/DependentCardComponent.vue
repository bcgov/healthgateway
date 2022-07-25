<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faDownload,
    faEllipsisV,
    faInfoCircle,
} from "@fortawesome/free-solid-svg-icons";
import { BTab, BTabs } from "bootstrap-vue";
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Emit, Prop, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import Covid19LaboratoryTestDescriptionComponent from "@/components/laboratory/Covid19LaboratoryTestDescriptionComponent.vue";
import DeleteModalComponent from "@/components/modal/DeleteModalComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import type { Dependent } from "@/models/dependent";
import { ResultError } from "@/models/errors";
import {
    ImmunizationAgent,
    ImmunizationEvent,
    Recommendation,
} from "@/models/immunizationModel";
import { Covid19LaboratoryTest } from "@/models/laboratory";
import Report from "@/models/report";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IDependentService,
    IImmunizationService,
    ILaboratoryService,
    ILogger,
    IReportService,
} from "@/services/interfaces";
import EventTracker from "@/utility/eventTracker";

import LoadingComponent from "./LoadingComponent.vue";

library.add(faEllipsisV, faDownload, faInfoCircle);

interface Covid19LaboratoryTestRow {
    id: string;
    reportAvailable: boolean;
    test: Covid19LaboratoryTest;
}

interface ImmunizationRow {
    date: string;
    immunization: string;
    agent: string;
    product: string;
    provider_clinic: string;
    lotNumber: string;
}

interface RecommendationRow {
    immunization: string;
    due_date: string;
    status: string;
}

@Component({
    components: {
        BTabs,
        BTab,
        LoadingComponent,
        "sensitive-document-download-modal": MessageModalComponent,
        DeleteModalComponent,
        Covid19LaboratoryTestDescriptionComponent,
    },
})
export default class DependentCardComponent extends Vue {
    @Prop() dependent!: Dependent;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;

    @Action("addError", { namespace: "errorBanner" })
    addError!: (params: {
        errorType: ErrorType;
        source: ErrorSourceType;
        traceId: string | undefined;
    }) => void;

    @Ref("sensitiveDocumentDownloadModal")
    readonly sensitiveDocumentDownloadModal!: MessageModalComponent;

    @Ref("deleteModal")
    readonly deleteModal!: DeleteModalComponent;

    @Emit()
    private needsUpdate(): void {
        return;
    }

    private message =
        "Are you sure you want to remove " +
        this.dependent.dependentInformation.firstname +
        " " +
        this.dependent.dependentInformation.lastname +
        " from your list of dependents?";

    private isLoading = false;
    private logger!: ILogger;
    private immunizationService!: IImmunizationService;
    private laboratoryService!: ILaboratoryService;
    private dependentService!: IDependentService;
    private testRows: Covid19LaboratoryTestRow[] = [];
    private immunizations: ImmunizationEvent[] = [];
    private recommendations: Recommendation[] = [];
    private isDataLoaded = false;
    private isGeneratingReport = false;
    private isImmunizationDataLoaded = false;
    private reportFormatType = ReportFormatType.PDF;
    private ReportFormatType: unknown = ReportFormatType;
    private dependentTab = 0;

    private selectedTestRow!: Covid19LaboratoryTestRow;

    private get headerData(): ReportHeader {
        return {
            phn: this.dependent.dependentInformation.PHN,
            dateOfBirth: this.formatDate(
                this.dependent.dependentInformation.dateOfBirth || ""
            ),
            name: this.dependent.dependentInformation
                ? this.dependent.dependentInformation.firstname +
                  " " +
                  this.dependent.dependentInformation.lastname
                : "",
            isRedacted: false,
            datePrinted: new DateWrapper().format(),
            filterText: "",
        };
    }

    private get isDownloadImmunizationReportButtonDisabled(): boolean {
        this.logger.debug(
            `isGeneratingReport: ${this.isGeneratingReport} immunizationItems:  ${this.immunizationItems.length} and recommendationItems: ${this.recommendationItems.length}`
        );
        return (
            this.isGeneratingReport ||
            this.dependentTab !== 2 ||
            (this.immunizationItems.length == 0 &&
                this.recommendationItems.length == 0)
        );
    }

    private get isExpired(): boolean {
        let birthDate = new DateWrapper(
            this.dependent.dependentInformation.dateOfBirth
        );
        let now = new DateWrapper();
        return (
            now.diff(birthDate, "year").years >
            this.webClientConfig.maxDependentAge
        );
    }

    private get isImmunizationTabShown(): boolean {
        return this.webClientConfig.modules["DependentImmunizationTab"];
    }

    private get immunizationItems(): ImmunizationRow[] {
        return this.immunizations.map<ImmunizationRow>((x) => ({
            date: DateWrapper.format(x.dateOfImmunization),
            immunization: x.immunization.name,
            agent: this.getAgentNames(x.immunization.immunizationAgents),
            product: this.getProductNames(x.immunization.immunizationAgents),
            provider_clinic: x.providerOrClinic,
            lotNumber: this.getAgentLotNumbers(
                x.immunization.immunizationAgents
            ),
        }));
    }

    private get recommendationItems(): RecommendationRow[] {
        return this.recommendations.map<RecommendationRow>((x) => ({
            immunization: x.targetDiseases.find((y) => y.name)?.name ?? "",
            due_date:
                x.diseaseDueDate === undefined || x.diseaseDueDate === null
                    ? ""
                    : DateWrapper.format(x.diseaseDueDate),
            status: x.status || "",
        }));
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.immunizationService = container.get<IImmunizationService>(
            SERVICE_IDENTIFIER.ImmunizationService
        );
        this.laboratoryService = container.get<ILaboratoryService>(
            SERVICE_IDENTIFIER.LaboratoryService
        );
        this.dependentService = container.get<IDependentService>(
            SERVICE_IDENTIFIER.DependentService
        );
    }

    private deleteDependent(): void {
        this.isLoading = true;
        this.dependentService
            .removeDependent(this.user.hdid, this.dependent)
            .then(() => this.needsUpdate())
            .catch((err: ResultError) =>
                this.addError({
                    errorType: ErrorType.Delete,
                    source: ErrorSourceType.Dependent,
                    traceId: err.traceId,
                })
            )
            .finally(() => {
                this.isLoading = false;
            });
    }

    private downloadCovid19Report(): void {
        this.isGeneratingReport = true;
        let test = this.selectedTestRow.test;
        this.laboratoryService
            .getReportDocument(
                this.selectedTestRow.id,
                this.dependent.ownerId,
                true
            )
            .then((result) => {
                const report = result.resourcePayload;
                fetch(
                    `data:${report.mediaType};${report.encoding},${report.data}`
                )
                    .then((response) => response.blob())
                    .then((blob) =>
                        saveAs(
                            blob,
                            `COVID_Result_${this.dependent.dependentInformation.firstname}${this.dependent.dependentInformation.lastname}_${test.collectedDateTime}.pdf`
                        )
                    );
            })
            .catch((err: ResultError) => {
                this.logger.error(err.resultMessage);
                this.addError({
                    errorType: ErrorType.Download,
                    source: ErrorSourceType.Covid19LaboratoryReport,
                    traceId: err.traceId,
                });
            })
            .finally(() => {
                this.isGeneratingReport = false;
            });
    }

    private downloadImmunizationReport(): void {
        this.isGeneratingReport = true;

        this.trackDownload();

        this.generateReport(
            TemplateType.DependentImmunization,
            this.reportFormatType,
            this.headerData
        )
            .then((result: RequestResult<Report>) => {
                const mimeType = this.getMimeType(this.reportFormatType);
                const downloadLink = `data:${mimeType};base64,${result.resourcePayload.data}`;
                fetch(downloadLink).then((res) =>
                    res
                        .blob()
                        .then((blob) =>
                            saveAs(blob, result.resourcePayload.fileName)
                        )
                );
            })
            .finally(() => {
                this.isGeneratingReport = false;
            });
    }

    private downloadReport(): void {
        this.logger.debug(
            `Download report for dependent tab: ${this.dependentTab}`
        );

        if (this.dependentTab === 1) {
            this.downloadCovid19Report();
        } else if (this.dependentTab === 2) {
            this.downloadImmunizationReport();
        }
    }

    private formatDate(date: StringISODate): string {
        return new DateWrapper(date).format();
    }

    private fetchCovid19LaboratoryTests(): void {
        this.logger.debug(
            `Fetching COVID 19 Laboratory Tests - data loaded: ${this.isDataLoaded}`
        );
        if (this.isDataLoaded) {
            return;
        }
        this.logger.debug(
            `Fetching COVID 19 Laboratory Tests for Hdid: ${this.dependent.ownerId}`
        );
        this.isLoading = true;
        this.laboratoryService
            .getCovid19LaboratoryOrders(this.dependent.ownerId)
            .then((result) => {
                const payload = result.resourcePayload;
                if (result.resultStatus == ResultType.Success) {
                    this.testRows =
                        payload.orders.flatMap<Covid19LaboratoryTestRow>((o) =>
                            o.labResults.map<Covid19LaboratoryTestRow>((r) => ({
                                id: o.id,
                                reportAvailable: o.reportAvailable,
                                test: r,
                            }))
                        );
                    this.sortEntries();
                    this.isDataLoaded = true;
                    this.isLoading = false;
                } else if (
                    result.resultError?.actionCode === ActionType.Refresh &&
                    !payload.loaded &&
                    payload.retryin > 0
                ) {
                    this.logger.info(
                        "Re-querying for COVID-19 Laboratory Orders"
                    );
                    setTimeout(
                        () => this.fetchCovid19LaboratoryTests(),
                        payload.retryin
                    );
                } else {
                    this.logger.error(
                        "Error returned from the COVID-19 Laboratory Orders call: " +
                            JSON.stringify(result.resultError)
                    );
                    this.addError({
                        errorType: ErrorType.Retrieve,
                        source: ErrorSourceType.Covid19Laboratory,
                        traceId: result.resultError?.traceId,
                    });
                    this.isLoading = false;
                }
            })
            .catch((err: ResultError) => {
                this.logger.error(err.resultMessage);
                this.addError({
                    errorType: ErrorType.Retrieve,
                    source: ErrorSourceType.Covid19Laboratory,
                    traceId: err.traceId,
                });
                this.isLoading = false;
            });
    }

    private fetchPatientImmunizations(): void {
        const hdid = this.dependent.ownerId;
        this.logger.debug(`Fetching Patient Immunizations for Hdid: ${hdid}`);
        this.logger.debug(`Logged in user Hdid: ${this.user.hdid}`);
        this.logger.debug(
            `Fetching Patient Immunizations - immunization data laoded: ${this.isImmunizationDataLoaded}`
        );

        if (this.isImmunizationDataLoaded) {
            return;
        }

        this.isLoading = true;
        this.immunizationService
            .getPatientImmunizations(hdid)
            .then((result) => {
                if (result.resultStatus == ResultType.Success) {
                    const payload = result.resourcePayload;
                    if (payload.loadState.refreshInProgress) {
                        this.logger.info("Re-querying Patient Immunizations");
                        setTimeout(
                            () => this.fetchPatientImmunizations(),
                            10000
                        );
                    } else {
                        this.setImmunizations(payload.immunizations);
                        this.setRecommendations(payload.recommendations);
                        this.isImmunizationDataLoaded = true;
                        this.isLoading = false;
                        this.logger.debug(
                            `Patient Immunizations:
                                ${JSON.stringify(this.immunizations)}`
                        );
                        this.logger.debug(
                            `Patient Recommendations:
                                ${JSON.stringify(this.recommendations)}`
                        );
                    }
                } else {
                    this.logger.error(
                        `Error returned from the Patient Immunizations call:
                            ${JSON.stringify(result.resultError)}`
                    );
                    this.addError({
                        errorType: ErrorType.Retrieve,
                        source: ErrorSourceType.Immunization,
                        traceId: result.resultError?.traceId,
                    });
                    this.isLoading = false;
                }
            })
            .catch((err: ResultError) => {
                this.logger.error(err.resultMessage);
                this.addError({
                    errorType: ErrorType.Retrieve,
                    source: ErrorSourceType.Immunization,
                    traceId: err.traceId,
                });
                this.isLoading = false;
            });
    }

    private generateReport(
        templateType: TemplateType,
        reportFormatType: ReportFormatType,
        headerData: ReportHeader
    ): Promise<RequestResult<Report>> {
        const reportService = container.get<IReportService>(
            SERVICE_IDENTIFIER.ReportService
        );

        return reportService.generateReport({
            data: {
                header: headerData,
                records: this.immunizationItems,
                recommendations: this.recommendationItems,
            },
            template: templateType,
            type: reportFormatType,
        });
    }

    private getAgentLotNumbers(
        immunizationAgents: ImmunizationAgent[]
    ): string {
        const lotNumbers = immunizationAgents.map<string>((x) => x.lotNumber);
        return lotNumbers.join(", ");
    }

    private getAgentNames(immunizationAgents: ImmunizationAgent[]): string {
        const agents = immunizationAgents.map<string>((x) => x.name);
        return agents.join(", ");
    }

    private getMimeType(reportFormatType: ReportFormatType): string {
        switch (reportFormatType) {
            case ReportFormatType.PDF:
                return "application/pdf";
            case ReportFormatType.CSV:
                return "text/csv";
            case ReportFormatType.XLSX:
                return "application/vnd.openxmlformats";
            default:
                return "";
        }
    }

    private getOutcomeClasses(outcome: string): string[] {
        switch (outcome?.toUpperCase()) {
            case "NEGATIVE":
                return ["text-success"];
            case "POSITIVE":
                return ["text-danger"];
            default:
                return ["text-muted"];
        }
    }

    private getProductNames(immunizationAgents: ImmunizationAgent[]): string {
        const productNames = immunizationAgents.map<string>(
            (x) => x.productName
        );
        return productNames.join(", ");
    }

    private setImmunizations(immunizations: ImmunizationEvent[]): void {
        this.immunizations = immunizations;

        this.immunizations.sort((a, b) => {
            const firstDate = new DateWrapper(a.dateOfImmunization);
            const secondDate = new DateWrapper(b.dateOfImmunization);

            if (firstDate.isBefore(secondDate)) {
                return 1;
            }

            if (firstDate.isAfter(secondDate)) {
                return -1;
            }

            return 0;
        });
    }

    private setRecommendations(recommendations: Recommendation[]): void {
        this.recommendations = recommendations.filter((x) =>
            x.targetDiseases.some((y) => y.name)
        );

        this.recommendations.sort((a, b) => {
            const firstDateEmpty =
                a.diseaseDueDate === null || a.diseaseDueDate === undefined;
            const secondDateEmpty =
                b.diseaseDueDate === null || b.diseaseDueDate === undefined;

            if (firstDateEmpty && secondDateEmpty) {
                return 0;
            }

            if (firstDateEmpty) {
                return 1;
            }

            if (secondDateEmpty) {
                return -1;
            }

            const firstDate = new DateWrapper(a.diseaseDueDate);
            const secondDate = new DateWrapper(b.diseaseDueDate);

            if (firstDate.isBefore(secondDate)) {
                return -1;
            }

            if (firstDate.isAfter(secondDate)) {
                return 1;
            }

            return 0;
        });
    }

    private sortEntries(): void {
        this.testRows.sort((a, b) => {
            let dateA = new DateWrapper(a.test.collectedDateTime);
            let dateB = new DateWrapper(b.test.collectedDateTime);
            if (dateA.isBefore(dateB)) {
                return 1;
            }
            if (dateA.isAfter(dateB)) {
                return -1;
            }
            return 0;
        });
    }

    private showCovid19DownloadConfirmationModal(
        row: Covid19LaboratoryTestRow
    ): void {
        this.selectedTestRow = row;
        this.sensitiveDocumentDownloadModal.showModal();
    }

    private showImmunizationDownloadConfirmationModal(
        reportFormatType: ReportFormatType
    ): void {
        this.logger.debug(
            `Show sensitive immunization document download modal: ${reportFormatType}`
        );
        this.reportFormatType = reportFormatType;
        this.sensitiveDocumentDownloadModal.showModal();
    }

    private showDeleteConfirmationModal(): void {
        this.deleteModal.showModal();
    }

    private trackDownload(): void {
        const reportName = "Dependent Immunization";
        const formatTypeName = ReportFormatType[this.reportFormatType];
        const eventName = `${reportName} (${formatTypeName})`;
        this.logger.debug(`Track download for: ${eventName}`);

        EventTracker.downloadReport(eventName);
    }
}
</script>

<template>
    <div>
        <b-card
            no-body
            :data-testid="`dependent-card-${dependent.dependentInformation.PHN}`"
        >
            <b-tabs v-model="dependentTab" card>
                <b-tab active data-testid="dependentTab">
                    <template #title>
                        <div>Profile</div>
                    </template>
                    <div v-if="isExpired">
                        <b-row>
                            <b-col class="d-flex justify-content-center">
                                <h5>Your access has expired</h5>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col class="d-flex justify-content-center">
                                <p>
                                    You no longer have access to this dependent
                                    as they have turned
                                    {{ webClientConfig.maxDependentAge }}
                                </p>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col class="d-flex justify-content-center">
                                <hg-button
                                    variant="secondary"
                                    @click="deleteDependent()"
                                >
                                    Remove Dependent
                                </hg-button>
                            </b-col>
                        </b-row>
                    </div>
                    <div v-else>
                        <b-row>
                            <b-col class="col-12 col-sm-4">
                                <b-row>
                                    <b-col class="col-12">PHN</b-col>
                                    <b-col class="col-12">
                                        <b-form-input
                                            v-model="
                                                dependent.dependentInformation
                                                    .PHN
                                            "
                                            data-testid="dependentPHN"
                                            readonly
                                        />
                                    </b-col>
                                </b-row>
                            </b-col>
                            <b-col class="col-12 col-sm-4">
                                <b-row>
                                    <b-col class="col-12">Date of Birth</b-col>
                                    <b-col class="col-12">
                                        <b-form-input
                                            :value="
                                                formatDate(
                                                    dependent
                                                        .dependentInformation
                                                        .dateOfBirth
                                                )
                                            "
                                            data-testid="dependentDOB"
                                            readonly
                                        />
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                    </div>
                </b-tab>
                <b-tab
                    :disabled="isExpired"
                    data-testid="covid19Tab"
                    class="tableTab mt-2"
                    @click="fetchCovid19LaboratoryTests()"
                >
                    <template #title>
                        <div data-testid="covid19TabTitle">COVID-19</div>
                    </template>
                    <b-row v-if="isLoading" class="m-2">
                        <b-col>
                            <b-spinner />
                        </b-col>
                    </b-row>
                    <b-row v-else-if="testRows.length == 0" class="m-2">
                        <b-col data-testid="covid19NoRecords">
                            No records found.
                        </b-col>
                    </b-row>
                    <table
                        v-else
                        class="w-100"
                        aria-describedby="COVID-19 Test Results"
                    >
                        <tr>
                            <th scope="col">COVID-19 Test Date</th>
                            <th scope="col" class="d-none d-sm-table-cell">
                                Test Type
                            </th>
                            <th scope="col" class="d-none d-sm-table-cell">
                                Status
                            </th>
                            <th scope="col">Result</th>
                            <th scope="col">Report</th>
                        </tr>
                        <tr v-for="(row, index) in testRows" :key="index">
                            <td data-testid="dependentCovidTestDate">
                                {{ formatDate(row.test.collectedDateTime) }}
                            </td>
                            <td
                                data-testid="dependentCovidTestType"
                                class="d-none d-sm-table-cell"
                            >
                                {{ row.test.testType }}
                            </td>
                            <td
                                data-testid="dependentCovidTestStatus"
                                class="d-none d-sm-table-cell"
                            >
                                {{ row.test.testStatus }}
                            </td>
                            <td data-testid="dependentCovidTestLabResult">
                                <span
                                    v-if="row.test.filteredLabResultOutcome"
                                    class="font-weight-bold"
                                    :class="
                                        getOutcomeClasses(
                                            row.test.labResultOutcome
                                        )
                                    "
                                >
                                    {{ row.test.filteredLabResultOutcome }}
                                </span>
                                <span v-if="row.test.resultReady">
                                    <hg-button
                                        :id="
                                            'dependent-covid-test-info-button-' +
                                            index
                                        "
                                        aria-label="Result Description"
                                        href="#"
                                        variant="link"
                                        data-testid="dependent-covid-test-info-button"
                                        class="shadow-none p-0 ml-1"
                                    >
                                        <hg-icon
                                            icon="info-circle"
                                            size="small"
                                        />
                                    </hg-button>
                                    <b-popover
                                        :target="
                                            'dependent-covid-test-info-button-' +
                                            index
                                        "
                                        triggers="hover focus"
                                        placement="bottomleft"
                                        data-testid="dependent-covid-test-info-popover"
                                    >
                                        <Covid19LaboratoryTestDescriptionComponent
                                            class="p-2"
                                            :description="
                                                row.test.resultDescription
                                            "
                                            :link="row.test.resultLink"
                                        />
                                    </b-popover>
                                </span>
                            </td>
                            <td>
                                <hg-button
                                    v-if="
                                        row.reportAvailable &&
                                        row.test.resultReady
                                    "
                                    data-testid="dependentCovidReportDownloadBtn"
                                    variant="link"
                                    class="p-1"
                                    @click="
                                        showCovid19DownloadConfirmationModal(
                                            row
                                        )
                                    "
                                >
                                    <hg-icon
                                        icon="download"
                                        size="medium"
                                        square
                                        aria-hidden="true"
                                    />
                                </hg-button>
                            </td>
                        </tr>
                    </table>
                </b-tab>
                <b-tab
                    v-if="isImmunizationTabShown"
                    :disabled="isExpired"
                    :data-testid="`immunization-tab-${dependent.ownerId}`"
                    class="tableTab mt-2"
                    @click="fetchPatientImmunizations()"
                >
                    <template #title>
                        <div
                            :data-testid="`immunization-tab-title-${dependent.ownerId}`"
                        >
                            Immunization
                        </div>
                    </template>
                    <b-row v-if="isLoading" class="m-2">
                        <b-col>
                            <b-spinner />
                        </b-col>
                    </b-row>
                    <div
                        v-else
                        :data-testid="`immunization-tab-div-${dependent.ownerId}`"
                    >
                        <b-card no-body>
                            <b-tabs class="p-2">
                                <b-tab title="History" active>
                                    <b-dropdown
                                        v-if="immunizationItems.length != 0"
                                        id="download-immunization-history-report-btn"
                                        text="Download"
                                        class="p-4 float-right"
                                        variant="outline-dark"
                                        :data-testid="`download-immunization-history-report-btn-${dependent.ownerId}`"
                                        :disabled="
                                            isDownloadImmunizationReportButtonDisabled
                                        "
                                    >
                                        <b-dropdown-item
                                            :data-testid="`download-immunization-history-report-pdf-btn-${dependent.ownerId}`"
                                            @click="
                                                showImmunizationDownloadConfirmationModal(
                                                    ReportFormatType.PDF
                                                )
                                            "
                                            >PDF</b-dropdown-item
                                        >
                                        <b-dropdown-item
                                            :data-testid="`download-immunization-history-report-csv-btn-${dependent.ownerId}`"
                                            @click="
                                                showImmunizationDownloadConfirmationModal(
                                                    ReportFormatType.CSV
                                                )
                                            "
                                            >CSV</b-dropdown-item
                                        >
                                        <b-dropdown-item
                                            :data-testid="`download-immunization-history-report-xlsx-btn-${dependent.ownerId}`"
                                            @click="
                                                showImmunizationDownloadConfirmationModal(
                                                    ReportFormatType.XLSX
                                                )
                                            "
                                            >XLSX</b-dropdown-item
                                        >
                                    </b-dropdown>
                                    <b-row
                                        v-if="immunizationItems.length == 0"
                                        class="m-2"
                                    >
                                        <b-col
                                            :data-testid="`immunization-history-no-rows-found-${dependent.ownerId}`"
                                        >
                                            No records found.
                                        </b-col>
                                    </b-row>
                                    <table
                                        v-else
                                        class="w-100"
                                        aria-describedby="Immunization History"
                                        :data-testid="`immunization-history-table-${dependent.ownerId}`"
                                    >
                                        <tr>
                                            <th scope="col">Date</th>
                                            <th scope="col">Immunization</th>
                                            <th
                                                scope="col"
                                                class="d-none d-sm-table-cell"
                                            >
                                                Agent
                                            </th>
                                            <th
                                                scope="col"
                                                class="d-none d-sm-table-cell"
                                            >
                                                Product
                                            </th>
                                            <th scope="col">Provider/Clinic</th>
                                            <th
                                                scope="col"
                                                class="d-none d-sm-table-cell"
                                            >
                                                Lot Number
                                            </th>
                                        </tr>
                                        <tr
                                            v-for="(
                                                row, index
                                            ) in immunizationItems"
                                            :key="index"
                                        >
                                            <td
                                                :data-testid="`history-immunization-date-${dependent.ownerId}-${index}`"
                                            >
                                                {{ row.date }}
                                            </td>
                                            <td
                                                :data-testid="`history-product-${dependent.ownerId}-${index}`"
                                            >
                                                {{ row.immunization }}
                                            </td>
                                            <td
                                                :data-testid="`history-immunizing-agent-${dependent.ownerId}-${index}`"
                                                class="d-none d-sm-table-cell"
                                            >
                                                {{ row.agent }}
                                            </td>
                                            <td
                                                :data-testid="`history-immunizing-product-${dependent.ownerId}-${index}`"
                                                class="d-none d-sm-table-cell"
                                            >
                                                {{ row.product }}
                                            </td>
                                            <td
                                                :data-testid="`history-provider-clinic-${dependent.ownerId}-${index}`"
                                            >
                                                {{ row.provider_clinic }}
                                            </td>
                                            <td
                                                :data-testid="`history-lot-number-${dependent.ownerId}-${index}`"
                                                class="d-none d-sm-table-cell"
                                            >
                                                {{ row.lotNumber }}
                                            </td>
                                        </tr>
                                    </table>
                                </b-tab>
                                <b-tab title="Forecasts">
                                    <b-dropdown
                                        v-if="recommendationItems.length != 0"
                                        id="download-immunization-forecast-report-btn"
                                        text="Download"
                                        class="p-4 float-right"
                                        variant="outline-dark"
                                        :data-testid="`download-immunization-forecast-report-btn-${dependent.ownerId}`"
                                        :disabled="
                                            isDownloadImmunizationReportButtonDisabled
                                        "
                                    >
                                        <b-dropdown-item
                                            :data-testid="`download-immunization-forecast-report-pdf-btn-${dependent.ownerId}`"
                                            @click="
                                                showImmunizationDownloadConfirmationModal(
                                                    ReportFormatType.PDF
                                                )
                                            "
                                            >PDF</b-dropdown-item
                                        >
                                        <b-dropdown-item
                                            :data-testid="`download-immunization-forecast-report-csv-btn-${dependent.ownerId}`"
                                            @click="
                                                showImmunizationDownloadConfirmationModal(
                                                    ReportFormatType.CSV
                                                )
                                            "
                                            >CSV</b-dropdown-item
                                        >
                                        <b-dropdown-item
                                            :data-testid="`download-immunization-forecast-report-xlsx-btn-${dependent.ownerId}`"
                                            @click="
                                                showImmunizationDownloadConfirmationModal(
                                                    ReportFormatType.XLSX
                                                )
                                            "
                                            >XLSX</b-dropdown-item
                                        >
                                    </b-dropdown>
                                    <b-row
                                        v-if="recommendationItems.length == 0"
                                        class="m-2"
                                    >
                                        <b-col
                                            :data-testid="`immunization-forecast-no-rows-found-${dependent.ownerId}`"
                                        >
                                            No records found.
                                        </b-col>
                                    </b-row>
                                    <table
                                        v-else
                                        class="w-100"
                                        aria-describedby="Immunization Forecast"
                                        :data-testid="`immunization-forecast-table-${dependent.ownerId}`"
                                    >
                                        <tr>
                                            <th scope="col">Immunization</th>
                                            <th scope="col">Due Date</th>
                                            <th scope="col">Status</th>
                                        </tr>
                                        <tr
                                            v-for="(
                                                row, index
                                            ) in recommendationItems"
                                            :key="index"
                                        >
                                            <td
                                                :data-testid="`forecast-immunization-${dependent.ownerId}-${index}`"
                                            >
                                                {{ row.immunization }}
                                            </td>
                                            <td
                                                :data-testid="`forecast-due-date-${dependent.ownerId}-${index}`"
                                            >
                                                {{ row.due_date }}
                                            </td>
                                            <td
                                                :data-testid="`forecast-status-${dependent.ownerId}-${index}`"
                                            >
                                                {{ row.status }}
                                            </td>
                                        </tr>
                                    </table>
                                </b-tab>
                            </b-tabs>
                        </b-card>
                    </div>
                </b-tab>
                <template #tabs-start>
                    <div class="w-100">
                        <b-row>
                            <b-col>
                                <span
                                    class="card-title"
                                    data-testid="dependentName"
                                >
                                    {{
                                        dependent.dependentInformation.firstname
                                    }}
                                    {{
                                        dependent.dependentInformation.lastname
                                    }}
                                </span>
                            </b-col>
                            <ul class="list-unstyled">
                                <li
                                    role="presentation"
                                    class="ml-auto mr-1 nav-item align-self-center"
                                >
                                    <b-nav-item-dropdown
                                        right
                                        text=""
                                        :no-caret="true"
                                    >
                                        <template slot="button-content">
                                            <hg-icon
                                                icon="ellipsis-v"
                                                size="medium"
                                                data-testid="dependentMenuBtn"
                                                class="dependentMenu"
                                            />
                                        </template>
                                        <b-dropdown-item
                                            data-testid="deleteDependentMenuBtn"
                                            class="menuItem"
                                            @click="
                                                showDeleteConfirmationModal()
                                            "
                                        >
                                            Delete
                                        </b-dropdown-item>
                                    </b-nav-item-dropdown>
                                </li>
                            </ul>
                        </b-row>
                    </div>
                </template>
            </b-tabs>
        </b-card>
        <LoadingComponent
            :is-loading="isGeneratingReport"
            :full-screen="false"
        ></LoadingComponent>
        <delete-modal-component
            ref="deleteModal"
            title="Remove Dependent"
            message="Are you sure you want to remove this dependent?"
            @submit="deleteDependent()"
        />
        <sensitive-document-download-modal
            ref="sensitiveDocumentDownloadModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="downloadReport()"
        />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

tr:nth-child(even) {
    background: $soft_background;
}

th {
    font-weight: bold;
}

td,
th {
    text-align: center;
}

.tableTab {
    padding: 0;
}

.dependentMenu {
    color: $soft_text;
}

.card-title {
    padding-left: 14px;
    font-size: 1.2em;
}
</style>
