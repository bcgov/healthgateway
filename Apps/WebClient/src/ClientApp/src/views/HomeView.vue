<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faEdit,
    faEllipsisV,
    faFileMedical,
    faFileWaveform,
    faHouseMedical,
    faMicroscope,
    faPills,
    faPlus,
    faSearch,
    faStethoscope,
    faSyringe,
    faUpload,
    faVial,
} from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/LoadingComponent.vue";
import AddQuickLinkComponent from "@/components/modal/AddQuickLinkComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import TutorialComponent from "@/components/shared/TutorialComponent.vue";
import {
    EntryTypeDetails,
    entryTypeMap,
    getEntryTypeByModule,
} from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import UserPreferenceType from "@/constants/userPreferenceType";
import type { WebClientConfiguration } from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper } from "@/models/dateWrapper";
import { QuickLink } from "@/models/quickLink";
import { LoadStatus } from "@/models/storeOperations";
import { TimelineFilterBuilder } from "@/models/timelineFilter";
import User from "@/models/user";
import { UserPreference } from "@/models/userPreference";
import VaccinationRecord from "@/models/vaccinationRecord";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import SnowPlow from "@/utility/snowPlow";

library.add(
    faCheckCircle,
    faEdit,
    faEllipsisV,
    faFileMedical,
    faFileWaveform,
    faHouseMedical,
    faMicroscope,
    faPills,
    faPlus,
    faSearch,
    faStethoscope,
    faSyringe,
    faUpload,
    faVial
);

interface QuickLinkCard {
    index: number;
    title: string;
    description: string;
    icon: string;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        AddQuickLinkComponent,
        LoadingComponent,
        MessageModalComponent,
        TutorialComponent,
    },
};

@Component(options)
export default class HomeView extends Vue {
    @Action("addError", { namespace: "errorBanner" })
    addError!: (params: {
        errorType: ErrorType;
        source: ErrorSourceType;
        traceId: string | undefined;
    }) => void;

    @Action("setTooManyRequestsError", { namespace: "errorBanner" })
    setTooManyRequestsError!: (params: { key: string }) => void;

    @Action("retrieveAuthenticatedVaccineRecord", {
        namespace: "vaccinationStatus",
    })
    retrieveAuthenticatedVaccineRecord!: (params: {
        hdid: string;
    }) => Promise<CovidVaccineRecord>;

    @Action("stopAuthenticatedVaccineRecordDownload", {
        namespace: "vaccinationStatus",
    })
    stopAuthenticatedVaccineRecordDownload!: (params: { hdid: string }) => void;

    @Action("setFilter", { namespace: "timeline" })
    setFilter!: (filterBuilder: TimelineFilterBuilder) => void;

    @Action("clearFilter", { namespace: "timeline" })
    clearFilter!: () => void;

    @Action("updateQuickLinks", { namespace: "user" })
    updateQuickLinks!: (params: {
        hdid: string;
        quickLinks: QuickLink[];
    }) => Promise<void>;

    @Action("setUserPreference", { namespace: "user" })
    setUserPreference!: (params: {
        preference: UserPreference;
    }) => Promise<void>;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Getter("quickLinks", { namespace: "user" })
    quickLinks!: QuickLink[] | undefined;

    @Getter("authenticatedVaccineRecordStatusChanges", {
        namespace: "vaccinationStatus",
    })
    vaccineRecordStatusChanges!: number;

    @Getter("authenticatedVaccineRecords", { namespace: "vaccinationStatus" })
    vaccineRecords!: Map<string, VaccinationRecord>;

    @Ref("sensitivedocumentDownloadModal")
    readonly sensitivedocumentDownloadModal!: MessageModalComponent;

    @Ref("vaccineRecordResultModal")
    readonly vaccineRecordResultModal!: MessageModalComponent;

    @Ref("addQuickLinkModal")
    readonly addQuickLinkModal!: AddQuickLinkComponent;

    private logger!: ILogger;

    private get addQuickLinkTutorialPreference(): string {
        return UserPreferenceType.TutorialAddQuickLink;
    }

    private get isVaccineRecordDownloading(): boolean {
        const vaccinationRecord: VaccinationRecord | undefined =
            this.getVaccinationRecord();
        if (
            this.vaccineRecordStatusChanges > 0 &&
            vaccinationRecord !== undefined
        ) {
            return vaccinationRecord.status === LoadStatus.REQUESTED;
        }
        return false;
    }

    private get vaccineRecordStatusMessage(): string {
        const vaccinationRecord: VaccinationRecord | undefined =
            this.getVaccinationRecord();
        if (
            this.vaccineRecordStatusChanges > 0 &&
            vaccinationRecord !== undefined
        ) {
            return vaccinationRecord.statusMessage;
        }
        return "";
    }

