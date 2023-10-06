<script setup lang="ts">
import { saveAs } from "file-saver";
import { computed, ref, watch } from "vue";
import { useRouter } from "vue-router";

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
import { DateWrapper } from "@/models/dateWrapper";
import { QuickLink } from "@/models/quickLink";
import { LoadStatus } from "@/models/storeOperations";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { ILogger } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { useErrorStore } from "@/stores/error";
import { useTimelineStore } from "@/stores/timeline";
import { useUserStore } from "@/stores/user";
import { useVaccinationStatusAuthenticatedStore } from "@/stores/vaccinationStatusAuthenticated";
import ConfigUtil from "@/utility/configUtil";
import { getGridCols } from "@/utility/gridUtilty";
import SnowPlow from "@/utility/snowPlow";

interface QuickLinkCard {
    index: number;
    title: string;
    description: string;
    logoUri?: string;
    icon: string;
}

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const router = useRouter();
const configStore = useConfigStore();
const userStore = useUserStore();
const authenticatedVaccinationStatusStore =
    useVaccinationStatusAuthenticatedStore();
const errorStore = useErrorStore();
const timelineStore = useTimelineStore();

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
const isPacificTime = computed(() => {
    const isDaylightSavings = new DateWrapper().isInDST();
    const pacificTimeZoneHourOffset = isDaylightSavings ? 7 : 8;
    return new Date().getTimezoneOffset() / 60 === pacificTimeZoneHourOffset;
});
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
const showOrganDonorButton = computed(
    () =>
        ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration) &&
        !preferenceOrganDonorHidden.value
);
const showHealthConnectButton = computed(
    () =>
        ConfigUtil.isServiceEnabled(ServiceName.HealthConnectRegistry) &&
        !preferenceHealthConnectHidden.value
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

function trackClickLink(linkType: string | undefined): void {
    if (linkType) {
        SnowPlow.trackEvent({
            action: "click",
            text: `home_${linkType}`,
        });
    }
}

function retrieveVaccinePdf(): void {
    trackClickLink("federal_proof");
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
    trackClickLink("all_records");
    router.push({ path: "/timeline" });
}

function handleClickVaccineCard(): void {
    trackClickLink("bc_vaccine_card");
    router.push({ path: "/covid19" });
}

function handleClickOrganDonorCard(): void {
    trackClickLink("organ_donor_registration");
    router.push({ path: "/services" });
}

function showRecommendationsDialog(): void {
    trackClickLink("recommendations");
    recommendationsDialogComponent.value?.showDialog();
}

function handleClickHealthConnectCard(): void {
    trackClickLink("primarycare");
    window.open(
        "https://www.healthlinkbc.ca/health-connect-registry",
        undefined,
        "noopener"
    );
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
        const linkType = detailsCollection[0].eventName;
        trackClickLink(linkType);
    }

    const entryTypes = detailsCollection.map((d) => d.type);
    const builder = TimelineFilterBuilder.create().withEntryTypes(entryTypes);

    timelineStore.setFilter(builder);
    router.push({ path: "/timeline" });
}

function showSensitiveDocumentDownloadModal(): void {
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
            SnowPlow.trackEvent({
                action: "click_button",
                text: "FederalPVC",
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
    <v-alert
        v-if="unverifiedEmail || unverifiedSms"
        data-testid="incomplete-profile-banner"
        closable
        type="info"
        class="d-print-none mb-4"
        title="Verify Contact Information"
        variant="outlined"
        border
    >
        <span class="text-body-1">
            Your email or cell phone number has not been verified. You can use
            the Health Gateway without verified contact information, however,
            you will not receive notifications. Visit the
            <router-link
                id="profilePageLink"
                data-testid="profile-page-link"
                variant="primary"
                to="/profile"
                >Profile Page</router-link
            >
            to complete your verification.
        </span>
    </v-alert>
    <v-alert
        v-if="!isPacificTime"
        closable
        type="info"
        title="Looks like you're in a different timezone."
        text="Heads up: your health records are recorded and displayed in
                Pacific Time."
        class="d-print-none mb-4"
        variant="outlined"
        border
    />
    <PageTitleComponent title="Home">
        <template #append>
            <AddQuickLinkComponent :disabled="isAddQuickLinkButtonDisabled" />
        </template>
    </PageTitleComponent>
    <v-row>
        <v-col :cols="getGridCols" class="d-flex">
            <HgCardComponent
                title="Health Records"
                data-testid="health-records-card"
                class="flex-grow-1"
                @click="handleClickHealthRecords()"
            >
                <template #icon>
                    <img
                        class="quick-link-icon"
                        src="@/assets/images/gov/health-gateway-logo.svg"
                        alt="Health Gateway Logo"
                    />
                </template>
                <p class="text-body-1">
                    View and manage all your available health records, including
                    dispensed medications, health visits, COVID‑19 test results,
                    immunizations and more.
                </p>
            </HgCardComponent>
        </v-col>
        <v-col v-if="showFederalCardButton" :cols="getGridCols" class="d-flex">
            <HgCardComponent
                title="Proof of Vaccination"
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
                    Download and print your Federal Proof of Vaccination for
                    domestic and international travel.
                </p>
            </HgCardComponent>
        </v-col>
        <v-col
            v-if="showHealthConnectButton"
            :cols="getGridCols"
            class="d-flex"
        >
            <HgCardComponent
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
                    Register on the Health Connect Registry to get a family
                    doctor or nurse practitioner in your community.
                </p>
            </HgCardComponent>
        </v-col>
        <v-col v-if="showOrganDonorButton" :cols="getGridCols" class="d-flex">
            <HgCardComponent
                title="Organ Donor Registration"
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
                    Check whether you are registered as an organ donor with BC
                    Transplant. If you are registered, you can review the
                    details of your decision.
                </p>
            </HgCardComponent>
        </v-col>
        <v-col
            v-if="showRecommendationsCardButton"
            :cols="getGridCols"
            class="d-flex"
        >
            <HgCardComponent
                data-testid="recommendations-card-button"
                class="flex-grow-1"
                @click="showRecommendationsDialog()"
            >
                <template #icon>
                    <img
                        class="quick-link-icon-large"
                        src="@/assets/images/services/immunize-bc-logo.svg"
                        alt="Immunize BC Logo"
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
                    View your recommendations for vaccines.
                </p>
            </HgCardComponent>
        </v-col>
        <v-col v-if="showVaccineCardButton" :cols="getGridCols" class="d-flex">
            <HgCardComponent
                title="BC Vaccine Card"
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
            :cols="getGridCols"
            class="d-flex"
        >
            <HgCardComponent
                :title="card.title"
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
.quick-link-icon {
    height: 1.5em;
    &-large {
        height: 2.1em;
    }
}
</style>
