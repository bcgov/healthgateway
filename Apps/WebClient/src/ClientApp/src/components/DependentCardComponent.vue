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
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import Covid19LaboratoryTestDescriptionComponent from "@/components/laboratory/Covid19LaboratoryTestDescriptionComponent.vue";
import DeleteModalComponent from "@/components/modal/DeleteModalComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import type { WebClientConfiguration } from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
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
import { LoadStatus } from "@/models/storeOperations";
import User from "@/models/user";
import VaccinationRecord from "@/models/vaccinationRecord";
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
import SnowPlow from "@/utility/snowPlow";

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

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        BTabs,
        BTab,
        LoadingComponent,
        MessageModalComponent,
        DeleteModalComponent,
        Covid19LaboratoryTestDescriptionComponent,
    },
};

@Component(options)
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

    @Action("retrieveAuthenticatedVaccineRecord", {
        namespace: "vaccinationStatus",
    })
    retrieveAuthenticatedVaccineRecord!: (params: {
        hdid: string;
    }) => Promise<CovidVaccineRecord>;

    @Action("stopAuthenticatedVaccineRecordDownload", {
        namespace: "vaccinationStatus",
    })
    stopAuthenticatedVaccineRecordDownload!: (params: { hdid: string }) => void;

    @Getter("authenticatedVaccineRecordStatusChanges", {
        namespace: "vaccinationStatus",
    })
    vaccineRecordStatusChanges!: number;

    @Getter("authenticatedVaccineRecordActiveHdid", {
        namespace: "vaccinationStatus",
    })
    vaccineRecordActiveHdid!: string;

    @Getter("authenticatedVaccineRecords", { namespace: "vaccinationStatus" })
    vaccineRecords!: Map<string, VaccinationRecord>;

    @Action("setTooManyRequestsError", { namespace: "errorBanner" })
    setTooManyRequestsError!: (params: { key: string }) => void;

    @Action("setTooManyRequestsWarning", { namespace: "errorBanner" })
    setTooManyRequestsWarning!: (params: { key: string }) => void;

    @Ref("reportDownloadModal")
    readonly reportDownloadModal!: MessageModalComponent;

    @Ref("vaccineProofDownloadModal")
    readonly vaccineProofDownloadModal!: MessageModalComponent;

    @Ref("vaccineRecordResultModal")
    readonly vaccineRecordResultModal!: MessageModalComponent;

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
    private isReport = false;
    private isReportDownloading = false;
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

    private get isVaccineRecordDownloading(): boolean {
        if (
            this.vaccineRecordActiveHdid === this.dependent.ownerId &&
            this.vaccineRecordStatusChanges > 0
        ) {
            const vaccinationRecord: VaccinationRecord | undefined =
                this.getVaccinationRecord();
            if (vaccinationRecord !== undefined) {
                return vaccinationRecord.status === LoadStatus.REQUESTED;
            }
        }
        return false;
    }

    private get vaccineRecordStatusMessage(): string {
        if (
            this.vaccineRecordActiveHdid === this.dependent.ownerId &&
            this.vaccineRecordStatusChanges > 0
        ) {
            const vaccinationRecord: VaccinationRecord | undefined =
                this.getVaccinationRecord();
            if (vaccinationRecord !== undefined) {
                return vaccinationRecord.statusMessage;
            }
        }
        return "";
    }

    private get vaccineRecordResultMessage(): string {
        if (
            this.vaccineRecordActiveHdid === this.dependent.ownerId &&
            this.vaccineRecordStatusChanges > 0
        ) {
            const vaccinationRecord: VaccinationRecord | undefined =
                this.getVaccinationRecord();
            if (vaccinationRecord !== undefined) {
                return vaccinationRecord.resultMessage;
            }
        }
        return "";
    }

    private get isDownloadImmunizationReportButtonDisabled(): boolean {
        this.logger.debug(
            `isReportDownloading: ${this.isReportDownloading} immunizationItems:  ${this.immunizationItems.length} and recommendationItems: ${this.recommendationItems.length}`
        );
        return (
            this.isReportDownloading ||
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
            immunization: x.recommendedVaccinations,
            due_date:
                x.agentDueDate === undefined || x.agentDueDate === null
                    ? ""
                    : DateWrapper.format(x.agentDueDate),
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
            .catch((err: ResultError) => {
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Delete,
                        source: ErrorSourceType.Dependent,
                        traceId: err.traceId,
                    });
                }
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private downloadCovid19Report(): void {
        this.isReportDownloading = true;
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
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Download,
                        source: ErrorSourceType.Covid19LaboratoryReport,
                        traceId: err.traceId,
                    });
                }
            })
            .finally(() => {
                this.isReportDownloading = false;
            });
    }

    private downloadImmunizationReport(): void {
        this.isReportDownloading = true;

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
            .catch((err: ResultError) => {
                this.logger.error(err.resultMessage);
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Download,
                        source: ErrorSourceType.DependentImmunizationReport,
                        traceId: err.traceId,
                    });
                }
            })
            .finally(() => {
                this.isReportDownloading = false;
            });
    }

    private downloadDocument(): void {
        if (this.isReport) {
            this.logger.debug(
                `Download document from dependent tab: ${this.dependentTab} and report format: ${this.reportFormatType}`
            );
            if (this.dependentTab === 1) {
                this.downloadCovid19Report();
            } else if (this.dependentTab === 2) {
                this.downloadImmunizationReport();
            }
        } else {
            this.downloadVaccinePdf();
        }
    }

    private downloadVaccinePdf(): void {
        this.logger.debug(
            `Downloading vaccine PDF for hdid: ${this.dependent.ownerId}`
        );
        this.trackClickLink("Click Button", "Dependent Proof");
        this.retrieveAuthenticatedVaccineRecord({
            hdid: this.dependent.ownerId,
        });
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
                if (err.statusCode === 429) {
                    this.setTooManyRequestsWarning({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Retrieve,
                        source: ErrorSourceType.Covid19Laboratory,
                        traceId: err.traceId,
                    });
                }
                this.isLoading = false;
            });
    }

    private fetchPatientImmunizations(): void {
        const hdid = this.dependent.ownerId;
        this.logger.debug(`Fetching Patient Immunizations for Hdid: ${hdid}`);
        this.logger.debug(`Logged in user Hdid: ${this.user.hdid}`);
        this.logger.debug(
            `Fetching Patient Immunizations - immunization data loaded: ${this.isImmunizationDataLoaded}`
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
                if (err.statusCode === 429) {
                    this.setTooManyRequestsWarning({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Retrieve,
                        source: ErrorSourceType.Immunization,
                        traceId: err.traceId,
                    });
                }
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

    private getVaccinationRecord(): VaccinationRecord | undefined {
        return this.vaccineRecords.get(this.dependent.ownerId);
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
        this.recommendations = recommendations.filter(
            (x) => x.recommendedVaccinations
        );

        this.recommendations.sort((a, b) => {
            const firstDateEmpty =
                a.agentDueDate === null || a.agentDueDate === undefined;
            const secondDateEmpty =
                b.agentDueDate === null || b.agentDueDate === undefined;

            if (firstDateEmpty && secondDateEmpty) {
                return 0;
            }

            if (firstDateEmpty) {
                return 1;
            }

            if (secondDateEmpty) {
                return -1;
            }

            const firstDate = new DateWrapper(a.agentDueDate);
            const secondDate = new DateWrapper(b.agentDueDate);

            if (firstDate.isBefore(secondDate)) {
                return 1;
            }

            if (firstDate.isAfter(secondDate)) {
                return -1;
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

    private showVaccineProofDownloadConfirmaationModal(): void {
        this.isReport = false;
        this.reportDownloadModal.showModal();
    }

    private showCovid19DownloadConfirmationModal(
        row: Covid19LaboratoryTestRow
    ): void {
        this.isReport = true;
        this.selectedTestRow = row;
        this.reportDownloadModal.showModal();
    }

    private showImmunizationDownloadConfirmationModal(
        reportFormatType: ReportFormatType
    ): void {
        this.isReport = true;
        this.reportFormatType = reportFormatType;
        this.reportDownloadModal.showModal();
    }

    private showDeleteConfirmationModal(): void {
        this.deleteModal.showModal();
    }

    private trackClickLink(action: string, linkType: string | undefined): void {
        if (linkType) {
            SnowPlow.trackEvent({
                action: `${action}`,
                text: `${linkType}`,
            });
        }
    }

    private trackDownload(): void {
        const reportName = "Dependent Immunization";
        const formatTypeName = ReportFormatType[this.reportFormatType];
        const eventName = `${reportName} (${formatTypeName})`;
        this.logger.debug(`Track download for: ${eventName}`);

        EventTracker.downloadReport(eventName);
    }

    @Watch("vaccineRecordStatusChanges")
    private showVaccineRecordResultModal(): void {
        if (this.vaccineRecordActiveHdid === this.dependent.ownerId) {
            const vaccinationRecord: VaccinationRecord | undefined =
                this.getVaccinationRecord();
            if (
                vaccinationRecord !== undefined &&
                vaccinationRecord.resultMessage.length > 0
            ) {
                this.vaccineRecordResultModal.showModal();
            }
        }
    }

    @Watch("vaccineRecordStatusChanges")
    private saveVaccinePdf(): void {
        if (this.vaccineRecordActiveHdid === this.dependent.ownerId) {
            const vaccinationRecord: VaccinationRecord | undefined =
                this.getVaccinationRecord();
            if (
                vaccinationRecord?.record !== undefined &&
                vaccinationRecord.hdid === this.dependent.ownerId &&
                vaccinationRecord.status === LoadStatus.LOADED &&
                vaccinationRecord.download
            ) {
                const mimeType = vaccinationRecord.record.document.mediaType;
                const downloadLink = `data:${mimeType};base64,${vaccinationRecord.record.document.data}`;
                fetch(downloadLink).then((res) => {
                    this.trackClickLink("Download Card", "Dependent Proof");
                    res.blob().then((blob) => saveAs(blob, "VaccineProof.pdf"));
                });
                this.stopAuthenticatedVaccineRecordDownload({
                    hdid: this.dependent.ownerId,
                });
            }
        }
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
                <b-tab no-body active data-testid="dependentTab">
                    <template #title>
                        <div>Profile</div>
                    </template>
                    <div v-if="isExpired" class="p-3">
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
                    <div v-else class="p-3">
                        <b-row>
                            <b-col class="col-12 col-md-6 col-lg-4">
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
                            <b-col class="col-12 col-md-6 col-lg-4">
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
                    no-body
                    @click="fetchCovid19LaboratoryTests"
                >
                    <template #title>
                        <div data-testid="covid19TabTitle">COVID-19</div>
                    </template>
                    <div class="p-3">
                        <div class="d-flex justify-content-center">
                            <hg-button
                                :id="`download-proof-of-vaccination-btn-id-${dependent.ownerId}`"
                                :data-testid="`download-proof-of-vaccination-btn-${dependent.ownerId}`"
                                variant="secondary"
                                @click="
                                    showVaccineProofDownloadConfirmaationModal
                                "
                            >
                                <hg-icon
                                    icon="check-circle"
                                    size="medium"
                                    square
                                    aria-hidden="true"
                                    class="mr-2"
                                />
                                Download Proof of Vaccination
                            </hg-button>
                        </div>
                        <b-spinner v-if="isLoading" class="mt-3" />
                        <h4 v-if="!isLoading" class="pt-3">
                            COVID-19 Test Results
                        </h4>
                        <div
                            v-if="!isLoading && testRows.length === 0"
                            data-testid="covid19NoRecords"
                        >
                            No records found.
                        </div>
                    </div>
                    <b-table-simple
                        v-if="!isLoading && testRows.length > 0"
                        small
                        striped
                        borderless
                        :items="testRows"
                        class="w-100 mb-0"
                        aria-describedby="COVID-19 Test Results"
                    >
                        <b-thead>
                            <b-tr>
                                <b-th class="align-middle">Date</b-th>
                                <b-th
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    Type
                                </b-th>
                                <b-th
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    Status
                                </b-th>
                                <b-th class="align-middle">Result</b-th>
                                <b-th class="align-middle">Report</b-th>
                            </b-tr>
                        </b-thead>
                        <b-tbody>
                            <b-tr v-for="(row, index) in testRows" :key="index">
                                <b-td
                                    data-testid="dependentCovidTestDate"
                                    class="align-middle"
                                >
                                    {{ formatDate(row.test.collectedDateTime) }}
                                </b-td>
                                <b-td
                                    data-testid="dependentCovidTestType"
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    {{ row.test.testType }}
                                </b-td>
                                <b-td
                                    data-testid="dependentCovidTestStatus"
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    {{ row.test.testStatus }}
                                </b-td>
                                <b-td
                                    data-testid="dependentCovidTestLabResult"
                                    class="align-middle"
                                >
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
                                            boundary="viewport"
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
                                </b-td>
                                <b-td class="align-middle">
                                    <hg-button
                                        v-if="
                                            row.reportAvailable &&
                                            row.test.resultReady
                                        "
                                        data-testid="dependentCovidReportDownloadBtn"
                                        variant="link"
                                        class="p-0"
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
                                </b-td>
                            </b-tr>
                        </b-tbody>
                    </b-table-simple>
                </b-tab>
                <b-tab
                    v-if="isImmunizationTabShown"
                    :disabled="isExpired"
                    no-body
                    :data-testid="`immunization-tab-${dependent.ownerId}`"
                    @click="fetchPatientImmunizations"
                >
                    <template #title>
                        <div
                            :data-testid="`immunization-tab-title-${dependent.ownerId}`"
                        >
                            Immunization
                        </div>
                    </template>
                    <div id="dependent-immunization-disclaimer">
                        <b-alert
                            :show="immunizationItems.length != 0"
                            variant="info"
                            class="mt-3 mb-1 mx-3"
                            data-testid="dependent-immunization-disclaimer-alert"
                        >
                            <span>
                                If your dependent's immunizations are missing or
                                incorrect,
                                <a
                                    href="https://www.immunizationrecord.gov.bc.ca/"
                                    target="_blank"
                                    rel="noopener"
                                    >fill in this online form</a
                                >.
                            </span>
                        </b-alert>
                    </div>
                    <b-spinner v-if="isLoading" class="m-3" />
                    <div
                        v-else
                        :data-testid="`immunization-tab-div-${dependent.ownerId}`"
                    >
                        <b-card no-body class="border-0">
                            <b-tabs card nav-wrapper-class="bg-white">
                                <b-tab title="History" no-body active>
                                    <div
                                        v-if="immunizationItems.length === 0"
                                        class="p-3"
                                        :data-testid="`immunization-history-no-rows-found-${dependent.ownerId}`"
                                    >
                                        No records found. If this is your first
                                        time checking for records, please try
                                        refreshing the page in a few minutes.
                                    </div>
                                    <div v-else>
                                        <b-row
                                            align-h="end"
                                            class="p-3"
                                            no-gutters
                                        >
                                            <b-col
                                                cols="auto"
                                                align-self="center"
                                            >
                                                <b-dropdown
                                                    v-if="
                                                        immunizationItems.length !=
                                                        0
                                                    "
                                                    id="download-immunization-history-report-btn"
                                                    text="Download"
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
                                            </b-col>
                                        </b-row>
                                        <b-table-simple
                                            small
                                            striped
                                            borderless
                                            :items="immunizationItems"
                                            class="w-100 mb-0"
                                            aria-describedby="Immunization History"
                                            :data-testid="`immunization-history-table-${dependent.ownerId}`"
                                        >
                                            <b-thead>
                                                <b-tr>
                                                    <b-th class="align-middle">
                                                        Date
                                                    </b-th>
                                                    <b-th class="align-middle">
                                                        Immunization
                                                    </b-th>
                                                    <b-th
                                                        class="d-none d-lg-table-cell align-middle"
                                                    >
                                                        Agent
                                                    </b-th>
                                                    <b-th
                                                        class="d-none d-lg-table-cell align-middle"
                                                    >
                                                        Product
                                                    </b-th>
                                                    <b-th class="align-middle">
                                                        Provider/Clinic
                                                    </b-th>
                                                    <th
                                                        class="d-none d-lg-table-cell align-middle"
                                                    >
                                                        Lot Number
                                                    </th>
                                                </b-tr>
                                            </b-thead>
                                            <b-tbody>
                                                <b-tr
                                                    v-for="(
                                                        row, index
                                                    ) in immunizationItems"
                                                    :key="index"
                                                >
                                                    <b-td
                                                        :data-testid="`history-immunization-date-${dependent.ownerId}-${index}`"
                                                        class="align-middle"
                                                    >
                                                        {{ row.date }}
                                                    </b-td>
                                                    <b-td
                                                        :data-testid="`history-product-${dependent.ownerId}-${index}`"
                                                        class="align-middle"
                                                    >
                                                        {{ row.immunization }}
                                                    </b-td>
                                                    <b-td
                                                        :data-testid="`history-immunizing-agent-${dependent.ownerId}-${index}`"
                                                        class="d-none d-lg-table-cell align-middle"
                                                    >
                                                        {{ row.agent }}
                                                    </b-td>
                                                    <b-td
                                                        :data-testid="`history-immunizing-product-${dependent.ownerId}-${index}`"
                                                        class="d-none d-lg-table-cell align-middle"
                                                    >
                                                        {{ row.product }}
                                                    </b-td>
                                                    <b-td
                                                        :data-testid="`history-provider-clinic-${dependent.ownerId}-${index}`"
                                                        class="align-middle"
                                                    >
                                                        {{
                                                            row.provider_clinic
                                                        }}
                                                    </b-td>
                                                    <b-td
                                                        :data-testid="`history-lot-number-${dependent.ownerId}-${index}`"
                                                        class="d-none d-lg-table-cell align-middle"
                                                    >
                                                        {{ row.lotNumber }}
                                                    </b-td>
                                                </b-tr>
                                            </b-tbody>
                                        </b-table-simple>
                                    </div>
                                </b-tab>
                                <b-tab title="Forecasts" no-body>
                                    <div class="p-3">
                                        <b-row align-h="end" no-gutters>
                                            <b-col cols="12" :md="true">
                                                <p class="mb-md-0">
                                                    School-aged children are
                                                    offered most immunizations
                                                    in their school,
                                                    particularly in grades 6 and
                                                    9. The school can let you
                                                    know which vaccines are
                                                    offered. You need to book an
                                                    appointment to get your
                                                    child vaccinated against
                                                    COVID‑19.
                                                    <a
                                                        href="https://www2.gov.bc.ca/gov/content/covid-19/vaccine"
                                                        rel="noopener"
                                                        target="_blank"
                                                        >Find out how.</a
                                                    >
                                                </p>
                                            </b-col>
                                            <b-col
                                                v-if="
                                                    recommendationItems.length >
                                                    0
                                                "
                                                cols="auto"
                                                align-self="center"
                                                class="pl-3"
                                            >
                                                <b-dropdown
                                                    id="download-immunization-forecast-report-btn"
                                                    text="Download"
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
                                            </b-col>
                                        </b-row>
                                        <div
                                            v-if="
                                                recommendationItems.length === 0
                                            "
                                            :data-testid="`immunization-forecast-no-rows-found-${dependent.ownerId}`"
                                        >
                                            No records found.
                                        </div>
                                    </div>
                                    <b-table-simple
                                        v-if="recommendationItems.length > 0"
                                        small
                                        striped
                                        borderless
                                        class="w-100 mb-0"
                                        aria-describedby="Immunization Forecast"
                                        :data-testid="`immunization-forecast-table-${dependent.ownerId}`"
                                    >
                                        <b-thead>
                                            <b-tr>
                                                <b-th class="align-middle">
                                                    Immunization
                                                </b-th>
                                                <b-th class="align-middle">
                                                    Due Date
                                                </b-th>
                                                <b-th class="align-middle">
                                                    Status
                                                </b-th>
                                            </b-tr>
                                        </b-thead>
                                        <b-tbody>
                                            <b-tr
                                                v-for="(
                                                    row, index
                                                ) in recommendationItems"
                                                :key="index"
                                            >
                                                <b-td
                                                    :data-testid="`forecast-immunization-${dependent.ownerId}-${index}`"
                                                    class="align-middle"
                                                >
                                                    {{ row.immunization }}
                                                </b-td>
                                                <b-td
                                                    :data-testid="`forecast-due-date-${dependent.ownerId}-${index}`"
                                                    class="align-middle"
                                                >
                                                    {{ row.due_date }}
                                                </b-td>
                                                <b-td
                                                    :data-testid="`forecast-status-${dependent.ownerId}-${index}`"
                                                    class="align-middle"
                                                >
                                                    {{ row.status }}
                                                </b-td>
                                            </b-tr>
                                        </b-tbody>
                                    </b-table-simple>
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
            :is-loading="isReportDownloading"
            :full-screen="false"
        />
        <LoadingComponent
            :is-loading="isVaccineRecordDownloading"
            :text="vaccineRecordStatusMessage"
            :full-screen="false"
        />
        <delete-modal-component
            ref="deleteModal"
            title="Remove Dependent"
            confirm="Remove Dependent"
            message="Are you sure you want to remove this dependent?"
            @submit="deleteDependent"
        />
        <MessageModalComponent
            ref="reportDownloadModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="downloadDocument"
        />
        <MessageModalComponent
            ref="vaccineRecordResultModal"
            ok-only
            title="Alert"
            :message="vaccineRecordResultMessage"
        />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

th {
    font-weight: bold;
}

td,
th {
    text-align: center;
}

.dependentMenu {
    color: $soft_text;
}

.card-title {
    padding-left: 14px;
    font-size: 1.2em;
}
</style>
