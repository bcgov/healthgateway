<script setup lang="ts">
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
    faXRay,
} from "@fortawesome/free-solid-svg-icons";
import { computed, watch } from "vue";

import LoadingComponent from "@/components/shared/LoadingComponent.vue";
// import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import {
    EntryTypeDetails,
    entryTypeMap,
    getEntryTypeByModule,
} from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ServiceName } from "@/constants/serviceName";
import UserPreferenceType from "@/constants/userPreferenceType";
import { DateWrapper } from "@/models/dateWrapper";
import { QuickLink } from "@/models/quickLink";
import { LoadStatus } from "@/models/storeOperations";
import { TimelineFilterBuilder } from "@/models/timelineFilter";
import { ILogger } from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import SnowPlow from "@/utility/snowPlow";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { useConfigStore } from "@/stores/config";
import { useVaccinationStatusAuthenticatedStore } from "@/stores/vaccinationStatusAuthenticated";
import { useUserStore } from "@/stores/user";
import { useRouter } from "vue-router";
import { useErrorStore } from "@/stores/error";
import { useTimelineStore } from "@/stores/timeline";
import { saveAs } from "file-saver";
import PageTitleComponent from "@/components/shared/PageTitleComponent.vue";
import HgCardComponent from "@/components/shared/HgCardComponent.vue";
import { useDisplay } from "vuetify";
import AddQuickLinkComponent from "@/components/modal/AddQuickLinkComponent.vue";

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
    faVial,
    faXRay
);

interface QuickLinkCard {
    index: number;
    title: string;
    description: string;
    icon: string;
}

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const router = useRouter();
const display = useDisplay();
const configStore = useConfigStore();
const userStore = useUserStore();
const authenticatedVaccinationStatusStore =
    useVaccinationStatusAuthenticatedStore();
const errorStore = useErrorStore();
const timelineStore = useTimelineStore();

// const sensitivedocumentDownloadModal =
//     ref<InstanceType<typeof MessageModalComponent>>();
// const vaccineRecordResultModal =
//     ref<InstanceType<typeof MessageModalComponent>>();

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
const unverifiedSMS = computed(
    () => !user.value.verifiedSMS && user.value.hasSMS
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
const showVaccineCardButton = computed(
    () => !preferenceVaccineCardHidden.value
);
const showOrganDonorButton = computed(
    () =>
        ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration) &&
        !preferenceOrganDonorHidden.value
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
        !preferenceOrganDonorHidden.value
);

const quickLinkCols = computed(() => {
    const { md, lg, xl } = display;
    return xl.value ? 3 : lg.value ? 4 : md.value ? 6 : 12;
});

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
    // sensitivedocumentDownloadModal.value?.showModal(); // TODO: Reinstate
}

