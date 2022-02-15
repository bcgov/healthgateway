<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faClipboardList,
    faEye,
    faEyeSlash,
} from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import { Component, Vue } from "vue-property-decorator";

import AddressComponent from "@/components/core/Address.vue";
import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import LoadingComponent from "@/components/core/Loading.vue";
import CovidTreatmentAssessmentComponent from "@/components/covidTreatmentAssessment/CovidTreatmentAssessment.vue";
import { Countries } from "@/constants/countries";
import { Provinces } from "@/constants/provinces";
import { ResultType } from "@/constants/resulttype";
import { SnackbarPosition } from "@/constants/snackbarPosition";
import type Address from "@/models/address";
import type BannerFeedback from "@/models/bannerFeedback";
import type CovidCardPatientResult from "@/models/covidCardPatientResult";
import CovidTreatmentAssessmentDetails from "@/models/CovidTreatmentAssessmentDetails";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import PreviousAssessmentDetailsList from "@/models/previousAssessmentDetailsList";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ICovidSupportService } from "@/services/interfaces";
import { Mask, phnMask } from "@/utility/masks";
import PHNValidator from "@/utility/phnValidator";
import SnowPlow from "@/utility/snowPlow";

library.add(faEye, faEyeSlash, faClipboardList);

interface ImmunizationRow {
    date: string;
    product: string;
    lotNumber: string;
    clinic: string;
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
    },
})
export default class CovidCardView extends Vue {
    private isEditMode = false;
    private isLoading = false;
    private showFeedback = false;
    private showCovidTreatmentAssessment = false;

    private phn = "";
    private activePhn = "";
    private address: Address = { ...emptyAddress };
    private immunizations: ImmunizationRow[] = [];
    private searchResult: CovidCardPatientResult | null = null;
    private maskHdid = true;
    private covidSupportService!: ICovidSupportService;

    private bannerFeedback: BannerFeedback = {
        type: ResultType.NONE,
        title: "",
        message: "",
    };

    private tableHeaders = [
        {
            text: "Date",
            value: "date",
            width: "10em",
        },
        {
            text: "Product",
            value: "product",
        },
        {
            text: "Lot Number",
            value: "lotNumber",
        },
        {
            text: "Clinic",
            value: "clinic",
        },
    ];

    private assessmentHistoryTableHeaders = [
        {
            text: "Date",
            value: "dateOfAssessment",
        },
        {
            text: "Time",
            value: "timeOfAssessment",
        },
        {
            text: "ID",
            value: "formId",
        },
    ];

    private get patientName(): string {
        return `${this.searchResult?.patient?.firstname} ${this.searchResult?.patient?.lastname}`;
    }

    private get patientHdid(): string {
        return this.searchResult?.patient?.hdid ?? "";
    }

    private get containsInvalidDoses(): boolean {
        return this.searchResult?.vaccineDetails?.containsInvalidDoses === true;
    }

    private get snackbarPosition(): string {
        return SnackbarPosition.Bottom;
    }

    private get birthDate(): string {
        if (this.searchResult === null) {
            return "";
        }
        return this.formatDate(this.searchResult.patient.birthdate);
    }

