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
import {
    ImmunizationAgent,
    ImmunizationEvent,
    Recommendation,
} from "@/models/immunizationModel";
import {
    Covid19LaboratoryTest,
    LaboratoryReport,
    LaboratoryUtil,
} from "@/models/laboratory";
import { ResultError } from "@/models/requestResult";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IDependentService,
    IImmunizationService,
    ILaboratoryService,
    ILogger,
} from "@/services/interfaces";

library.add(faEllipsisV, faDownload, faInfoCircle);

interface Covid19LaboratoryTestRow {
    id: string;
    reportAvailable: boolean;
    test: Covid19LaboratoryTest;
}

interface ImmunizationRow {
    date: StringISODate;
    immunization: string;
    agents: string;
    provider_clinic: string;
    lotNumber: string;
}

interface RecommendationRow {
    immunization: string;
    due_date: StringISODate;
    status: string;
}

@Component({
    components: {
        BTabs,
        BTab,
        MessageModalComponent,
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

    @Ref("sensitivedocumentDownloadModal")
    readonly sensitivedocumentDownloadModal!: MessageModalComponent;

    @Ref("deleteModal")
    readonly deleteModal!: DeleteModalComponent;

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
    private isImmunizationDataLoaded = false;

    private selectedTestRow!: Covid19LaboratoryTestRow;

    private get isExpired() {
        let birthDate = new DateWrapper(
            this.dependent.dependentInformation.dateOfBirth
        );
        let now = new DateWrapper();
        return (
            now.diff(birthDate, "year").years >
            this.webClientConfig.maxDependentAge
        );
    }

    private get immunizationTabShown(): boolean {
        return this.webClientConfig.modules["DependentImmunizationTab"];
    }

    private get immunizationItems(): ImmunizationRow[] {
        return this.immunizations.map<ImmunizationRow>((x) => {
            return {
                date: DateWrapper.format(x.dateOfImmunization),
                immunization: x.immunization.name,
                agents: "Agent 1, Agent 2, Agent 3",
                //agents: this.getAgentNames(x.immunization.immunizationAgents),
                //provider_clinic: x.providerOrClinic,
                provider_clinic: "Island Health District 99999999999999",
                //lotNumber: this.getAgentLotNumbers(
                //    x.immunization.immunizationAgents
                //),
                lotNumber: "ET93847-99990asdafa, Lot 1, Lot 2, Lot 3",
            };
        });
    }

    private get recomendationItems(): RecommendationRow[] {
        return this.recommendations.map<RecommendationRow>((x) => {
            return {
                //immunization: x.immunization.name,
                immunization: "COVID-19",
                due_date:
                    x.diseaseDueDate === undefined || x.diseaseDueDate === null
                        ? ""
                        : DateWrapper.format(x.diseaseDueDate),
                status: x.status || "",
            };
        });
    }

    private created() {
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

    private showSensitiveDocumentDownloadModal(row: Covid19LaboratoryTestRow) {
        this.selectedTestRow = row;
        this.sensitivedocumentDownloadModal.showModal();
    }

    private fetchCovid19LaboratoryTests() {
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
                        payload.orders.flatMap<Covid19LaboratoryTestRow>(
                            (o) => {
                                return o.labResults.map<Covid19LaboratoryTestRow>(
                                    (r) => {
                                        return {
                                            id: o.id,
                                            reportAvailable: o.reportAvailable,
                                            test: r,
                                        };
                                    }
                                );
                            }
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
                    setTimeout(() => {
                        this.fetchCovid19LaboratoryTests();
                    }, payload.retryin);
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

    private fetchPatientImmunizations() {
        const hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A"; //this.dependent.dependentInformation.hdid
        this.logger.debug(
            `Fetching Patient Immunizations - immunization data laoded: ${this.isImmunizationDataLoaded}`
        );

        if (this.isImmunizationDataLoaded) {
            return;
        }

        this.logger.debug(`Fetching Patient Immunizations for Hdid: ${hdid}`);

        this.isLoading = true;
        this.immunizationService
            .getPatientImmunizations(hdid)
            .then((result) => {
                if (result.resultStatus == ResultType.Success) {
                    const payload = result.resourcePayload;
                    if (payload.loadState.refreshInProgress) {
                        this.logger.info("Re-querying Patient Immunizations");
                        setTimeout(() => {
                            this.fetchPatientImmunizations();
                        }, 10000);
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

    private getAgentLotNumbers(
        immunizationAgents: ImmunizationAgent[]
    ): string {
        const lotNumbers: string[] = immunizationAgents.map<string>(
            (x) => x.lotNumber
        );
        return lotNumbers.join(", ");
    }

    private getAgentNames(immunizationAgents: ImmunizationAgent[]): string {
        const agents: string[] = immunizationAgents.map<string>((x) => x.name);
        return agents.join(", ");
    }

    private setImmunizations(immunizations: ImmunizationEvent[]) {
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

    private setRecommendations(recommendations: Recommendation[]) {
        //this.recommendations = recommendations.filter(
        //    (x) => x.immunization.name !== null && x.immunization.name !== ""
        //);

        this.recommendations = recommendations;
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
                return 1;
            }

            if (firstDate.isAfter(secondDate)) {
                return -1;
            }

            return 0;
        });
    }

    private sortEntries() {
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

    private getReport() {
        let test = this.selectedTestRow.test;
        this.laboratoryService
            .getReportDocument(
                this.selectedTestRow.id,
                this.dependent.ownerId,
                true
            )
            .then((result) => {
                let report: LaboratoryReport = result.resourcePayload;
                fetch(
                    `data:${report.mediaType};${report.encoding},${report.data}`
                )
                    .then((response) => response.blob())
                    .then((blob) => {
                        saveAs(
                            blob,
                            `COVID_Result_${this.dependent.dependentInformation.firstname}${this.dependent.dependentInformation.lastname}_${test.collectedDateTime}.pdf`
                        );
                    });
            })
            .catch((err: ResultError) => {
                this.logger.error(err.resultMessage);
                this.addError({
                    errorType: ErrorType.Download,
                    source: ErrorSourceType.Covid19LaboratoryReport,
                    traceId: err.traceId,
                });
            });
    }

    private deleteDependent(): void {
        this.isLoading = true;
        this.dependentService
            .removeDependent(this.user.hdid, this.dependent)
            .then(() => {
                this.needsUpdate();
            })
            .catch((err: ResultError) => {
                this.addError({
                    errorType: ErrorType.Delete,
                    source: ErrorSourceType.Dependent,
                    traceId: err.traceId,
                });
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private showDeleteConfirmationModal(): void {
        this.deleteModal.showModal();
    }

    @Emit()
    private needsUpdate() {
        return;
    }

    private formatDate(date: StringISODate): string {
        return new DateWrapper(date).format();
    }

    private checkResultReady(test: Covid19LaboratoryTest): boolean {
        return LaboratoryUtil.isTestResultReady(test.testStatus);
    }

    private formatResult(test: Covid19LaboratoryTest): string {
        if (this.checkResultReady(test)) {
            return test?.labResultOutcome ?? "";
        } else {
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
}
</script>

<template>
    <div>
        <b-card no-body>
            <b-tabs card>
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
                        <b-col><b-spinner /></b-col>
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
                                    class="font-weight-bold"
                                    :class="
                                        getOutcomeClasses(
                                            row.test.labResultOutcome
                                        )
                                    "
                                >
                                    {{ formatResult(row.test) }}
                                </span>
                                <span v-if="checkResultReady(row.test)">
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
                                        checkResultReady(row.test)
                                    "
                                    data-testid="dependentCovidReportDownloadBtn"
                                    variant="link"
                                    class="p-1"
                                    @click="
                                        showSensitiveDocumentDownloadModal(row)
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
                    v-if="immunizationTabShown"
                    :disabled="isExpired"
                    data-testid="immunization-tab"
                    class="tableTab mt-2"
                    @click="fetchPatientImmunizations()"
                >
                    <template #title>
                        <div data-testid="immunization-tab-title">
                            Immunization
                        </div>
                    </template>
                    <b-row v-if="isLoading" class="m-2">
                        <b-col><b-spinner /></b-col>
                    </b-row>
                    <div v-else>
                        <b-card no-body>
                            <b-tabs pills card>
                                <b-tab
                                    title="History"
                                    active
                                    data-testid="history-tab"
                                >
                                    <b-row
                                        v-if="immunizationItems.length == 0"
                                        class="m-2"
                                    >
                                        <b-col
                                            data-testid="immunization-history-table"
                                        >
                                            No records found.
                                        </b-col>
                                    </b-row>
                                    <table
                                        v-else
                                        class="w-100"
                                        aria-describedby="Immunization History"
                                    >
                                        <tr>
                                            <th scope="col">Date</th>
                                            <th scope="col">Product</th>
                                            <th
                                                scope="col"
                                                class="d-none d-sm-table-cell"
                                            >
                                                Immunizing Agent
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
                                                :data-testid="`history-immunization-date-${index}`"
                                            >
                                                {{ row.date }}
                                            </td>
                                            <td
                                                :data-testid="`history-product-${index}`"
                                            >
                                                {{ row.immunization }}
                                            </td>
                                            <td
                                                :data-testid="`history-immunizing-agent-${index}`"
                                                class="d-none d-sm-table-cell"
                                            >
                                                {{ row.agents }}
                                            </td>
                                            <td
                                                :data-testid="`history-provider-clinic-${index}`"
                                            >
                                                {{ row.provider_clinic }}
                                            </td>
                                            <td
                                                :data-testid="`history-lot-number-${index}`"
                                                class="d-none d-sm-table-cell"
                                            >
                                                {{ row.lotNumber }}
                                            </td>
                                        </tr>
                                    </table>
                                </b-tab>
                                <b-tab
                                    title="Forecasts"
                                    data-testid="forecast-tab"
                                >
                                    <b-row
                                        v-if="recomendationItems.length == 0"
                                        class="m-2"
                                    >
                                        <b-col
                                            data-testid="immunization-forecast-table"
                                        >
                                            No records found.
                                        </b-col>
                                    </b-row>
                                    <table
                                        v-else
                                        class="w-100"
                                        aria-describedby="Immunization Forecast"
                                    >
                                        <tr>
                                            <th scope="col">Immunization</th>
                                            <th
                                                scope="col"
                                                class="d-none d-sm-table-cell"
                                            >
                                                Due Date
                                            </th>
                                            <th
                                                scope="col"
                                                class="d-none d-sm-table-cell"
                                            >
                                                Status
                                            </th>
                                        </tr>
                                        <tr
                                            v-for="(
                                                row, index
                                            ) in recomendationItems"
                                            :key="index"
                                        >
                                            <td
                                                :data-testid="`forecast-immunization-${index}`"
                                            >
                                                {{ row.immunization }}
                                            </td>
                                            <td
                                                :data-testid="`forecast-due-date-${index}`"
                                                class="d-none d-sm-table-cell"
                                            >
                                                {{ row.due_date }}
                                            </td>
                                            <td
                                                :data-testid="`forecast-status-${index}`"
                                                class="d-none d-sm-table-cell"
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
        <delete-modal-component
            ref="deleteModal"
            title="Remove Dependent"
            message="Are you sure you want to remove this dependent?"
            @submit="deleteDependent()"
        />
        <MessageModalComponent
            ref="sensitivedocumentDownloadModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="getReport"
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
