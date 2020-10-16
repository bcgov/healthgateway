<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import Dependent from "@/models/dependent";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import { Action, Getter } from "vuex-class";
import RequestResult from "@/models/requestResult";
import { LaboratoryOrder, LaboratoryReport } from "@/models/laboratory";
import { ResultType } from "@/constants/resulttype";
import BannerError from "@/models/bannerError";
import ErrorTranslator from "@/utility/errorTranslator";
import container from "@/plugins/inversify.config";
import { ILogger, ILaboratoryService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import User from "@/models/user";
import { BTabs, BTab } from "bootstrap-vue";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faFileDownload } from "@fortawesome/free-solid-svg-icons";
library.add(faFileDownload);

@Component({
    components: {
        BTabs,
        BTab,
    },
})
export default class DependentCardComponent extends Vue {
    @Prop() dependent!: Dependent;

    @Getter("user", { namespace: "user" }) user!: User;

    @Action("getOrders", { namespace: "laboratory" })
    getLaboratoryOrders!: (params: {
        hdid: string;
    }) => Promise<RequestResult<LaboratoryOrder[]>>;

    @Action("addError", { namespace: "errorBanner" })
    addError!: (error: BannerError) => void;

    private isLoading = false;
    private logger!: ILogger;
    private laboratoryService!: ILaboratoryService;
    private labResults: LaboratoryOrder[] = [];
    private isDataLoaded = false;

    private get isExpired() {
        let birthDate = new DateWrapper(this.dependent.dateOfBirth);
        let now = new DateWrapper();
        return now.diff(birthDate, "year").years > 19;
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.laboratoryService = container.get<ILaboratoryService>(
            SERVICE_IDENTIFIER.LaboratoryService
        );
    }

    private fetchLaboratoryResults() {
        if (this.isDataLoaded) {
            return;
        }
        this.isLoading = true;
        this.getLaboratoryOrders({ hdid: this.dependent.hdid })
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
            .getReportDocument(labResult.id, this.user.hdid)
            .then((result) => {
                const link = document.createElement("a");
                let report: LaboratoryReport = result.resourcePayload;
                link.href = `data:${report.mediaType};${report.encoding},${report.data}`;
                link.download = `COVID_Result_${this.dependent.name}_${labResult.collectedDateTime}.pdf`;
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

    private formatDate(date: StringISODate): string {
        return new DateWrapper(date).format("yyyy-MM-dd");
    }
}
</script>

<template>
    <b-card no-body>
        <b-tabs card>
            <b-tab
                :title="`${dependent.firstName} ${dependent.lastName}`"
                active
            >
                <div v-if="isExpired">
                    <b-row>
                        <b-col class="d-flex justify-content-center">
                            <h5>Your access has been expired</h5>
                        </b-col>
                    </b-row>
                    <b-row>
                        <b-col class="d-flex justify-content-center">
                            <p>
                                This dependent was removed from your profile due
                                to reaching the age of 19, for more information
                                visit the following page.
                            </p>
                        </b-col>
                    </b-row>
                    <b-row>
                        <b-col class="d-flex justify-content-center">
                            <b-button type="link">Dependent age limit</b-button>
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
                                        v-model="dependent.maskedPHN"
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
                                            formatDate(dependent.dateOfBirth)
                                        "
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
                                        v-model="dependent.gender"
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
                title="COVID-19"
                class="tableTab mt-2"
                @click="fetchLaboratoryResults()"
            >
                <b-row v-if="isLoading">
                    <b-col><b-spinner></b-spinner></b-col>
                </b-row>
                <b-row v-else-if="labResults.length == 0">
                    <b-col>No records found.</b-col>
                </b-row>
                <table v-else class="w-100">
                    <tr>
                        <th>Test Date</th>
                        <th class="d-none d-sm-table-cell">Test Type</th>
                        <th class="d-none d-sm-table-cell">Test Location</th>
                        <th>Result</th>
                        <th>Report</th>
                    </tr>
                    <tr v-for="item in labResults" :key="item.labResults[0].id">
                        <td>
                            {{
                                formatDate(item.labResults[0].collectedDateTime)
                            }}
                        </td>
                        <td class="d-none d-sm-table-cell">
                            {{ item.labResults[0].testType }}
                        </td>
                        <td class="d-none d-sm-table-cell">
                            {{ item.location }}
                        </td>
                        <td>{{ item.labResults[0].testStatus }}</td>
                        <td>
                            <b-btn variant="link" @click="getReport(item)">
                                <font-awesome-icon
                                    icon="file-download"
                                    aria-hidden="true"
                                    size="1x"
                                />
                            </b-btn>
                        </td>
                    </tr>
                </table>
            </b-tab>
        </b-tabs>
    </b-card>
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
</style>
