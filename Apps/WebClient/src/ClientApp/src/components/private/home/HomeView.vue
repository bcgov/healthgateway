<script setup lang="ts">
import { saveAs } from "file-saver";
import { computed, ref, watch } from "vue";
import { useRouter } from "vue-router";

import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import HgCardComponent from "@/components/common/HgCardComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import AddQuickLinkComponent from "@/components/private/home/AddQuickLinkComponent.vue";
import RecommendationsDialogComponent from "@/components/private/reports/RecommendationsDialogComponent.vue";
import {
    EntryType,
    EntryTypeDetails,
    entryTypeMap,
    getEntryTypeByModule,
} from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ServiceName } from "@/constants/serviceName";
import UserPreferenceType from "@/constants/userPreferenceType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { QuickLink } from "@/models/quickLink";
import { LoadStatus } from "@/models/storeOperations";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import {
    Action,
    Dataset,
    Destination,
    ExternalUrl,
    Format,
    InternalUrl,
    Origin,
    Text,
    Type,
} from "@/plugins/extensions";
import { ILogger, ITrackingService } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { useErrorStore } from "@/stores/error";
import { useTimelineStore } from "@/stores/timeline";
import { useUserStore } from "@/stores/user";
import { useVaccinationStatusAuthenticatedStore } from "@/stores/vaccinationStatusAuthenticated";
import ConfigUtil from "@/utility/configUtil";
import EventDataUtility from "@/utility/eventDataUtility";
import { useGrid } from "@/utility/useGrid";

interface QuickLinkCard {
    index: number;
    title: string;
    description: string;
    logoUri?: string;
    icon: string;
}

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);
const router = useRouter();
const configStore = useConfigStore();
const userStore = useUserStore();
const authenticatedVaccinationStatusStore =
    useVaccinationStatusAuthenticatedStore();
const errorStore = useErrorStore();
const timelineStore = useTimelineStore();
const { columns } = useGrid();

const sensitiveDocumentDownloadModal =
    ref<InstanceType<typeof MessageModalComponent>>();
const vaccineRecordResultModal =
    ref<InstanceType<typeof MessageModalComponent>>();
const recommendationsDialogComponent =
    ref<InstanceType<typeof RecommendationsDialogComponent>>();

const user = computed(() => userStore.user);
const quickLinks = computed(() => userStore.quickLinks);
const vaccineRecordState = computed(() =>
    authenticatedVaccinationStatusStore.vaccineRecordState(user.value.hdid)
);

const isVaccineRecordDownloading = computed(
    () => vaccineRecordState.value.status === LoadStatus.REQUESTED
);
const vaccineRecordStatusMessage = computed(
    () => vaccineRecordState.value.statusMessage
);
const vaccineRecordResultMessage = computed(
    () => vaccineRecordState.value.resultMessage
);
const unverifiedEmail = computed(
    () => !user.value.verifiedEmail && user.value.hasEmail
);
const unverifiedSms = computed(
    () => !user.value.verifiedSms && user.value.hasSms
);
const showFederalCardButton = computed(
    () =>
        configStore.webConfig.featureToggleConfiguration.homepage
            .showFederalProofOfVaccination
);
const preferenceVaccineCardHidden = computed(
    () =>
        user.value.preferences[UserPreferenceType.HideVaccineCardQuickLink]
            ?.value === "true"
);
const preferenceOrganDonorHidden = computed(
    () =>
        user.value.preferences[UserPreferenceType.HideOrganDonorQuickLink]
            ?.value === "true"
);
const preferenceImmunizationRecordHidden = computed(
    () =>
        user.value.preferences[
            UserPreferenceType.HideImmunizationRecordQuickLink
        ]?.value === "true"
);
const preferenceRecommendationsLinkHidden = computed(
    () =>
        user.value.preferences[UserPreferenceType.HideRecommendationsQuickLink]
            ?.value === "true"
);
const preferenceHealthConnectHidden = computed(
    () =>
        user.value.preferences[
            UserPreferenceType.HideHealthConnectRegistryQuickLink
        ]?.value === "true"
);
const showRecommendationsCardButton = computed(
    () =>
        configStore.webConfig.featureToggleConfiguration.homepage
            .showRecommendationsLink &&
        ConfigUtil.isDatasetEnabled(EntryType.Immunization) &&
        !preferenceRecommendationsLinkHidden.value
);
const showVaccineCardButton = computed(
    () => !preferenceVaccineCardHidden.value
);
const showOrganDonorCardButton = computed(
    () =>
        ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration) &&
        !preferenceOrganDonorHidden.value
);
const showHealthConnectCardButton = computed(
    () =>
        ConfigUtil.isServiceEnabled(ServiceName.HealthConnectRegistry) &&
        !preferenceHealthConnectHidden.value
);
const showOtherRecordSourcesCardButton = computed(() =>
    ConfigUtil.isOtherRecordSourcesFeatureEnabled()
);
const showImmunizationRecordCardButton = computed(
    () =>
        ConfigUtil.isImmunizationRecordLinkEnabled() &&
        ConfigUtil.isDatasetEnabled(EntryType.Immunization) &&
        !preferenceImmunizationRecordHidden.value
);

