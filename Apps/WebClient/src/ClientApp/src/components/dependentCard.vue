<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faEllipsisV, faFileDownload } from "@fortawesome/free-solid-svg-icons";
import { BTab, BTabs } from "bootstrap-vue";
import Vue from "vue";
import { Component, Emit, Prop, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import DeleteModalComponent from "@/components/modal/deleteConfirmation.vue";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
import { ResultType } from "@/constants/resulttype";
import BannerError from "@/models/bannerError";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import type { Dependent } from "@/models/dependent";
import {
    LaboratoryOrder,
    LaboratoryReport,
    LaboratoryUtil,
} from "@/models/laboratory";
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

library.add(faEllipsisV, faFileDownload);

@Component({
    components: {
        BTabs,
        BTab,
        MessageModalComponent,
        DeleteModalComponent,
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
    private labResults: LaboratoryOrder[] = [];
    private isDataLoaded = false;

    private selectedLabOrder!: LaboratoryOrder;

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

    private showSensitiveDocumentDownloadModal(labOrder: LaboratoryOrder) {
        this.selectedLabOrder = labOrder;
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
                    this.labResults = results.resourcePayload;
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
        this.labResults.sort((a, b) => {
            let dateA = new DateWrapper(a.labResults[0].collectedDateTime);
            let dateB = new DateWrapper(b.labResults[0].collectedDateTime);
            return dateA.isAfter(dateB) ? -1 : dateA.isBefore(dateB) ? 1 : 0;
        });
    }

    private getReport() {
        let labResult = this.selectedLabOrder.labResults[0];
        this.laboratoryService
            .getReportDocument(this.selectedLabOrder.id, this.dependent.ownerId)
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
                                    >Remove Dependent</hg-button
                                >
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
                                        ></b-form-input>
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
                                        ></b-form-input>
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
                        <b-col><b-spinner></b-spinner></b-col>
                    </b-row>
                    <b-row v-else-if="labResults.length == 0" class="m-2">
                        <b-col data-testid="covid19NoRecords"
                            >No records found.</b-col
                        >
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
                                Test Location
                            </th>
                            <th scope="col">Result</th>
                            <th scope="col">Report</th>
                        </tr>
                        <tr
                            v-for="item in labResults"
                            :key="item.labResults[0].id"
                        >
                            <td data-testid="dependentCovidTestDate">
                                {{
                                    formatDate(
                                        item.labResults[0].collectedDateTime
                                    )
                                }}
                            </td>
                            <td
                                data-testid="dependentCovidTestType"
                                class="d-none d-sm-table-cell"
                            >
                                {{ item.labResults[0].testType }}
                            </td>
                            <td
                                data-testid="dependentCovidTestLocation"
                                class="d-none d-sm-table-cell"
                            >
                                {{ item.location }}
                            </td>
                            <td
                                data-testid="dependentCovidTestLabResult"
                                :class="item.labResults[0].labResultOutcome"
                            >
                                {{ formatResult(item.labResults[0]) }}
                            </td>
                            <td>
                                <hg-button
                                    v-if="
                                        item.reportAvailable &&
                                        checkResultReady(item.labResults[0])
                                    "
                                    data-testid="dependentCovidReportDownloadBtn"
                                    variant="link"
                                    @click="
                                        showSensitiveDocumentDownloadModal(item)
                                    "
                                >
                                    <hg-icon
                                        icon="file-download"
                                        size="medium"
                                        aria-hidden="true"
                                    />
                                </hg-button>
                            </td>
                        </tr>
                    </table>
                    <div class="p-1">
                        <strong>What to expect next</strong>
                        <p>
                            If you receive a
                            <strong>positive</strong> COVID-19 result:
                        </p>
                        <ul>
                            <li>
                                You and the people you live with need to
                                self-isolate now.
                            </li>
                            <li>
                                Public health will contact you in 2 to 3 days
                                with further instructions.
                            </li>
                            <li>
                                If you are a health care worker, please notify
                                your employer of your positive result.
                            </li>
                            <li>
                                Monitor your health and contact a health care
                                provider or call 8-1-1 if you are concerned
                                about your symptoms.
                            </li>
                            <li>
                                Go to
                                <a
                                    href="http://www.bccdc.ca/results"
                                    target="blank_"
                                    >www.bccdc.ca/results</a
                                >
                                for more information about your test result.
                            </li>
                        </ul>
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
        ></delete-modal-component>
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
td.Positive {
    color: red;
}
td.Negative {
    color: green;
}
.card-title {
    padding-left: 14px;
    font-size: 1.2em;
}
</style>