    private get vaccineRecordResultMessage(): string {
        const vaccinationRecord: VaccinationRecord | undefined =
            this.getVaccinationRecord();
        if (
            this.vaccineRecordStatusChanges > 0 &&
            vaccinationRecord !== undefined
        ) {
            return vaccinationRecord.resultMessage;
        }
        return "";
    }

    private get unverifiedEmail(): boolean {
        return !this.user.verifiedEmail && this.user.hasEmail;
    }

    private get unverifiedSMS(): boolean {
        return !this.user.verifiedSMS && this.user.hasSMS;
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
        return this.config.featureToggleConfiguration.homepage
            .showFederalProofOfVaccination;
    }

    private get preferenceVaccineCardHidden(): boolean {
        const preference =
            this.user.preferences[UserPreferenceType.HideVaccineCardQuickLink];
        return preference?.value === "true";
    }

    private get preferenceOrganDonorHidden(): boolean {
        const preference =
            this.user.preferences[UserPreferenceType.HideOrganDonorQuickLink];
        return preference?.value === "true";
    }

    private get preferenceImmunizationRecordHidden(): boolean {
        const preferenceName =
            UserPreferenceType.HideImmunizationRecordQuickLink;
        const preference = this.user.preferences[preferenceName];
        return preference?.value === "true";
    }

    private get showVaccineCardButton(): boolean {
        return !this.preferenceVaccineCardHidden;
    }

    private get showOrganDonorButton(): boolean {
        const showPreference = !this.preferenceOrganDonorHidden;
        const servicesConfig = ConfigUtil.getFeatureConfiguration().services;
        const servicesEnabled = servicesConfig && servicesConfig.enabled;
        const donorFeatureEnabled =
            servicesConfig &&
            servicesConfig.organDonor &&
            servicesConfig.enabled;
        return servicesEnabled && donorFeatureEnabled && showPreference;
    }

    private get showImmunizationRecordButton(): boolean {
        return !this.preferenceImmunizationRecordHidden;
    }