const enabledQuickLinks = computed(
    () =>
        quickLinks.value?.filter((quickLink) =>
            quickLink.filter.modules.every((module) => {
                const entryType = getEntryTypeByModule(module)?.type;
                return (
                    entryType !== undefined &&
                    ConfigUtil.isDatasetEnabled(entryType)
                );
            })
        ) ?? []
);
const quickLinkCards = computed(() =>
    enabledQuickLinks.value.map((quickLink, index) => {
        const card: QuickLinkCard = {
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
                card.logoUri = details.logoUri;
            }
        }

        return card;
    })
);
const isAddQuickLinkButtonDisabled = computed(
    () =>
        [...entryTypeMap.values()].filter(
            (details) =>
                ConfigUtil.isDatasetEnabled(details.type) &&
                enabledQuickLinks.value.find(
                    (existingLink) =>
                        existingLink.filter.modules.length === 1 &&
                        existingLink.filter.modules[0] === details.moduleName
                ) === undefined
        ).length === 0 &&
        !preferenceImmunizationRecordHidden.value &&
        !preferenceVaccineCardHidden.value &&
        !preferenceOrganDonorHidden.value &&
        !preferenceHealthConnectHidden.value &&
        !preferenceRecommendationsLinkHidden.value
);

function retrieveAuthenticatedVaccineRecord(hdid: string): Promise<void> {
    return authenticatedVaccinationStatusStore.retrieveVaccineRecord(hdid);
}

function stopAuthenticatedVaccineRecordDownload(hdid: string): void {
    authenticatedVaccinationStatusStore.stopVaccineRecordDownload(hdid);
}

function retrieveVaccinePdf(): void {
    retrieveAuthenticatedVaccineRecord(user.value.hdid);
}

function removeQuickLink(targetQuickLink: QuickLink): Promise<void> {
    const updatedLinks =
        quickLinks.value?.filter(
            (quickLink) => quickLink !== targetQuickLink
        ) ?? [];

    return userStore
        .updateQuickLinks(user.value.hdid, updatedLinks)
        .catch((error) => {
            logger.error(error);
            if (error.statusCode === 429) {
                errorStore.setTooManyRequestsError("page");
            } else {
                errorStore.addError(
                    ErrorType.Update,
                    ErrorSourceType.Profile,
                    undefined
                );
            }
        });
}

function handleClickHealthRecords(): void {
    trackingService.trackEvent({
        action: Action.InternalLink,
        text: Text.HealthRecords,
        origin: Origin.Home,
        destination: Destination.HealthRecords,
        type: Type.HomeTile,
        url: InternalUrl.HealthRecords,
    });
    router.push({ path: "/timeline" });
}

function handleClickImmunizationRecord(): void {
    trackingService.trackEvent({
        action: Action.InternalLink,
        text: Text.ImmunizationRecord,
        origin: Origin.Home,
        destination: Destination.Download,
        dataset: Dataset.Immunizations,
        type: Type.HomeTile,
        url: InternalUrl.Export,
    });
    router.push({
        path: "/reports",
        query: { defaultEntryType: EntryType.Immunization },
    });
}

