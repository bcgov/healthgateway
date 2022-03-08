<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faClipboard,
    faEdit,
    faFlask,
    faPills,
    faPlus,
    faSearch,
    faSyringe,
    faUserMd,
    faVial,
} from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/loading.vue";
import AddQuickLinkComponent from "@/components/modal/addQuickLink.vue";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import type { WebClientConfiguration } from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper } from "@/models/dateWrapper";
import { QuickLink } from "@/models/quickLink";
import { TimelineFilterBuilder } from "@/models/timelineFilter";
import User from "@/models/user";
import SnowPlow from "@/utility/snowPlow";

library.add(
    faCheckCircle,
    faClipboard,
    faEdit,
    faFlask,
    faPills,
    faPlus,
    faSearch,
    faSyringe,
    faUserMd,
    faVial
);

interface QuickLinkCard {
    index: number;
    title: string;
    description: string;
    icon: string;
}

@Component({
    components: {
        LoadingComponent,
        MessageModalComponent,
        AddQuickLinkComponent,
    },
})
export default class HomeView extends Vue {
    @Action("retrieveAuthenticatedVaccineRecord", {
        namespace: "vaccinationStatus",
    })
    retrieveAuthenticatedVaccineRecord!: (params: {
        hdid: string;
    }) => Promise<CovidVaccineRecord>;

    @Action("setFilter", { namespace: "timeline" })
    setFilter!: (filterBuilder: TimelineFilterBuilder) => void;

    @Action("clearFilter", { namespace: "timeline" })
    clearFilter!: () => void;

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

    @Getter("quickLinks", { namespace: "user" }) quickLinks!:
        | QuickLink[]
        | undefined;

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

    @Ref("addQuickLinkModal")
    readonly addQuickLinkModal!: AddQuickLinkComponent;

    private get isLoading(): boolean {
        return this.isLoadingVaccineRecord;
    }

    private get loadingStatusMessage(): string {
        return this.vaccineRecordStatusMessage;
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

    private get showFederalCardButton(): boolean {
        return this.config.modules["FederalCardButton"];
    }

    private get showVaccineCardButton(): boolean {
        return this.config.modules["VaccinationStatus"];
    }

    private get enabledQuickLinks(): QuickLink[] {
        return (
            this.quickLinks?.filter((quickLink) =>
                quickLink.filter.modules.every(
                    (module) => this.config.modules[module]
                )
            ) ?? []
        );
    }

    private get quickLinkCards(): QuickLinkCard[] {
        return this.enabledQuickLinks.map((quickLink, index) => {
            let card: QuickLinkCard = {
                index,
                title: quickLink.name,
                description: "View your filtered health records.",
                icon: "search",
            };

            const modules = quickLink.filter.modules;
            if (quickLink.filter.modules.length === 1) {
                const details = entryTypeMap.get(modules[0] as EntryType);
                if (details) {
                    card.description = details.description;
                    card.icon = details.icon;
                }
            }

            return card;
        });
    }

    private get isAddQuickLinkButtonDisabled(): boolean {
        return (
            [...entryTypeMap.values()].filter(
                (details) =>
                    this.config.modules[details.type] &&
                    this.enabledQuickLinks.find(
                        (existingLink) =>
                            existingLink.filter.modules.length === 1 &&
                            existingLink.filter.modules[0] === details.type
                    ) === undefined
            ).length === 0
        );
    }

    private retrieveVaccinePdf() {
        this.retrieveAuthenticatedVaccineRecord({
            hdid: this.user.hdid,
        });
    }

    private handleClickHealthRecords(): void {
        this.clearFilter();
        this.$router.push({ path: "/timeline" });
    }

    private handleClickQuickLink(index: number): void {
        const quickLink = this.enabledQuickLinks[index];

        const entryTypes = quickLink.filter.modules
            .map((module) => module as EntryType)
            .filter((entryType) => entryType !== undefined);

        const builder =
            TimelineFilterBuilder.create().withEntryTypes(entryTypes);

        this.setFilter(builder);
        this.$router.push({ path: "/timeline" });
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

    private showAddQuickLinkModal(): void {
        this.addQuickLinkModal.showModal();
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
        <page-title title="Home">
            <hg-button
                :disabled="isAddQuickLinkButtonDisabled"
                data-testid="add-quick-link-button"
                class="float-right"
                variant="secondary"
                @click="showAddQuickLinkModal"
            >
                <hg-icon icon="plus" size="medium" class="mr-2" />
                <span>Add Quick Link</span>
            </hg-button>
        </page-title>
        <h2>What do you want to focus on today?</h2>
        <b-row cols="1" cols-lg="3">
            <b-col class="p-3">
                <hg-card-button
                    title="Health Records"
                    data-testid="health-records-card-btn"
                    @click="handleClickHealthRecords"
                >
                    <template #icon>
                        <img
                            class="health-gateway-logo align-self-center"
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
            <b-col v-if="showFederalCardButton" class="p-3">
                <hg-card-button
                    title="Proof of Vaccination"
                    data-testid="proof-vaccination-card-btn"
                    @click="showSensitiveDocumentDownloadModal()"
                >
                    <template #icon>
                        <img
                            class="canada-government-logo align-self-center"
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
            <b-col v-if="showVaccineCardButton" class="p-3">
                <hg-card-button
                    title="BC Vaccine Card"
                    to="/covid19"
                    data-testid="bc-vaccine-card-btn"
                >
                    <template #icon>
                        <hg-icon
                            class="checkmark align-self-center"
                            icon="check-circle"
                            size="large"
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
            <b-col v-for="card in quickLinkCards" :key="card.title" class="p-3">
                <hg-card-button
                    :title="card.title"
                    data-testid="quick-link-card"
                    @click="handleClickQuickLink(card.index)"
                >
                    <template #icon>
                        <hg-icon
                            :icon="card.icon"
                            class="quick-link-card-icon align-self-center"
                            size="large"
                            square
                        />
                    </template>
                    <div>{{ card.description }}</div>
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
        <AddQuickLinkComponent
            ref="addQuickLinkModal"
            @show="showAddQuickLinkModal"
        />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.health-gateway-logo {
    height: 1.5em;
    width: 1.5em;
}

.canada-government-logo {
    height: 1.5em;
}

.checkmark {
    color: $hg-state-success;
}

.quick-link-card-icon {
    color: $primary;
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
