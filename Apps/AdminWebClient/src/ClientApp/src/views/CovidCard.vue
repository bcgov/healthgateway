<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faClipboardList,
    faEye,
    faEyeSlash,
} from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import { ValidationObserver } from "vee-validate";
import { Component, Vue } from "vue-property-decorator";
import { Getter } from "vuex-class";

import AddressComponent from "@/components/core/Address.vue";
import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import LoadingComponent from "@/components/core/Loading.vue";
import CovidTreatmentAssessmentComponent from "@/components/covidTreatmentAssessment/CovidTreatmentAssessment.vue";
import { Countries } from "@/constants/countries";
import { Feature } from "@/constants/feature";
import { FeedbackType } from "@/constants/feedbacktype";
import { Provinces } from "@/constants/provinces";
import { SnackbarPosition } from "@/constants/snackbarPosition";
import type Address from "@/models/address";
import type BannerFeedback from "@/models/bannerFeedback";
import type CovidCardPatientResult from "@/models/covidCardPatientResult";
import CovidTreatmentAssessmentDetails from "@/models/covidTreatmentAssessmentDetails";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ICovidSupportService } from "@/services/interfaces";
import { Mask, phnMaskTemplate } from "@/utility/masks";
import PHNValidator from "@/utility/phnValidator";
import SnowPlow from "@/utility/snowPlow";

library.add(faEye, faEyeSlash, faClipboardList);

interface ImmunizationRow {
    date: string;
    product: string;
    lotNumber: string;
    clinic: string;
}

interface AssessmentHistoryRow {
    dateTimeOfAssessment: string;
    formId: string;
}

const emptyAddress: Address = {
    streetLines: [],
    city: "",
    state: "",
    postalCode: "",
    country: "",
};

@Component({
    components: {
        LoadingComponent,
        BannerFeedbackComponent,
        CovidTreatmentAssessmentComponent,
        AddressComponent,
        ValidationObserver,
    },
})
export default class CovidCardView extends Vue {
    @Getter("features", { namespace: "config" })
    private features!: { [id: string]: boolean };

    private isEditMode = false;
    private isLoading = false;
    private showFeedback = false;
    private showCovidTreatmentAssessment = false;

    private phn = "";
    private activePhn = "";
    private address: Address = { ...emptyAddress };
    private immunizations: ImmunizationRow[] = [];
    private searchResult: CovidCardPatientResult | null = null;
    private assessmentDetails: CovidTreatmentAssessmentDetails | null = null;
    private assessmentHistory: AssessmentHistoryRow[] = [];
    private maskHdid = true;
    private covidSupportService!: ICovidSupportService;

    private bannerFeedback: BannerFeedback = {
        type: FeedbackType.NONE,
        title: "",
        message: "",
    };

    private tableHeaders = [
        { text: "Date", value: "date", width: "10em" },
        { text: "Product", value: "product" },
        { text: "Lot Number", value: "lotNumber" },
        { text: "Clinic", value: "clinic" },
    ];

    private assessmentHistoryTableHeaders = [
        { text: "Date", value: "dateTimeOfAssessment" },
        { text: "ID", value: "formId", sortable: false },
    ];

    private get covid19TreatmentAssessmentEnabled(): boolean {
        return this.features[Feature.Covid19TreatmentAssessment] === true;
    }

    private get patientName(): string {
        return `${this.searchResult?.patient?.firstname} ${this.searchResult?.patient?.lastname}`;
    }

    private get patientHdid(): string {
        return this.searchResult?.patient?.hdid ?? "";
    }

    private get containsInvalidDoses(): boolean {
        return this.searchResult?.vaccineDetails?.containsInvalidDoses === true;
    }

    private get immunizationsAreBlocked(): boolean {
        return this.searchResult?.blocked === true;
    }

    private get snackbarPositionBottom(): string {
        return SnackbarPosition.Bottom;
    }

    private get birthDate(): string {
        if (this.searchResult === null) {
            return "";
        }
        return this.formatDate(this.searchResult.patient.birthdate);
    }