function handleClickVaccineCard(): void {
    trackingService.trackEvent({
        action: Action.InternalLink,
        text: Text.BcVaccineCard,
        origin: Origin.Home,
        destination: Destination.BcVaccineCard,
        type: Type.HomeTile,
    });
    router.push({ path: "/covid19" });
}

function handleClickOrganDonorCard(): void {
    trackingService.trackEvent({
        action: Action.InternalLink,
        text: Text.OrganDonor,
        origin: Origin.Home,
        destination: Destination.Services,
        type: Type.HomeTile,
        url: InternalUrl.OrganDonor,
    });
    router.push({ path: "/services" });
}

function handleClickOtherRecordSources(): void {
    trackingService.trackEvent({
        action: Action.Visit,
        text: Text.ExternalLink,
        destination: Destination.OtherRecordSources,
        origin: Origin.Home,
    });
    router.push({ path: "/otherRecordSources" });
}

function showRecommendationsDialog(): void {
    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.ViewRecommendedImmunizations,
        origin: Origin.Home,
        dataset: Dataset.Immunizations,
        type: Type.HomeTile,
    });
    recommendationsDialogComponent.value?.showDialog();
}

function handleClickHealthConnectCard(): void {
    trackingService.trackEvent({
        action: Action.ExternalLink,
        text: Text.HealthLinkBC,
        destination: Destination.HealthConnectRegistry,
        origin: Origin.Home,
        type: Type.HomeTile,
        url: ExternalUrl.HealthConnectRegistry,
    });
    window.open(ExternalUrl.HealthConnectRegistry, undefined, "noopener");
}

function handleClickRemoveQuickLink(index: number): void {
    logger.debug("Removing quick link");
    const quickLink = enabledQuickLinks.value[index];
    removeQuickLink(quickLink);
}

function handleClickRemoveVaccineCardQuickLink(): void {
    logger.debug("Removing vaccine card quick link");
    setPreferenceValue(UserPreferenceType.HideVaccineCardQuickLink, "true");
}

function handleClickRemoveOrganDonorQuickLink(): void {
    logger.debug("Removing organ donor card quick link");
    setPreferenceValue(UserPreferenceType.HideOrganDonorQuickLink, "true");
}

function handleClickRemoveHealthConnectCard(): void {
    logger.debug("Removing health connect card");
    setPreferenceValue(
        UserPreferenceType.HideHealthConnectRegistryQuickLink,
        "true"
    );
}

function handleClickRemoveRecommendations(): void {
    logger.debug("Removing Immunize BC card");
    setPreferenceValue(UserPreferenceType.HideRecommendationsQuickLink, "true");
}

function handleClickRemoveImmunizationRecord(): void {
    logger.debug("Removing Immunization Record card");
    setPreferenceValue(
        UserPreferenceType.HideImmunizationRecordQuickLink,
        "true"
    );
}

function setPreferenceValue(preferenceType: string, value: string) {
    const preference = {
        ...user.value.preferences[preferenceType],
        value,
    };

    userStore.setUserPreference(preference).catch((error) => {
        logger.error(error);
        if (error.statusCode === 429) {
            errorStore.setTooManyRequestsError("page");
        } else {
            errorStore.addError(
                ErrorType.Update,
                ErrorSourceType.Profile,
                undefined
            );
        }
    });
}

function handleClickQuickLink(index: number): void {
    const quickLink = enabledQuickLinks.value[index];

    const detailsCollection = quickLink.filter.modules
        .map((module) => getEntryTypeByModule(module))
        .filter((d): d is EntryTypeDetails => d !== undefined);

    if (detailsCollection.length === 1) {
        trackingService.trackEvent({
            action: Action.InternalLink,
            text: Text.QuickLink,
            origin: Origin.Home,
            dataset: EventDataUtility.getDataset(detailsCollection[0].type),
            destination: `${EventDataUtility.getDataset(
                detailsCollection[0].type
            )} Timeline`,
            url: InternalUrl.QuickLink,
        });
    }

    const entryTypes = detailsCollection.map((d) => d.type);
    const builder = TimelineFilterBuilder.create().withEntryTypes(entryTypes);

    timelineStore.setFilter(builder);
    router.push({ path: "/timeline" });
}

function showSensitiveDocumentDownloadModal(): void {
    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.ViewProofOfVaccination,
        origin: Origin.Home,
        type: Type.HomeTile,
    });
    sensitiveDocumentDownloadModal.value?.showModal();
}