watch(vaccineRecordState, () => {
    if (vaccineRecordState.value.resultMessage.length > 0) {
        // vaccineRecordResultModal.value?.showModal(); // TODO: Reinstate
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
    <v-container>
        <LoadingComponent
            :is-loading="isVaccineRecordDownloading"
            :text="vaccineRecordStatusMessage"
        />
        <v-alert
            v-if="unverifiedEmail || unverifiedSMS"
            data-testid="incomplete-profile-banner"
            show
            dismissible
            type="info"
            class="no-print my-3"
            title="Verify Contact Information"
        >
            <span class="text-body-1">
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
        </v-alert>
        <v-alert
            v-if="!isPacificTime"
            show
            closable
            type="info"
            title="Looks like you're in a different timezone."
            text="Heads up: your health records are recorded and displayed in
                Pacific Time."
            class="no-print my-3"
        />
        <PageTitleComponent title="Home">
            <template #append>
                <AddQuickLinkComponent
                    :disabled="isAddQuickLinkButtonDisabled"
                />
            </template>
        </PageTitleComponent>
        <v-row>
            <v-col :cols="quickLinkCols">
                <HgCardComponent
                    title="Health Records"
                    data-testid="health-records-card"
                    @click="handleClickHealthRecords()"
                    class="h-100"
                    elevation="6"
                >
                    <template #icon>
                        <img
                            class="quick-link-icon align-self-center"
                            src="@/assets/images/gov/health-gateway-logo.svg"
                            alt="Health Gateway Logo"
                        />
                    </template>
                    <p class="text-body-1">
                        View and manage all your available health records,
                        including dispensed medications, health visits, COVID‑19
                        test results, immunizations and more.
                    </p>
                </HgCardComponent>
            </v-col>
            <v-col :cols="quickLinkCols" v-if="showFederalCardButton">
                <HgCardComponent
                    title="Proof of Vaccination"
                    data-testid="proof-vaccination-card-btn"
                    @click="showSensitiveDocumentDownloadModal()"
                    class="h-100"
                    elevation="6"
                >
                    <template #icon>
                        <img
                            class="quick-link-icon align-self-center"
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
            <v-col :cols="quickLinkCols" v-if="showOrganDonorButton">
                <HgCardComponent
                    title="Organ Donor Registration"
                    data-testid="organ-donor-registration-card"
                    @click="handleClickOrganDonorCard()"
                    class="h-100"
                    elevation="6"
                >
                    <template #icon>
                        <img
                            class="quick-link-icon align-self-center"
                            src="@/assets/images/services/odr-logo.svg"
                            alt="Organ Donor Registry Logo"
                        />
                    </template>
                    <template #menu-items>
                        <v-list-item
                            data-testid="remove-quick-link-button"
                            @click.stop="handleClickRemoveOrganDonorQuickLink()"
                            title="Remove"
                        />
                    </template>
                    <p class="text-body-1">
                        Check whether you are registered as an organ donor with
                        BC Transplant. If you are registered, you can review the
                        details of your decision.
                    </p>
                </HgCardComponent>
            </v-col>
            <v-col :cols="quickLinkCols" v-if="showVaccineCardButton">
                <HgCardComponent
                    title="BC Vaccine Card"
                    data-testid="bc-vaccine-card-card"
                    @click="handleClickVaccineCard()"
                    class="h-100"
                    elevation="6"
                >
                    <template #icon>
                        <v-icon icon="check-circle" color="success" />
                    </template>
                    <template #menu-items>
                        <v-list-item
                            data-testid="remove-quick-link-button"
                            @click.stop="
                                handleClickRemoveVaccineCardQuickLink()
                            "
                            title="Remove"
                        />
                    </template>
                    <p class="text-body-1">
                        View, download and print your BC Vaccine Card. Present
                        this card as proof of vaccination at some BC businesses,
                        services and events.
                    </p>
                </HgCardComponent>
            </v-col>
            <v-col
                :cols="quickLinkCols"
                v-for="card in quickLinkCards"
                :key="card.title"
            >
                <HgCardComponent
                    :title="card.title"
                    data-testid="quick-link-card"
                    @click="handleClickQuickLink(card.index)"
                    class="h-100"
                    elevation="6"
                >
                    <template #icon>
                        <v-icon
                            :icon="card.icon"
                            class="quick-link-icon align-self-center"
                            color="primary"
                            size="small"
                        />
                    </template>
                    <template #menu-items>
                        <v-list-item
                            data-testid="remove-quick-link-button"
                            @click.stop="handleClickRemoveQuickLink(card.index)"
                            title="Remove"
                        />
                    </template>
                    <p class="text-body-1">{{ card.description }}</p>
                </HgCardComponent>
            </v-col>
        </v-row>
        <!--        <MessageModalComponent-->
        <!--            ref="sensitivedocumentDownloadModal"-->
        <!--            title="Sensitive Document Download"-->
        <!--            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."-->
        <!--            @submit="retrieveVaccinePdf"-->
        <!--        />-->
        <!--        <MessageModalComponent-->
        <!--            ref="vaccineRecordResultModal"-->
        <!--            ok-only-->
        <!--            title="Alert"-->
        <!--            :message="vaccineRecordResultMessage"-->
        <!--        />-->
    </v-container>
</template>

<style lang="scss" scoped>
.quick-link-icon {
    height: 1.5em;
}
</style>
