<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faDownload,
    faEllipsisV,
    faInfoCircle,
} from "@fortawesome/free-solid-svg-icons";
import { BTab, BTabs } from "bootstrap-vue";
import Vue from "vue";
import { Component, Emit, Prop, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LaboratoryResultDescriptionComponent from "@/components/laboratory/laboratoryResultDescription.vue";
import DeleteModalComponent from "@/components/modal/deleteConfirmation.vue";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
import { ResultType } from "@/constants/resulttype";
import BannerError from "@/models/bannerError";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import type { Dependent } from "@/models/dependent";
import { LaboratoryReport, LaboratoryUtil } from "@/models/laboratory";
import { LaboratoryResult } from "@/models/laboratory";
import { ResultError } from "@/models/requestResult";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import {
    IDependentService,
    ILaboratoryService,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

library.add(faEllipsisV, faDownload, faInfoCircle);

interface LaboratoryResultRow {
    id: string;
    reportAvailable: boolean;
    result: LaboratoryResult;
}

@Component({
    components: {
        BTabs,
        BTab,
        MessageModalComponent,
        DeleteModalComponent,
        LaboratoryResultDescriptionComponent,
    },
})
export default class DependentCardComponent extends Vue {
    @Prop() dependent!: Dependent;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;

    @Action("addError", { namespace: "errorBanner" })
    addError!: (error: BannerError) => void;

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
    private laboratoryService!: ILaboratoryService;
    private dependentService!: IDependentService;
    private labResultRows: LaboratoryResultRow[] = [];
    private isDataLoaded = false;

    private selectedLabResultRow!: LaboratoryResultRow;

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

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.laboratoryService = container.get<ILaboratoryService>(
            SERVICE_IDENTIFIER.LaboratoryService
        );
        this.dependentService = container.get<IDependentService>(
            SERVICE_IDENTIFIER.DependentService
        );
    }

    private showSensitiveDocumentDownloadModal(row: LaboratoryResultRow) {
        this.selectedLabResultRow = row;
        this.sensitivedocumentDownloadModal.showModal();
    }

    private fetchLaboratoryResults() {
        if (this.isDataLoaded) {
            return;
        }
        this.isLoading = true;
        this.laboratoryService
            .getOrders(this.dependent.ownerId)
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    this.labResultRows =
                        results.resourcePayload.flatMap<LaboratoryResultRow>(
                            (o) => {
                                return o.labResults.map<LaboratoryResultRow>(
                                    (r) => {
                                        return {
                                            id: o.id,
                                            reportAvailable: o.reportAvailable,
                                            result: r,
                                        };
                                    }
                                );
                            }
                        );
                    this.sortEntries();
                    this.isDataLoaded = true;
                } else {
                    this.logger.error(
                        "Error returned from the laboratory call: " +
                            JSON.stringify(results.resultError)
                    );
                    this.addError(
                        ErrorTranslator.toBannerError(
                            "Fetch Laboratory Error",
                            results.resultError
                        )
                    );
                }
            })
            .catch((err) => {
                this.logger.error(err);
                this.addError(
                    ErrorTranslator.toBannerError("Fetch Laboratory Error", err)
                );
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private sortEntries() {
        this.labResultRows.sort((a, b) => {
            let dateA = new DateWrapper(a.result.collectedDateTime);
            let dateB = new DateWrapper(b.result.collectedDateTime);
            return dateA.isAfter(dateB) ? -1 : dateA.isBefore(dateB) ? 1 : 0;
        });
    }

    private getReport() {
        let labResult = this.selectedLabResultRow.result;
        this.laboratoryService
            .getReportDocument(
                this.selectedLabResultRow.id,
                this.dependent.ownerId
            )
            .then((result) => {
                const link = document.createElement("a");
                let report: LaboratoryReport = result.resourcePayload;
                link.href = `data:${report.mediaType};${report.encoding},${report.data}`;
                link.download = `COVID_Result_${this.dependent.dependentInformation.firstname}${this.dependent.dependentInformation.lastname}_${labResult.collectedDateTime}.pdf`;
                link.click();
                URL.revokeObjectURL(link.href);
            })
            .catch((err) => {
                this.logger.error(err);
                this.addError(
                    ErrorTranslator.toBannerError(
                        "Download Laboratory Report Error",
                        err
                    )
                );
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
                this.addError(
                    ErrorTranslator.toBannerError(
                        "Error removing dependent",
                        err
                    )
                );
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

    private checkResultReady(labResult: LaboratoryResult): boolean {
        return LaboratoryUtil.isTestResultReady(labResult.testStatus);
    }

    private formatResult(labResult: LaboratoryResult): string {
        if (this.checkResultReady(labResult)) {
            return labResult?.labResultOutcome ?? "";
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
                    @click="fetchLaboratoryResults()"
                >
                    <template #title>
                        <div data-testid="covid19TabTitle">COVID-19</div>
                    </template>
                    <b-row v-if="isLoading" class="m-2">
                        <b-col><b-spinner /></b-col>
                    </b-row>
                    <b-row v-else-if="labResultRows.length == 0" class="m-2">
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
                        <tr v-for="(row, index) in labResultRows" :key="index">
                            <td data-testid="dependentCovidTestDate">
                                {{ formatDate(row.result.collectedDateTime) }}
                            </td>
                            <td
                                data-testid="dependentCovidTestType"
                                class="d-none d-sm-table-cell"
                            >
                                {{ row.result.testType }}
                            </td>
                            <td
                                data-testid="dependentCovidTestStatus"
                                class="d-none d-sm-table-cell"
                            >
                                {{ row.result.testStatus }}
                            </td>
                            <td data-testid="dependentCovidTestLabResult">
                                <span
                                    class="font-weight-bold"
                                    :class="
                                        getOutcomeClasses(
                                            row.result.labResultOutcome
                                        )
                                    "
                                >
                                    {{ formatResult(row.result) }}
                                </span>
                                <span v-if="checkResultReady(row.result)">
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
                                    >
                                        <LaboratoryResultDescriptionComponent
                                            class="p-2"
                                            :description="
                                                row.result.resultDescription
                                            "
                                            :link="row.result.resultLink"
                                        />
                                    </b-popover>
                                </span>
                            </td>
                            <td>
                                <hg-button
                                    v-if="
                                        row.reportAvailable &&
                                        checkResultReady(row.result)
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
