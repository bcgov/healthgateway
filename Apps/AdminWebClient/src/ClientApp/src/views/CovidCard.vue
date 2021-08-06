<script lang="ts">
import { saveAs } from "file-saver";
import { Component, Vue, Watch } from "vue-property-decorator";

import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import LoadingComponent from "@/components/core/Loading.vue";
import { CountryList } from "@/constants/countryList";
import { ProvinceList } from "@/constants/provinceList";
import { ResultType } from "@/constants/resulttype";
import { StateList } from "@/constants/stateList";
import { Address } from "@/models/address";
import BannerFeedback from "@/models/bannerFeedback";
import CovidCardRequestResult from "@/models/covidCardRequestResult";
import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ICovidSupportService } from "@/services/interfaces";
import PHNValidator from "@/utility/phnValidator";

interface ImmunizationRow {
    date: string;
    product: string;
    lotNumber: string;
    clinic: string;
}

@Component({
    components: {
        LoadingComponent,
        BannerFeedbackComponent,
    },
})
export default class CovidCardView extends Vue {
    private isEditMode = false;
    private isLoading = false;
    private showFeedback = false;
    private phn = "";
    private address: Address;
    private immunizations: ImmunizationRow[] = [];
    private searchResult?: CovidCardRequestResult = null;
    private covidSupportService!: ICovidSupportService;
    private selectedCountry = "";
    private bannerFeedback: BannerFeedback = {
        type: ResultType.NONE,
        title: "",
        message: "",
    };
    private tableHeaders = [
        {
            text: "Date",
            value: "date",
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

    private get countryList() {
        return Object.keys(CountryList).map((countryCode) => {
            return { text: CountryList[countryCode], value: countryCode };
        });
    }

    @Watch("selectedCountry")
    private onCountryChanged() {
        this.address.country = this.selectedCountry;
    }

    private get patientName() {
        return `${this.searchResult?.patientInfo.firstname} ${this.searchResult?.patientInfo.lastname}`;
    }

    private get provinceStateList() {
        if (this.selectedCountry == "CA") {
            return Object.keys(ProvinceList).map((provinceCode) => {
                return {
                    text: ProvinceList[provinceCode],
                    value: provinceCode,
                };
            });
        } else if (this.selectedCountry == "US") {
            return StateList;
        } else {
            return [];
        }
    }

    private mounted() {
        this.covidSupportService = container.get(
            SERVICE_IDENTIFIER.CovidSupportService
        );
    }

    private handleSearch() {
        this.searchResult = null;
        this.isEditMode = false;
        this.immunizations = [];
        this.address = null;

        if (!PHNValidator.IsValid(this.phn)) {
            this.showFeedback = true;
            this.bannerFeedback = {
                type: ResultType.Error,
                title: "Validation error",
                message: "Invalid PHN",
            };
            return;
        }

        this.isLoading = true;

        this.covidSupportService
            .searchByPHN(this.phn)
            .then((result) => {
                this.phn = null;
                this.searchResult = result;
                this.address = this.searchResult?.patientInfo?.address;
                this.selectedCountry = this.address?.country;
                this.immunizations = this.searchResult.immunizations?.map(
                    (immz) => {
                        return {
                            date: immz.dateOfImmunization,
                            clinic: immz.providerOrClinic,
                            product:
                                immz.immunization.immunizationAgents[0]
                                    .productName,
                            lotNumber:
                                immz.immunization.immunizationAgents[0]
                                    .lotNumber,
                        };
                    }
                );
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

    private handleSubmit() {
        this.isLoading = true;
        this.covidSupportService
            .mailDocument({
                personalHealthNumber: this.phn,
                address: this.address,
            })
            .then((mailResult) => {
                if (mailResult) {
                    this.searchResult = null;
                    this.showFeedback = true;
                    this.bannerFeedback = {
                        type: ResultType.Success,
                        title: "Success",
                        message: "Covid card mailed successfully.",
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
        this.isLoading = true;
        this.covidSupportService
            .retrieveDocument(this.phn)
            .then((documentResult) => {
                if (documentResult) {
                    const downloadLink = `data:application/pdf;base64,${documentResult.document}`;
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
        if (this.isEditMode) {
            this.selectedCountry = "CA";
            this.address = {};
        } else {
            this.address = this.searchResult?.patientInfo?.address;
        }
    }

    private formatDateTime(date: StringISODateTime): string {
        if (!date) {
            return "";
        }
        return new DateWrapper(date, { isUtc: true }).format(
            DateWrapper.defaultDateTimeFormat
        );
    }
}
</script>

<template>
    <v-container>
        <LoadingComponent :is-loading="isLoading" />
        <BannerFeedbackComponent
            :show-feedback.sync="showFeedback"
            :feedback="bannerFeedback"
        />
        <form @submit.prevent="handleSearch()">
            <v-row align="center">
                <v-col cols="6">
                    <v-text-field v-model="phn" label="PHN" maxlength="10" />
                </v-col>
                <v-col cols="auto">
                    <v-btn type="submit" class="mt-2 primary">
                        Search
                        <v-icon class="ml-2" size="sm">fas fa-search</v-icon>
                    </v-btn>
                </v-col>
            </v-row>
        </form>
        <v-row
            v-if="searchResult != null && searchResult.patientInfo == null"
            align="center"
        >
            <v-col>
                <h2>No records found.</h2>
            </v-col>
        </v-row>
        <v-form
            v-if="searchResult != null && searchResult.patientInfo != null"
            autocomplete="false"
            @submit.prevent="handleSubmit()"
        >
            <v-row>
                <v-col>
                    <h2>Patient Information</h2>
                </v-col>
            </v-row>
            <v-row align="center">
                <v-col>
                    <v-text-field v-model="patientName" disabled label="Name" />
                </v-col>
                <v-col>
                    <v-text-field
                        v-model="searchResult.patientInfo.birthdate"
                        disabled
                        label="Birthdate"
                    />
                </v-col>
                <v-col>
                    <v-text-field
                        v-model="searchResult.patientInfo.personalhealthnumber"
                        disabled
                        label="PHN"
                    />
                </v-col>
            </v-row>
            <v-row>
                <v-col>
                    <h2>Mailing Address</h2>
                </v-col>
            </v-row>
            <v-row align="center">
                <v-col>
                    <v-text-field
                        v-model="address.address"
                        label="Address"
                        :disabled="!isEditMode"
                        autocomplete="chrome-off"
                    />
                </v-col>
            </v-row>
            <v-row align="center">
                <v-col>
                    <v-text-field
                        v-model="address.address2"
                        label="Address Line 2"
                        :disabled="!isEditMode"
                        autocomplete="chrome-off"
                    />
                </v-col>
            </v-row>
            <v-row align="center">
                <v-col>
                    <v-text-field
                        v-model="address.city"
                        label="City"
                        :disabled="!isEditMode"
                        autocomplete="chrome-off"
                    />
                </v-col>
                <v-col v-if="provinceStateList.length > 0">
                    <v-select
                        v-model="address.province"
                        :items="provinceStateList"
                        label="Province/State"
                        :disabled="!isEditMode"
                        autocomplete="chrome-off"
                    />
                </v-col>
                <v-col v-else>
                    <v-text-field
                        v-model="address.province"
                        label="Province/State"
                        :disabled="!isEditMode"
                        autocomplete="chrome-off"
                    />
                </v-col>
                <v-col>
                    <v-select
                        v-model="selectedCountry"
                        :items="countryList"
                        label="Country"
                        :disabled="!isEditMode"
                        autocomplete="chrome-off"
                    />
                </v-col>
                <v-col>
                    <v-text-field
                        v-model="address.postalCode"
                        label="Postal Code"
                        :disabled="!isEditMode"
                        autocomplete="chrome-off"
                    />
                </v-col>
            </v-row>
            <v-row>
                <v-col>
                    <v-checkbox
                        v-model="isEditMode"
                        label="Mail to a different address"
                        @change="onEditModeChange"
                    />
                </v-col>
            </v-row>
            <v-row>
                <v-col>
                    <h2>Covid Immunizations</h2>
                </v-col>
            </v-row>
            <v-row>
                <v-col no-gutters>
                    <v-data-table
                        :headers="tableHeaders"
                        :items="immunizations"
                        :items-per-page="5"
                        :hide-default-footer="true"
                    >
                        <template #:item.date="{ item }">
                            <span>{{ formatDateTime(item.date) }}</span>
                        </template>
                    </v-data-table>
                </v-col>
            </v-row>
            <v-row justify="end" class="mt-5">
                <v-btn type="button" class="secondary" @click="handlePrint()">
                    Print
                    <v-icon class="ml-2" size="sm">fas fa-print</v-icon>
                </v-btn>
                <v-btn type="submit" class="mx-2 primary">
                    Mail
                    <v-icon class="ml-2" size="sm">fas fa-paper-plane</v-icon>
                </v-btn>
            </v-row>
        </v-form>
    </v-container>
</template>