    private get phnMask(): Mask {
        return phnMask;
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
            this.search(this.activePhn, true);
        }
    }

    private handleSearch() {
        this.clear(false);

        const phnDigits = this.phn.replace(/[^0-9]/g, "");
        if (!PHNValidator.IsValid(phnDigits)) {
            this.showFeedback = true;
            this.bannerFeedback = {
                type: ResultType.Error,
                title: "Validation error",
                message: "Invalid PHN",
            };
            return;
        }
        this.activePhn = phnDigits;
        this.search(phnDigits, false);
    }

    private search(personalHealthNumber: string, refresh: boolean) {
        this.isLoading = true;

        this.covidSupportService
            .getPatient(personalHealthNumber, refresh)
            .then((result) => {
                if (result.blocked) {
                    this.searchResult = null;
                    this.showFeedback = true;
                    this.bannerFeedback = {
                        type: ResultType.Error,
                        title: "Search Error",
                        message:
                            "Unable to retrieve record for this individual",
                    };
                } else {
                    this.phn = "";
                    this.searchResult = result;
                    this.maskHdid = true;
                    this.setAddress(
                        this.searchResult?.patient?.postalAddress,
                        this.searchResult?.patient?.physicalAddress
                    );
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
            })
            .catch(() => {
                this.searchResult = null;
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Search Error",
                    message: "Unknown error searching patient data",
                };
            })
            .finally(() => {
                this.isLoading = false;
            });
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

    private handleSubmit() {
        SnowPlow.trackEvent({
            action: "click_button",
            text: "mail proof",
        });

        // retrieve the country name for the given country code
        // unless the country is Canada, in which case it should not be displayed
        let displayedCountryName = "";
        if (this.address.country !== "CA" && Countries[this.address.country]) {
            displayedCountryName = Countries[this.address.country][0];
        }

        this.isLoading = true;
        this.covidSupportService
            .mailDocument({
                personalHealthNumber:
                    this.searchResult?.patient.personalhealthnumber ?? "",
                mailAddress: {
                    ...this.address,
                    country: displayedCountryName,
                },
            })
            .then((mailResult) => {
                if (mailResult) {
                    this.searchResult = null;
                    this.showFeedback = true;
                    this.bannerFeedback = {
                        type: ResultType.Success,
                        title: "Success",
                        message: "BC Vaccine Card mailed successfully.",
                    };
                } else {
                    this.showFeedback = true;
                    this.bannerFeedback = {
                        type: ResultType.Error,
                        title: "Error",
                        message:
                            "Something went wrong when mailing the card, please try again later.",
                    };
                }
            })
            .catch(() => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Mail Error",
                    message: "Unknown error mailing report",
                };
            })
            .finally(() => {
                this.isLoading = false;
            });
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
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Download Error",
                    message: "Unknown error downloading report",
                };
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private onEditModeChange() {
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
    }

    private covidTreatmentAssessmentCancelled(): void {
        this.showCovidTreatmentAssessment = false;
    }

    private covidTreatmentAssessmentSubmitted(): void {
        this.showCovidTreatmentAssessment = false;
    }

    private previousAssessmentDetailsList: PreviousAssessmentDetailsList[] = [
        {
            dateOfAssessment: "2021-01-01",
            timeOfAssessment: "10:00 AM",
            formId: "123456",
        },
    ];

    private covidTreatmentAssessmentDetails: CovidTreatmentAssessmentDetails = {
        hasKnownPositiveC19Past7Days: false,
        citizenIsConsideredImmunoCompromised: false,
        has3DoseMoreThan14Days: false,
        hasDocumentedChronicCondition: false,
        previousAssessmentDetailsList: this.previousAssessmentDetailsList,
    };
}
</script>

<template>
    <v-container>
        <LoadingComponent :is-loading="isLoading" />
        <BannerFeedbackComponent
            :show-feedback.sync="showFeedback"
            :feedback="bannerFeedback"
            :position="snackbarPosition"
        />
        <CovidTreatmentAssessmentComponent
            v-if="showCovidTreatmentAssessment"
            @on-cancel="covidTreatmentAssessmentCancelled"
            @on-submit="covidTreatmentAssessmentSubmitted"
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
                <v-form
                    v-if="searchResult != null && searchResult.patient != null"
                    autocomplete="false"
                    @submit.prevent="handleSubmit()"
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
                            <h2>COVID-19 Immunizations</h2>
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
                    <v-row dense>
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
                    <v-row justify="end">
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
                    <v-row>
                        <v-col>
                            <h2>Mailing Address</h2>
                        </v-col>
                    </v-row>
                    <AddressComponent
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
                    <v-row dense>
                        <v-col class="text-right">
                            <v-btn
                                type="submit"
                                class="mx-2 success"
                                @click="startCovidTreatmentAssessment"
                            >
                                <span>Start COVID-19 Treatment Assessment</span>
                                <v-icon class="ml-2" size="sm">
                                    fas fa-clipboard-list
                                </v-icon>
                            </v-btn>
                        </v-col>
                    </v-row>
                    <v-row dense>
                        <v-col cols="auto">
                            <h2>Assessment History</h2>
                        </v-col>
                    </v-row>
                    <v-row dense>
                        <v-col no-gutters>
                            <v-data-table
                                :headers="assessmentHistoryTableHeaders"
                                :items="previousAssessmentDetailsList"
                                :items-per-page="5"
                                :hide-default-footer="true"
                            >
                            </v-data-table>
                        </v-col>
                    </v-row>
                </v-form>
            </v-col>
        </v-row>
    </v-container>
</template>