watch(vaccineRecordState, () => {
    if (vaccineRecordState.value.resultMessage.length > 0) {
        vaccineRecordResultModal.value?.showModal();
    }

    if (
        vaccineRecordState.value.record !== undefined &&
        vaccineRecordState.value.status === LoadStatus.LOADED &&
        vaccineRecordState.value.download
    ) {
        const mimeType = vaccineRecordState.value.record.document.mediaType;
        const downloadLink = `data:${mimeType};base64,${vaccineRecordState.value.record.document.data}`;
        fetch(downloadLink).then((res) => {
            trackingService.trackEvent({
                action: Action.ButtonClick,
                text: Text.DownloadProofOfVaccination,
                origin: Origin.Home,
                destination: Destination.Download,
                type: Type.Covid19ProofOfVaccination,
                format: Format.Pdf,
            });
            res.blob().then((blob) => saveAs(blob, "VaccineProof.pdf"));
        });
        stopAuthenticatedVaccineRecordDownload(user.value.hdid);
    }
});
</script>

<template>
    <LoadingComponent
        :is-loading="isVaccineRecordDownloading"
        :text="vaccineRecordStatusMessage"
    />
    <HgAlertComponent
        v-if="unverifiedEmail || unverifiedSms"
        data-testid="incomplete-profile-banner"
        closable
        type="info"
        title="Verify Contact Information"
        variant="outlined"
    >
        <template #default>
            <span class="text-body-1">
                Your email or cell phone number has not been verified. You can
                use the Health Gateway without verified contact information,
                however, you will not receive notifications. Visit the
                <router-link
                    id="profilePageLink"
                    data-testid="profile-page-link"
                    class="text-link"
                    to="/profile"
                    @click="
                        trackingService.trackEvent({
                            action: Action.InternalLink,
                            text: Text.VerifyContactInformation,
                            origin: Origin.Home,
                            destination: Destination.Profile,
                            type: Type.InfoBanner,
                            url: InternalUrl.Profile,
                        })
                    "
                    >Profile Page</router-link
                >
                to complete your verification.
            </span>
        </template>
    </HgAlertComponent>
    <HgAlertComponent
        data-testid="immunization-tip-banner"
        closable
        type="info"
        variant="outlined"
    >
        <template #default>
            <span class="text-body-1">
                Tip: Immunization schedule can be viewed in
                <router-link
                    id="exportRecordsLink"
                    data-testid="export-records-link"
                    class="text-link"
                    to="/reports"
                    @click="
                        trackingService.trackEvent({
                            action: Action.InternalLink,
                            text: Text.ImmunizationScheduleExport,
                            origin: Origin.Home,
                            destination: Destination.Export,
                            type: Type.InfoBanner,
                            url: InternalUrl.ImmunizationScheduleExport,
                        })
                    "
                    >Export</router-link
                >
                and
                <router-link
                    id="dependentsLink"
                    data-testid="dependents-link"
                    class="text-link"
                    to="/dependents"
                    @click="
                        trackingService.trackEvent({
                            action: Action.InternalLink,
                            text: Text.ImmunizationScheduleDependents,
                            origin: Origin.Home,
                            destination: Destination.Dependents,
                            type: Type.InfoBanner,
                            url: InternalUrl.ImmunizationScheduleDependents,
                        })
                    "
                    >Dependents</router-link
                >
                menu items
            </span>
        </template>
    </HgAlertComponent>
    <PageTitleComponent title="Home">
        <template #append>
            <AddQuickLinkComponent :disabled="isAddQuickLinkButtonDisabled" />
        </template>
    </PageTitleComponent>
    <v-row>
        <v-col :cols="columns" class="d-flex">
            <HgCardComponent
                title="Health Records"
                variant="outlined"
                elevation="1"
                border="thin grey-lighten-2"
                data-testid="health-records-card"
                class="flex-grow-1"
                @click="handleClickHealthRecords()"
            >
                <template #icon>
                    <img
                        class="quick-link-icon"
                        src="@/assets/images/home/health-records-icon.svg"
                        alt="Health Gateway Logo"
                    />
                </template>
                <p class="text-body-1">
                    View your available health records, including dispensed
                    medications, health visits, lab results, immunizations, and
                    more.
                </p>
            </HgCardComponent>
        </v-col>
        <v-col
            v-if="showImmunizationRecordCardButton"
            :cols="columns"
            class="d-flex"
        >
            <HgCardComponent
                title="Immunization Record"
                variant="outlined"
                elevation="1"
                border="thin grey-lighten-2"
                data-testid="immunization-record-card-button"
                class="flex-grow-1"
                @click="handleClickImmunizationRecord()"
            >
                <template #icon>
                    <v-icon
                        icon="cloud-download"
                        class="quick-link-icon"
                        aria-label="Download icon"
                        color="primary"
                        size="small"
                    />
                </template>
                <template #menu-items>
                    <v-list-item
                        data-testid="remove-quick-link-button"
                        title="Remove"
                        @click.stop="handleClickRemoveImmunizationRecord()"
                    />
                </template>
                <p class="text-body-1">
                    Download a record of your immunizations, including
                    recommended vaccines.
                </p>
            </HgCardComponent>
        </v-col>
        <v-col
            v-if="showRecommendationsCardButton"
            :cols="columns"
            class="d-flex"
        >
            <HgCardComponent
                title="Immunizations"
                variant="outlined"
                elevation="1"
                border="thin grey-lighten-2"
                data-testid="recommendations-card-button"
                class="flex-grow-1"
                @click="showRecommendationsDialog()"
            >
                <template #icon>
                    <v-icon
                        icon="syringe"
                        class="quick-link-icon"
                        aria-label="Syringe icon"
                        color="primary"
                        size="small"
                    />
                </template>
                <template #menu-items>
                    <v-list-item
                        data-testid="remove-quick-link-button"
                        title="Remove"
                        @click.stop="handleClickRemoveRecommendations()"
                    />
                </template>
                <p class="text-body-1">
                    View immunizations you received from community pharmacies or
                    public health, including vaccine recommendations.
                </p>
            </HgCardComponent>
        </v-col>
        <v-col
            v-if="showHealthConnectCardButton"
            :cols="columns"
            class="d-flex"
        >
            <HgCardComponent
                variant="outlined"
                elevation="1"
                border="thin grey-lighten-2"
                data-testid="health-connect-registry-card"
                class="flex-grow-1"
                @click="handleClickHealthConnectCard()"
            >
                <template #icon>
                    <img
                        class="quick-link-icon-large"
                        src="@/assets/images/services/health-link-logo.svg"
                        alt="Health Connect Registry Logo"
                    />
                </template>
                <template #menu-items>
                    <v-list-item
                        data-testid="remove-quick-link-button"
                        title="Remove"
                        @click.stop="handleClickRemoveHealthConnectCard()"
                    />
                </template>
                <p class="text-body-1">
                    Register for the Health Connect Registry and get matched
                    with a family doctor or nurse practitioner in your
                    community.
                </p>
            </HgCardComponent>
        </v-col>
        <v-col v-if="showOrganDonorCardButton" :cols="columns" class="d-flex">
            <HgCardComponent
                title="Organ Donor Registration"
                variant="outlined"
                elevation="1"
                border="thin grey-lighten-2"
                data-testid="organ-donor-registration-card"
                class="flex-grow-1"
                @click="handleClickOrganDonorCard()"
            >
                <template #icon>
                    <img
                        class="quick-link-icon"
                        src="@/assets/images/services/odr-logo.svg"
                        alt="Organ Donor Registry Logo"
                    />
                </template>
                <template #menu-items>
                    <v-list-item
                        data-testid="remove-quick-link-button"
                        title="Remove"
                        @click.stop="handleClickRemoveOrganDonorQuickLink()"
                    />
                </template>
                <p class="text-body-1">
                    Check whether you have registered your decision on organ
                    donation with BC Transplant. If you have registered your
                    decision, you can review the details here.
                </p>
            </HgCardComponent>
        </v-col>
        <v-col v-if="showFederalCardButton" :cols="columns" class="d-flex">
            <HgCardComponent
                title="Proof of Vaccination"
                variant="outlined"
                elevation="1"
                border="thin grey-lighten-2"
                data-testid="proof-vaccination-card-btn"
                class="flex-grow-1"
                @click="showSensitiveDocumentDownloadModal()"
            >
                <template #icon>
                    <img
                        class="quick-link-icon"
                        src="@/assets/images/gov/canada-gov-logo.svg"
                        alt="Canada Government Logo"
                    />
                </template>
                <p class="text-body-1">
                    Download and print your Federal Proof of Vaccination
                    Certificate (PVC) for domestic and international travel.
                </p>
            </HgCardComponent>
        </v-col>
        <v-col
            v-if="showVaccineCardButton && false"
            :cols="columns"
            class="d-flex"
        >
            <HgCardComponent
                title="BC Vaccine Card"
                variant="outlined"
                elevation="1"
                border="thin grey-lighten-2"
                data-testid="bc-vaccine-card-card"
                class="flex-grow-1"
                @click="handleClickVaccineCard()"
            >
                <template #icon>
                    <v-icon icon="check-circle" color="success" />
                </template>
                <template #menu-items>
                    <v-list-item
                        data-testid="remove-quick-link-button"
                        title="Remove"
                        @click.stop="handleClickRemoveVaccineCardQuickLink()"
                    />
                </template>
                <p class="text-body-1">
                    View, download and print your BC Vaccine Card. Present this
                    card as proof of vaccination at some BC businesses, services
                    and events.
                </p>
            </HgCardComponent>
        </v-col>
        <v-col
            v-for="card in quickLinkCards"
            :key="card.title"
            :cols="columns"
            class="d-flex"
        >
            <HgCardComponent
                :title="card.title"
                variant="outlined"
                elevation="1"
                border="thin grey-lighten-2"
                data-testid="quick-link-card"
                class="flex-grow-1"
                @click="handleClickQuickLink(card.index)"
            >
                <template #icon>
                    <img
                        v-if="card.logoUri"
                        class="quick-link-icon"
                        :alt="`${card.title ?? 'dataset'} Logo`"
                        :src="card.logoUri"
                    />
                    <v-icon
                        v-else
                        :icon="card.icon"
                        class="quick-link-icon"
                        color="primary"
                        size="small"
                    />
                </template>
                <template #menu-items>
                    <v-list-item
                        data-testid="remove-quick-link-button"
                        title="Remove"
                        @click.stop="handleClickRemoveQuickLink(card.index)"
                    />
                </template>
                <p class="text-body-1">{{ card.description }}</p>
            </HgCardComponent>
        </v-col>
    </v-row>
    <v-row>
        <v-col v-if="showOtherRecordSourcesCardButton" cols="12" class="d-flex">
            <HgCardComponent
                density="compact"
                variant="outlined"
                elevation="1"
                border="thin grey-lighten-2"
                class="flex-grow-1 w-100"
                data-testid="other-record-sources-card"
                @click="handleClickOtherRecordSources()"
            >
                <template #default>
                    <div class="d-flex align-start">
                        <img
                            src="@/assets/images/home/other-record-sources.svg"
                            alt="Other record sources illustration"
                            class="other-record-sources-img mr-4 mr-md-6"
                        />
                        <div>
                            <div
                                class="text-h6 font-weight-bold text-high-emphasis mb-1 mb-md-2"
                            >
                                Other record sources
                            </div>
                            <p class="text-body-1 mb-0">
                                Some health records may not appear in Health
                                Gateway. Learn about other trusted websites to
                                find where your records may be.
                            </p>
                        </div>
                    </div>
                </template>
            </HgCardComponent>
        </v-col>
    </v-row>
    <RecommendationsDialogComponent
        ref="recommendationsDialogComponent"
        :hdid="user.hdid"
    />
    <MessageModalComponent
        ref="sensitiveDocumentDownloadModal"
        title="Sensitive Document"
        message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
        @submit="retrieveVaccinePdf"
    />
    <MessageModalComponent
        ref="vaccineRecordResultModal"
        ok-only
        title="Alert"
        :message="vaccineRecordResultMessage"
    />
</template>

<style lang="scss" scoped>
.other-record-sources-img {
    height: 3.5rem;
    @media (max-width: 960px) {
        height: 3rem;
    }
}
.quick-link-icon {
    height: 1.5em;
    &-large {
        height: 2.1em;
    }
}
</style>