    private get enabledQuickLinks(): QuickLink[] {
        return (
            this.quickLinks?.filter((quickLink) =>
                quickLink.filter.modules.every((module) => {
                    const entryType = getEntryTypeByModule(module)?.type;
                    return (
                        entryType !== undefined &&
                        ConfigUtil.isDatasetEnabled(entryType)
                    );
                })
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
                const details = getEntryTypeByModule(modules[0]);
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
                    ConfigUtil.isDatasetEnabled(details.type) &&
                    this.enabledQuickLinks.find(
                        (existingLink) =>
                            existingLink.filter.modules.length === 1 &&
                            existingLink.filter.modules[0] ===
                                details.moduleName
                    ) === undefined
            ).length === 0 &&
            !this.preferenceImmunizationRecordHidden &&
            !this.preferenceVaccineCardHidden
        );
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private trackClickLink(linkType: string | undefined): void {
        if (linkType) {
            SnowPlow.trackEvent({
                action: "click",
                text: `home_${linkType}`,
            });
        }
    }

    private retrieveVaccinePdf(): void {
        this.trackClickLink("federal_proof");
        this.retrieveAuthenticatedVaccineRecord({
            hdid: this.user.hdid,
        });
    }

    private removeQuickLink(targetQuickLink: QuickLink): Promise<void> {
        const updatedLinks =
            this.quickLinks?.filter(
                (quickLink) => quickLink !== targetQuickLink
            ) ?? [];

        return this.updateQuickLinks({
            hdid: this.user.hdid,
            quickLinks: updatedLinks,
        }).catch((error) => {
            this.logger.error(error);
            if (error.statusCode === 429) {
                this.setTooManyRequestsError({ key: "page" });
            } else {
                this.addError({
                    errorType: ErrorType.Update,
                    source: ErrorSourceType.Profile,
                    traceId: undefined,
                });
            }
        });
    }

    private getVaccinationRecord(): VaccinationRecord | undefined {
        return this.vaccineRecords.get(this.user.hdid);
    }

    private handleClickHealthRecords(): void {
        this.trackClickLink("all_records");
        this.clearFilter();
        this.$router.push({ path: "/timeline" });
    }

    private handleClickVaccineCard(): void {
        this.trackClickLink("bc_vaccine_card");
        this.$router.push({ path: "/covid19" });
    }

    private handleClickOrganDonorCard(): void {
        this.trackClickLink("organ_donor_card");
        this.$router.push({ path: "/services" });
    }

    private handleClickImmunizationRecord(): void {
        this.trackClickLink("immunization_record");
        window.open(
            "https://www.immunizationrecord.gov.bc.ca/",
            "_blank",
            "noopener"
        );
    }

    private handleClickRemoveQuickLink(index: number): void {
        this.logger.debug("Removing quick link");
        const quickLink = this.enabledQuickLinks[index];
        this.removeQuickLink(quickLink);
    }

    private handleClickRemoveVaccineCardQuickLink(): void {
        this.logger.debug("Removing vaccine card quick link");
        this.setPreferenceValue(
            UserPreferenceType.HideVaccineCardQuickLink,
            "true"
        );
    }

    private handleClickRemoveOrganDonorQuickLink(): void {
        this.logger.debug("Removing organ donor card quick link");
        this.setPreferenceValue(
            UserPreferenceType.HideOrganDonorQuickLink,
            "true"
        );
    }

    private handleClickRemoveImmunizationRecordQuickLink(): void {
        this.logger.debug("Removing immunization record quick link");
        this.setPreferenceValue(
            UserPreferenceType.HideImmunizationRecordQuickLink,
            "true"
        );
    }

    private setPreferenceValue(preferenceType: string, value: string) {
        const preference = {
            ...this.user.preferences[preferenceType],
            value,
        };

        this.setUserPreference({ preference }).catch((error) => {
            this.logger.error(error);
            if (error.statusCode === 429) {
                this.setTooManyRequestsError({ key: "page" });
            } else {
                this.addError({
                    errorType: ErrorType.Update,
                    source: ErrorSourceType.Profile,
                    traceId: undefined,
                });
            }
        });
    }

    private handleClickQuickLink(index: number): void {
        const quickLink = this.enabledQuickLinks[index];

        const detailsCollection = quickLink.filter.modules
            .map((module) => getEntryTypeByModule(module))
            .filter((d): d is EntryTypeDetails => d !== undefined);

        if (detailsCollection.length === 1) {
            const linkType = detailsCollection[0].eventName;
            this.trackClickLink(linkType);
        }

        const entryTypes = detailsCollection.map((d) => d.type);
        const builder =
            TimelineFilterBuilder.create().withEntryTypes(entryTypes);

        this.setFilter(builder);
        this.$router.push({ path: "/timeline" });
    }

    @Watch("vaccineRecordStatusChanges")
    private showVaccineRecordResultModal(): void {
        const vaccinationRecord: VaccinationRecord | undefined =
            this.getVaccinationRecord();
        if (
            vaccinationRecord !== undefined &&
            vaccinationRecord.resultMessage.length > 0
        ) {
            this.vaccineRecordResultModal.showModal();
        }
    }

    @Watch("vaccineRecordStatusChanges")
    private saveVaccinePdf(): void {
        const vaccinationRecord: VaccinationRecord | undefined =
            this.getVaccinationRecord();

        if (
            vaccinationRecord?.record !== undefined &&
            vaccinationRecord.hdid === this.user.hdid &&
            vaccinationRecord.status === LoadStatus.LOADED &&
            vaccinationRecord.download
        ) {
            const mimeType = vaccinationRecord.record.document.mediaType;
            const downloadLink = `data:${mimeType};base64,${vaccinationRecord.record.document.data}`;
            fetch(downloadLink).then((res) => {
                SnowPlow.trackEvent({
                    action: "click_button",
                    text: "FederalPVC",
                });
                res.blob().then((blob) => saveAs(blob, "VaccineProof.pdf"));
            });
            this.stopAuthenticatedVaccineRecordDownload({
                hdid: this.user.hdid,
            });
        }
    }

    private showSensitiveDocumentDownloadModal(): void {
        this.sensitivedocumentDownloadModal.showModal();
    }

    private showAddQuickLinkModal(): void {
        this.addQuickLinkModal.showModal();
    }
}
</script>

<template>
    <div>
        <LoadingComponent
            :is-loading="isVaccineRecordDownloading"
            :text="vaccineRecordStatusMessage"
        />
        <b-alert
            v-if="unverifiedEmail || unverifiedSMS"
            data-testid="incomplete-profile-banner"
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
                id="add-quick-link-button"
                data-testid="add-quick-link-button"
                :disabled="isAddQuickLinkButtonDisabled"
                class="float-right"
                variant="secondary"
                @click="showAddQuickLinkModal"
            >
                <hg-icon icon="plus" size="medium" class="mr-2" />
                <span>Add Quick Link</span>
            </hg-button>
            <TutorialComponent
                :preference-type="addQuickLinkTutorialPreference"
                target="add-quick-link-button"
            >
                <div data-testid="add-quick-link-tutorial-popover">
                    Add a quick link to easily access a health record type from
                    your home screen.
                </div>
            </TutorialComponent>
        </page-title>
        <h2>What do you want to focus on today?</h2>
        <b-row cols="1" cols-lg="2" cols-xl="3">
            <b-col class="p-3">
                <hg-card-button
                    title="Health Records"
                    data-testid="health-records-card"
                    @click="handleClickHealthRecords()"
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
                        including dispensed medications, health visits, COVID‑19
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
            <b-col v-if="showOrganDonorButton" class="p-3">
                <hg-card-button
                    title="Organ Donor Registration"
                    data-testid="bc-vaccine-card-card"
                    @click="handleClickOrganDonorCard()"
                >
                    <template #icon>
                        <img
                            class="organ-donor-registry-logo align-self-center"
                            src="@/assets/images/services/odr-logo.svg"
                            alt="Organ Donor Registry Logo"
                        />
                    </template>
                    <template #menu>
                        <b-nav align="right">
                            <b-nav-item-dropdown
                                right
                                text=""
                                :no-caret="true"
                                menu-class="quick-link-menu"
                                toggle-class="quick-link-menu-button"
                            >
                                <template slot="button-content">
                                    <hg-icon
                                        icon="ellipsis-v"
                                        size="medium"
                                        data-testid="quick-link-menu-button"
                                    />
                                </template>
                                <b-dropdown-item
                                    data-testid="remove-quick-link-button"
                                    @click.stop="
                                        handleClickRemoveOrganDonorQuickLink()
                                    "
                                >
                                    Remove
                                </b-dropdown-item>
                            </b-nav-item-dropdown>
                        </b-nav>
                    </template>
                    <div>
                        BC Transplant oversees all aspects of organ donation and
                        transplant across BC and manages the BC Organ Donor
                        Registry.
                    </div>
                </hg-card-button>
            </b-col>
            <b-col v-if="showVaccineCardButton" class="p-3">
                <hg-card-button
                    title="BC Vaccine Card"
                    data-testid="bc-vaccine-card-card"
                    @click="handleClickVaccineCard()"
                >
                    <template #icon>
                        <hg-icon
                            class="checkmark align-self-center"
                            icon="check-circle"
                            size="large"
                            square
                        />
                    </template>
                    <template #menu>
                        <b-nav align="right">
                            <b-nav-item-dropdown
                                right
                                text=""
                                :no-caret="true"
                                menu-class="quick-link-menu"
                                toggle-class="quick-link-menu-button"
                            >
                                <template slot="button-content">
                                    <hg-icon
                                        icon="ellipsis-v"
                                        size="medium"
                                        data-testid="quick-link-menu-button"
                                    />
                                </template>
                                <b-dropdown-item
                                    data-testid="remove-quick-link-button"
                                    @click.stop="
                                        handleClickRemoveVaccineCardQuickLink()
                                    "
                                >
                                    Remove
                                </b-dropdown-item>
                            </b-nav-item-dropdown>
                        </b-nav>
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
                    <template #menu>
                        <b-nav align="right">
                            <b-nav-item-dropdown
                                right
                                text=""
                                :no-caret="true"
                                menu-class="quick-link-menu"
                                toggle-class="quick-link-menu-button"
                            >
                                <template slot="button-content">
                                    <hg-icon
                                        icon="ellipsis-v"
                                        size="medium"
                                        data-testid="quick-link-menu-button"
                                    />
                                </template>
                                <b-dropdown-item
                                    data-testid="remove-quick-link-button"
                                    @click.stop="
                                        handleClickRemoveQuickLink(card.index)
                                    "
                                >
                                    Remove
                                </b-dropdown-item>
                            </b-nav-item-dropdown>
                        </b-nav>
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

.quick-link-menu {
    a {
        &:focus:not([disabled]):not(:active),
        &:hover:not([disabled]):not(:active) {
            color: $hg-text-primary;
        }

        &:active:not([disabled]) {
            color: white;
        }
    }
}

.quick-link-menu-button {
    color: $soft_text;
    border: 1px solid rgba(0, 0, 0, 0);
    border-radius: 0.25rem;

    &:focus:not([disabled]),
    &:hover:not([disabled]) {
        color: $soft_text;
        border: 1px solid rgba(0, 0, 0, 0.15);
    }
}

.nav-link.active,
.nav-item.show .nav-link {
    &.quick-link-menu-button {
        background-color: white;
        border: 1px solid rgba(0, 0, 0, 0.15);
    }
}
</style>
