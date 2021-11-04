<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faCheckCircle, faSearch } from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/loading.vue";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
import type { WebClientConfiguration } from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper } from "@/models/dateWrapper";
import User from "@/models/user";
import SnowPlow from "@/utility/snowPlow";

library.add(faSearch, faCheckCircle);

@Component({
    components: {
        LoadingComponent,
        MessageModalComponent,
    },
})
export default class DashboardView extends Vue {
    @Action("retrieveAuthenticatedVaccineRecord", {
        namespace: "vaccinationStatus",
    })
    retrieveAuthenticatedVaccineRecord!: (params: {
        hdid: string;
    }) => Promise<CovidVaccineRecord>;

    @Getter("authenticatedVaccineRecordIsLoading", {
        namespace: "vaccinationStatus",
    })
    isLoadingVaccineRecord!: boolean;

    @Getter("authenticatedVaccineRecordStatusMessage", {
        namespace: "vaccinationStatus",
    })
    vaccineRecordStatusMessage!: string;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("authenticatedVaccineRecord", { namespace: "vaccinationStatus" })
    vaccineRecord!: CovidVaccineRecord | undefined;

    @Getter("authenticatedVaccineRecordResultMessage", {
        namespace: "vaccinationStatus",
    })
    vaccineRecordResultMessage!: string;

    @Ref("sensitivedocumentDownloadModal")
    readonly sensitivedocumentDownloadModal!: MessageModalComponent;

    @Ref("vaccineRecordResultModal")
    readonly vaccineRecordResultModal!: MessageModalComponent;

    private get isLoading(): boolean {
        return this.isLoadingVaccineRecord;
    }

    private get unverifiedEmail(): boolean {
        return !this.user.verifiedEmail && this.user.hasEmail;
    }

    private get unverifiedSMS(): boolean {
        return !this.user.verifiedSMS && this.user.hasSMS;
    }

    private get hasNewTermsOfService(): boolean {
        return this.user.hasTermsOfServiceUpdated;
    }

    private get isPacificTime(): boolean {
        const isDaylightSavings = new DateWrapper().isInDST();

        let timeZoneHourOffset = 8;
        if (isDaylightSavings) {
            timeZoneHourOffset = 7;
        }

        return new Date().getTimezoneOffset() / 60 === timeZoneHourOffset;
    }

    private retrieveVaccinePdf() {
        this.retrieveAuthenticatedVaccineRecord({
            hdid: this.user.hdid,
        });
    }

    @Watch("vaccineRecordResultMessage")
    private vaccineRecordErrorChanged() {
        if (this.vaccineRecordResultMessage.length > 0) {
            this.vaccineRecordResultModal.showModal();
        }
    }

    @Watch("vaccineRecord")
    private saveVaccinePdf() {
        if (this.vaccineRecord !== undefined) {
            const mimeType = this.vaccineRecord.document.mediaType;
            const downloadLink = `data:${mimeType};base64,${this.vaccineRecord.document.data}`;
            fetch(downloadLink).then((res) => {
                SnowPlow.trackEvent({
                    action: "click_button",
                    text: "FederalPVC",
                });
                res.blob().then((blob) => {
                    saveAs(blob, "VaccineProof.pdf");
                });
            });
        }
    }

    private showSensitiveDocumentDownloadModal() {
        this.sensitivedocumentDownloadModal.showModal();
    }

    private get showFederalCardButton(): boolean {
        return this.config.modules["FederalCardButton"];
    }

    private get cardColumnSize(): number {
        return this.showFederalCardButton ? 4 : 6;
    }

    private get loadingStatusMessage(): string {
        return this.vaccineRecordStatusMessage;
    }
}
</script>

<template>
    <div class="m-3 m-md-4 flex-grow-1 d-flex flex-column">
        <LoadingComponent
            :is-loading="isLoading"
            :text="loadingStatusMessage"
        />
        <b-alert
            v-if="hasNewTermsOfService"
            show
            dismissible
            variant="info"
            class="no-print"
        >
            <h4>Updated Terms of Service</h4>
            <span>
                The Terms of Service have been updated since your last login.
                You can review them
                <router-link
                    id="termsOfServiceLink"
                    variant="primary"
                    to="/termsOfService"
                >
                    here</router-link
                >.
            </span>
        </b-alert>
        <b-alert
            v-if="unverifiedEmail || unverifiedSMS"
            id="incomplete-profile-banner"
            show
            dismissible
            variant="info"
            class="no-print"
        >
            <h4>Verify Contact Information</h4>
            <span>
                Your email or cell phone number has not been verified. You can
                use the Health Gateway without verified contact information,
                however, you will not receive notifications. Visit the
                <router-link
                    id="profilePageLink"
                    data-testid="profile-page-link"
                    variant="primary"
                    to="/profile"
                    >Profile Page</router-link
                >
                to complete your verification.
            </span>
        </b-alert>
        <b-alert
            v-if="!isPacificTime"
            show
            dismissible
            variant="info"
            class="no-print"
        >
            <h4>Looks like you're in a different timezone.</h4>
            <span>
                Heads up: your health records are recorded and displayed in
                Pacific Time.
            </span>
        </b-alert>
        <page-title title="Dashboard" />
        <h2>What do you want to focus on today?</h2>
        <b-row>
            <b-col cols="12" :lg="cardColumnSize" class="p-3">
                <hg-card-button
                    title="BC Vaccine Card"
                    to="/covid19"
                    data-testid="bc-vaccine-card-btn"
                >
                    <template #icon>
                        <hg-icon
                            class="checkmark"
                            icon="check-circle"
                            size="extra-large"
                            square
                        />
                    </template>
                    <div>
                        View, download and print your BC Vaccine Card. Present
                        this card as proof of vaccination at some BC businesses,
                        services and events.
                    </div>
                </hg-card-button>
            </b-col>
            <b-col cols="12" :lg="cardColumnSize" class="p-3">
                <hg-card-button
                    title="Health Records"
                    to="/timeline"
                    data-testid="health-records-card-btn"
                >
                    <template #icon>
                        <img
                            class="health-gateway-logo"
                            src="@/assets/images/gov/health-gateway-logo.svg"
                            alt="Health Gateway Logo"
                        />
                    </template>
                    <div>
                        View and manage all your available health records,
                        including dispensed medications, health visits, COVID-19
                        test results, immunizations and more.
                    </div>
                </hg-card-button>
            </b-col>
            <b-col
                v-if="showFederalCardButton"
                cols="12"
                :lg="cardColumnSize"
                class="p-3"
            >
                <hg-card-button
                    title="Proof of Vaccination"
                    data-testid="proof-vaccination-card-btn"
                    @click="showSensitiveDocumentDownloadModal()"
                >
                    <template #icon>
                        <img
                            class="canada-government-logo"
                            src="@/assets/images/gov/canada-gov-logo.svg"
                            alt="Canada Government Logo"
                        />
                    </template>
                    <div>
                        Download and print your Federal Proof of Vaccination for
                        domestic and international travel.
                    </div>
                </hg-card-button>
            </b-col>
        </b-row>
        <MessageModalComponent
            ref="sensitivedocumentDownloadModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="retrieveVaccinePdf"
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

.health-gateway-logo {
    height: 2em;
    width: 2em;
}

.canada-government-logo {
    height: 1.5em;
}

.checkmark {
    color: $hg-state-success;
}
</style>
<style lang="scss">
@import "@/assets/scss/_variables.scss";

.vld-overlay {
    .vld-background {
        opacity: 0.75;
    }

    .vld-icon {
        text-align: center;
    }
}
</style>