    private get phnMask(): Mask {
        return phnMaskTemplate;
    }

    private mounted() {
        this.covidSupportService = container.get(
            SERVICE_IDENTIFIER.CovidSupportService
        );
    }

    private clear(emptySearchField = true) {
        this.searchResult = null;
        this.isEditMode = false;
        this.immunizations = [];
        this.address = { ...emptyAddress };

        if (emptySearchField) {
            this.phn = "";
            this.activePhn = "";
        }
    }

    private handleRefresh() {
        if (this.activePhn) {
            this.clear(false);
            this.search(true);
        }
    }

    private handleSearch() {
        this.clear(false);

        const phnDigits = this.phn.replace(/[^0-9]/g, "");
        if (!PHNValidator.IsValid(phnDigits)) {
            this.showBannerFeedback({
                type: FeedbackType.Error,
                title: "Validation error",
                message: "Invalid PHN",
            });
            return;
        }
        this.activePhn = phnDigits;
        this.search(false);
    }

    private search(refresh: boolean) {
        this.isLoading = true;

        let assessmentDetailsPromise = Promise.resolve();
        if (this.covid19TreatmentAssessmentEnabled) {
            assessmentDetailsPromise =
                this.getCovidTreatmentAssessmentDetails();
        }

        Promise.all([
            this.covidSupportService.getPatient(this.activePhn, refresh),
            assessmentDetailsPromise,
        ])
            .then(([searchResult]) => {
                this.searchResult = searchResult;
                this.setAddress(
                    this.searchResult?.patient?.postalAddress,
                    this.searchResult?.patient?.physicalAddress
                );
                this.phn = this.searchResult?.patient?.personalhealthnumber;
                this.maskHdid = true;
                if (!this.immunizationsAreBlocked) {
                    this.immunizations =
                        this.searchResult.vaccineDetails?.doses?.map((dose) => {
                            return {
                                date: dose.date,
                                clinic: dose.location,
                                product: dose.product,
                                lotNumber: dose.lot,
                            };
                        }) ?? [];
                    this.immunizations.sort((a, b) => {
                        const firstDate = new DateWrapper(a.date);
                        const secondDate = new DateWrapper(b.date);

                        // Sort dates in descending order
                        if (firstDate.isBefore(secondDate)) {
                            return 1;
                        }
                        return firstDate.isAfter(secondDate) ? -1 : 0;
                    });
                }
                if (this.searchResult.patient.responseCode) {
                    const message =
                        this.searchResult.patient.responseCode.split("|")[1];
                    this.showBannerFeedback({
                        type: FeedbackType.Info,
                        title: "Info",
                        message: message,
                    });
                }
            })
            .catch(() => {
                this.searchResult = null;
                this.showBannerFeedback({
                    type: FeedbackType.Error,
                    title: "Search Error",
                    message: "Unknown error searching patient data",
                });
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private async getCovidTreatmentAssessmentDetails(): Promise<void> {
        const assessmentDetails =
            await this.covidSupportService.getCovidTreatmentAssessmentDetails(
                this.activePhn
            );

        this.assessmentDetails = assessmentDetails;
        this.assessmentHistory =
            this.assessmentDetails.previousAssessmentDetailsList?.map(
                (entry) => {
                    const date = new DateWrapper(entry.dateTimeOfAssessment, {
                        hasTime: true,
                        isUtc: true,
                    });
                    return {
                        dateTimeOfAssessment: date.format("yyyy-MMM-dd h:mm a"),
                        formId: entry.formId,
                    };
                }
            ) ?? [];
    }

    private setAddress(
        defaultAddress: Address | null,
        backupAddress: Address | null
    ) {
        let address = { ...emptyAddress };
        if (defaultAddress) {
            address = { ...defaultAddress };
        } else if (backupAddress) {
            address = { ...backupAddress };
        }

        // select Canada if the address has a province but no country
        if (address.country === "" && Provinces[address.state]) {
            address.country = "CA";
        }

        this.address = address;
    }

    private async handleMail() {
        if (this.$refs.observer !== undefined) {
            const isValid = await (
                this.$refs.observer as Vue & { validate: () => boolean }
            ).validate();
            if (isValid) {
                SnowPlow.trackEvent({
                    action: "click_button",
                    text: "mail proof",
                });

                // retrieve the country name for the given country code
                // unless the country is Canada, in which case it should not be displayed
                let displayedCountryName = "";
                if (
                    this.address.country !== "CA" &&
                    Countries[this.address.country]
                ) {
                    displayedCountryName = Countries[this.address.country][0];
                }

                this.isLoading = true;
                this.covidSupportService
                    .mailDocument({
                        personalHealthNumber:
                            this.searchResult?.patient.personalhealthnumber ??
                            "",
                        mailAddress: {
                            ...this.address,
                            country: displayedCountryName,
                        },
                    })
                    .then((mailResult) => {
                        if (mailResult) {
                            this.searchResult = null;
                            this.showBannerFeedback({
                                type: FeedbackType.Success,
                                title: "Success",
                                message: "BC Vaccine Card mailed successfully.",
                            });
                        } else {
                            this.showBannerFeedback({
                                type: FeedbackType.Error,
                                title: "Error",
                                message:
                                    "Something went wrong when mailing the card, please try again later.",
                            });
                        }
                    })
                    .catch(() => {
                        this.showBannerFeedback({
                            type: FeedbackType.Error,
                            title: "Mail Error",
                            message: "Unknown error mailing report",
                        });
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            } else {
                console.log("Error validation");
            }
        }
    }

    private handlePrint() {
        SnowPlow.trackEvent({
            action: "click_button",
            text: "print proof",
        });
        this.isLoading = true;
        this.covidSupportService
            .retrieveDocument(
                this.searchResult?.patient.personalhealthnumber ?? ""
            )
            .then((documentResult) => {
                if (documentResult) {
                    const downloadLink = `data:application/pdf;base64,${documentResult.data}`;
                    fetch(downloadLink).then((res) => {
                        res.blob().then((blob) => {
                            saveAs(blob, documentResult.fileName);
                        });
                    });
                }
            })
            .catch(() => {
                this.showBannerFeedback({
                    type: FeedbackType.Error,
                    title: "Download Error",
                    message: "Unknown error downloading report",
                });
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private onEditModeChange(): void {
        if (!this.isEditMode) {
            this.setAddress(
                this.searchResult?.patient?.postalAddress ?? null,
                this.searchResult?.patient?.physicalAddress ?? null
            );
        }
    }

    private formatDate(date: StringISODate): string {
        if (!date) {
            return "";
        }
        return new DateWrapper(date).format(DateWrapper.defaultFormat);
    }

    private startCovidTreatmentAssessment(): void {
        this.showCovidTreatmentAssessment = true;
        window.scrollTo(0, 0);
    }

    private covidTreatmentAssessmentCancelled(): void {
        this.showCovidTreatmentAssessment = false;
    }

    private covidTreatmentAssessmentSubmitted(): void {
        this.isLoading = true;
    }

    private covidTreatmentAssessmentSubmissionSucceeded(): void {
        this.showCovidTreatmentAssessment = false;

        this.getCovidTreatmentAssessmentDetails()
            .then(() => {
                this.showBannerFeedback({
                    type: FeedbackType.Success,
                    title: "Success",
                    message:
                        "COVID‑19 treatment assessment submitted successfully.",
                });
            })
            .catch(() => {
                this.showBannerFeedback({
                    type: FeedbackType.Warning,
                    title: "Warning",
                    message:
                        "COVID‑19 treatment assessment submitted successfully, but updated assessment history could not be retrieved.",
                });
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private covidTreatmentAssessmentSubmissionFailed(): void {
        this.showBannerFeedback({
            type: FeedbackType.Error,
            title: "Error",
            message: "Unable to submit COVID‑19 treatment assessment.",
        });
        this.isLoading = false;
    }

    private showBannerFeedback(bannerFeedback: BannerFeedback): void {
        this.showFeedback = true;
        this.bannerFeedback = bannerFeedback;
    }
}
</script>

<template>
    <v-container>
        <LoadingComponent :is-loading="isLoading" />
        <BannerFeedbackComponent
            :show-feedback.sync="showFeedback"
            :feedback="bannerFeedback"
            :position="snackbarPositionBottom"
        />
        <CovidTreatmentAssessmentComponent
            v-if="
                covid19TreatmentAssessmentEnabled &&
                showCovidTreatmentAssessment
            "
            :details="assessmentDetails"
            :patient="searchResult.patient"
            :default-address="address"
            @on-cancel="covidTreatmentAssessmentCancelled"
            @on-submit="covidTreatmentAssessmentSubmitted"
            @on-submit-success="covidTreatmentAssessmentSubmissionSucceeded"
            @on-submit-failure="covidTreatmentAssessmentSubmissionFailed"
        />
        <v-row v-else no-gutters>
            <v-col cols="12" sm="12" md="10" offset-md="1">
                <form @submit.prevent="handleSearch()">
                    <v-row align="center" dense>
                        <v-col>
                            <v-text-field
                                v-model="phn"
                                v-mask="phnMask"
                                label="PHN"
                            />
                        </v-col>
                        <v-col cols="auto">
                            <v-btn type="submit" class="mt-2 primary">
                                <span>Search</span>
                                <v-icon class="ml-2" size="sm">
                                    fas fa-search
                                </v-icon>
                            </v-btn>
                        </v-col>
                        <v-col cols="auto">
                            <v-btn
                                type="reset"
                                class="mt-2 secondary"
                                @click="clear"
                            >
                                <span>Clear</span>
                                <v-icon class="ml-2" size="sm">
                                    mdi-backspace
                                </v-icon>
                            </v-btn>
                        </v-col>
                    </v-row>
                </form>
                <v-row
                    v-if="searchResult != null && searchResult.patient == null"
                    align="center"
                >
                    <v-col>
                        <h2>No records found.</h2>
                    </v-col>
                </v-row>
                <div
                    v-else-if="
                        searchResult != null && searchResult.patient != null
                    "
                >
                    <v-row>
                        <v-col>
                            <h2>Patient Information</h2>
                        </v-col>
                    </v-row>
                    <v-row align="center" dense>
                        <v-col>
                            <v-text-field
                                v-model="patientName"
                                readonly
                                label="Name"
                            />
                        </v-col>
                        <v-col>
                            <v-text-field
                                v-model="birthDate"
                                readonly
                                label="Birthdate"
                            />
                        </v-col>
                        <v-col>
                            <v-text-field
                                v-model="
                                    searchResult.patient.personalhealthnumber
                                "
                                readonly
                                label="PHN"
                            />
                        </v-col>
                    </v-row>
                    <v-row align="center" dense>
                        <v-col>
                            <v-text-field
                                v-if="patientHdid.length > 0"
                                :value="maskHdid ? '        ' : patientHdid"
                                :type="maskHdid ? 'password' : 'text'"
                                :append-outer-icon="
                                    maskHdid ? 'fa-eye-slash' : 'fa-eye'
                                "
                                readonly
                                dense
                                label="HDID"
                                @click:append-outer="maskHdid = !maskHdid"
                            />
                            <v-text-field
                                v-else
                                disabled
                                dense
                                label="No HDID"
                            />
                        </v-col>
                    </v-row>
                    <v-row align="center" dense>
                        <v-col cols="auto">
                            <h2>COVID‑19 Immunizations</h2>
                        </v-col>
                        <v-col class="text-right">
                            <v-btn
                                type="button"
                                class="mt-2 secondary"
                                @click="handleRefresh()"
                            >
                                <span>Refresh</span>
                                <v-icon class="ml-2" size="sm"
                                    >fas fa-sync</v-icon
                                >
                            </v-btn>
                        </v-col>
                    </v-row>
                    <v-row
                        v-if="immunizationsAreBlocked"
                        class="mt-2"
                        align="center"
                        dense
                    >
                        <v-col lg="6" offset-lg="3" xl="4" offset-xl="4">
                            <v-alert
                                dense
                                outlined
                                type="error"
                                class="alert-background-color"
                            >
                                Unable to retrieve vaccine records for this
                                individual.
                            </v-alert>
                        </v-col>
                    </v-row>
                    <v-row v-if="!immunizationsAreBlocked" dense>
                        <v-col no-gutters>
                            <v-data-table
                                :headers="tableHeaders"
                                :items="immunizations"
                                :items-per-page="5"
                                :hide-default-footer="true"
                            >
                                <template #[`item.date`]="{ item }">
                                    <span>{{ formatDate(item.date) }}</span>
                                </template>
                            </v-data-table>
                            <v-alert
                                v-if="containsInvalidDoses"
                                dense
                                color="orange"
                                type="warning"
                                class="mt-4"
                            >
                                This record has invalid doses.
                            </v-alert>
                        </v-col>
                    </v-row>
                    <v-row v-if="!immunizationsAreBlocked" justify="end">
                        <v-col class="text-right">
                            <v-btn
                                type="button"
                                class="primary"
                                :disabled="immunizations.length === 0"
                                @click="handlePrint()"
                            >
                                <span>Print</span>
                                <v-icon class="ml-2" size="sm"
                                    >fas fa-print</v-icon
                                >
                            </v-btn>
                        </v-col>
                    </v-row>
                    <ValidationObserver ref="observer">
                        <v-form
                            v-if="!immunizationsAreBlocked"
                            ref="form"
                            lazy-validation
                            autocomplete="off"
                            @submit.prevent="handleMail()"
                        >
                            <v-row>
                                <v-col>
                                    <h2>Mailing Address</h2>
                                </v-col>
                            </v-row>
                            <AddressComponent
                                v-if="!immunizationsAreBlocked"
                                v-bind.sync="address"
                                :is-disabled="!isEditMode"
                            />
                            <v-row align="center" dense>
                                <v-col cols="auto">
                                    <v-checkbox
                                        v-model="isEditMode"
                                        label="Mail to a different address"
                                        @change="onEditModeChange"
                                    />
                                </v-col>
                                <v-col class="text-right">
                                    <v-btn
                                        type="submit"
                                        class="mx-2 primary"
                                        :disabled="immunizations.length === 0"
                                    >
                                        <span>Mail</span>
                                        <v-icon class="ml-2" size="sm">
                                            fas fa-paper-plane
                                        </v-icon>
                                    </v-btn>
                                </v-col>
                            </v-row>
                        </v-form>
                    </ValidationObserver>
                    <v-row v-if="covid19TreatmentAssessmentEnabled" dense>
                        <v-col cols="12" class="text-right">
                            <v-btn
                                type="submit"
                                class="mx-2 success"
                                @click="startCovidTreatmentAssessment"
                            >
                                <span>Start COVID‑19 Treatment Assessment</span>
                                <v-icon class="ml-2" size="sm">
                                    fas fa-clipboard-list
                                </v-icon>
                            </v-btn>
                        </v-col>
                        <v-col cols="12">
                            <h2>Assessment History</h2>
                        </v-col>
                        <v-col cols="12" no-gutters>
                            <v-data-table
                                :headers="assessmentHistoryTableHeaders"
                                :items="assessmentHistory"
                                :items-per-page="5"
                                :hide-default-footer="false"
                                sort-by="dateTimeOfAssessment"
                                sort-desc
                                must-sort
                            />
                        </v-col>
                    </v-row>
                </div>
            </v-col>
        </v-row>
    </v-container>
</template>

<style lang="scss">
.alert-background-color {
    background-color: white !important;
    align-content: center !important;
}
</style>
