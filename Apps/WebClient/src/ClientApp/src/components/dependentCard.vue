<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Ref } from "vue-property-decorator";
import type { Dependent } from "@/models/dependent";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import { Action, Getter } from "vuex-class";
import { ResultError } from "@/models/requestResult";
import { LaboratoryOrder, LaboratoryReport } from "@/models/laboratory";
import DeleteModalComponent from "@/components/modal/deleteConfirmation.vue";
import { ResultType } from "@/constants/resulttype";
import BannerError from "@/models/bannerError";
import ErrorTranslator from "@/utility/errorTranslator";
import container from "@/plugins/inversify.config";
import {
    ILogger,
    ILaboratoryService,
    IDependentService,
} from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import User from "@/models/user";
import { BTabs, BTab } from "bootstrap-vue";
import { IconDefinition, library } from "@fortawesome/fontawesome-svg-core";
import { faEllipsisV, faFileDownload } from "@fortawesome/free-solid-svg-icons";
import { LaboratoryResult } from "@/models/laboratory";
library.add(faFileDownload);

@Component({
    components: {
        BTabs,
        BTab,
        DeleteModalComponent,
    },
})
export default class DependentCardComponent extends Vue {
    @Prop() dependent!: Dependent;

    @Getter("user", { namespace: "user" }) user!: User;

    @Action("addError", { namespace: "errorBanner" })
    addError!: (error: BannerError) => void;

    @Ref("deleteModal")
    readonly deleteModal!: DeleteModalComponent;

    private message =
        "Are you sure you want to remove " +
        this.dependent.dependentInformation.name +
        " from your list of dependents?";

    private isLoading = false;
    private logger!: ILogger;
    private laboratoryService!: ILaboratoryService;
    private dependentService!: IDependentService;
    private labResults: LaboratoryOrder[] = [];
    private isDataLoaded = false;

    private get isExpired() {
        let birthDate = new DateWrapper(
            this.dependent.dependentInformation.dateOfBirth
        );
        let now = new DateWrapper();
        return now.diff(birthDate, "year").years > 19;
    }

    private get menuIcon(): IconDefinition {
        return faEllipsisV;
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.laboratoryService = container.get<ILaboratoryService>(
            SERVICE_IDENTIFIER.LaboratoryService
        );
        this.dependentService = container.get<IDependentService>(
            SERVICE_IDENTIFIER.DependentService
        );
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

    private getReport(labOrder: LaboratoryOrder) {
        let labResult = labOrder.labResults[0];
        this.laboratoryService
            .getReportDocument(labOrder.id, this.dependent.ownerId)
            .then((result) => {
                const link = document.createElement("a");
                let report: LaboratoryReport = result.resourcePayload;
                link.href = `data:${report.mediaType};${report.encoding},${report.data}`;
                link.download = `COVID_Result_${this.dependent.dependentInformation.name}_${labResult.collectedDateTime}.pdf`;
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

    private showConfirmationModal(): void {
        this.deleteModal.showModal();
    }

    @Emit()
    private needsUpdate() {
        return;
    }

    private formatDate(date: StringISODate): string {
        return new DateWrapper(date).format("yyyy-MM-dd");
    }

    private checkResultReady(labResult: LaboratoryResult): boolean {
        return labResult.testStatus == "Final";
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
                                    as they have turned 19
                                </p>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col class="d-flex justify-content-center">
                                <b-button type="link" @click="deleteDependent()"
                                    >Remove Dependent</b-button
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
                            <b-col class="col-12 col-sm-4">
                                <b-row>
                                    <b-col class="col-12">Gender</b-col>
                                    <b-col class="col-12">
                                        <b-form-input
                                            v-model="
                                                dependent.dependentInformation
                                                    .gender
                                            "
                                            data-testid="dependentGender"
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
                    <table v-else class="w-100">
                        <tr>
                            <th>Test Date</th>
                            <th class="d-none d-sm-table-cell">Test Type</th>
                            <th class="d-none d-sm-table-cell">
                                Test Location
                            </th>
                            <th>Result</th>
                            <th>Report</th>
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
                                <b-btn
                                    v-if="checkResultReady(item.labResults[0])"
                                    data-testid="dependentCovidReportDownloadBtn"
                                    variant="link"
                                    @click="getReport(item)"
                                >
                                    <font-awesome-icon
                                        icon="file-download"
                                        aria-hidden="true"
                                        size="1x"
                                    />
                                </b-btn>
                            </td>
                        </tr>
                    </table>
                    <div class="p-1">
                        <strong>What to expect next</strong>
                        <p>
                            If you receive a
                            <b>positive</b> COVID-19 result:
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
                                    {{ dependent.dependentInformation.name }}
                                </span>
                            </b-col>
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
                                        <font-awesome-icon
                                            data-testid="dependentMenuBtn"
                                            class="dependentMenu"
                                            :icon="menuIcon"
                                            size="1x"
                                        ></font-awesome-icon>
                                    </template>
                                    <b-dropdown-item
                                        data-testid="deleteDependentMenuBtn"
                                        class="menuItem"
                                        @click="showConfirmationModal()"
                                    >
                                        Delete
                                    </b-dropdown-item>
                                </b-nav-item-dropdown>
                            </li>
                        </b-row>
                    </div>
                </template>
            </b-tabs>
        </b-card>
        <delete-modal-component
            ref="deleteModal"
            title="Delete Dependent"
            message="Are you sure you want to delete this dependent?"
            @submit="deleteDependent()"
        ></delete-modal-component>
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
