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
import { saveAs } from "file-saver";
import { computed, ref, watch } from "vue";
import { useRouter, useStore } from "vue-composition-wrapper";

import LoadingComponent from "@/components/LoadingComponent.vue";
import AddQuickLinkComponent from "@/components/modal/AddQuickLinkComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import {
    EntryTypeDetails,
    entryTypeMap,
    getEntryTypeByModule,
} from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ServiceName } from "@/constants/serviceName";
import UserPreferenceType from "@/constants/userPreferenceType";
import type { WebClientConfiguration } from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper } from "@/models/dateWrapper";
import { QuickLink } from "@/models/quickLink";
import { LoadStatus } from "@/models/storeOperations";
import { TimelineFilterBuilder } from "@/models/timelineFilter";
import User from "@/models/user";
import { UserPreference } from "@/models/userPreference";
import VaccineRecordState from "@/models/vaccineRecordState";
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
const store = useStore();

const sensitivedocumentDownloadModal =
    ref<InstanceType<typeof MessageModalComponent>>();
const vaccineRecordResultModal =
    ref<InstanceType<typeof MessageModalComponent>>();
const addQuickLinkModal = ref<AddQuickLinkComponent>();

const config = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);
const user = computed<User>(() => store.getters["user/user"]);
const quickLinks = computed<QuickLink[] | undefined>(
    () => store.getters["user/quickLinks"]
);
const vaccineRecordState = computed<VaccineRecordState>(() =>
    store.getters["vaccinationStatus/authenticatedVaccineRecordState"](
        user.value.hdid
    )
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
        config.value.featureToggleConfiguration.homepage
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
const preferenceHealthConnectHidden = computed(
    () =>
        user.value.preferences[
            UserPreferenceType.HideHealthConnectRegistryQuickLink
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
        !preferenceHealthConnectHidden.value
);

function addError(
    errorType: ErrorType,
    source: ErrorSourceType,
    traceId: string | undefined
): void {
    store.dispatch("errorBanner/addError", { errorType, source, traceId });
}

function setTooManyRequestsError(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsError", { key });
}

function retrieveAuthenticatedVaccineRecord(
    hdid: string
): Promise<CovidVaccineRecord> {
    return store.dispatch(
        "vaccinationStatus/retrieveAuthenticatedVaccineRecord",
        { hdid }
    );
}

function stopAuthenticatedVaccineRecordDownload(hdid: string): void {
    store.dispatch("vaccinationStatus/stopAuthenticatedVaccineRecordDownload", {
        hdid,
    });
}

function setFilter(filterBuilder: TimelineFilterBuilder): void {
    store.dispatch("timeline/setFilter", filterBuilder);
}

function updateQuickLinks(
    hdid: string,
    quickLinks: QuickLink[]
): Promise<void> {
    return store.dispatch("user/updateQuickLinks", { hdid, quickLinks });
}

function setUserPreference(preference: UserPreference): Promise<void> {
    return store.dispatch("user/setUserPreference", { preference });
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

    return updateQuickLinks(user.value.hdid, updatedLinks).catch((error) => {
        logger.error(error);
        if (error.statusCode === 429) {
            setTooManyRequestsError("page");
        } else {
            addError(ErrorType.Update, ErrorSourceType.Profile, undefined);
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

function handleClickHealthConnectCard(): void {
    trackClickLink("home_primarycare");
    window.location.replace(
        "https://www.healthlinkbc.ca/health-connect-registry"
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

function setPreferenceValue(preferenceType: string, value: string) {
    const preference = {
        ...user.value.preferences[preferenceType],
        value,
    };

    setUserPreference(preference).catch((error) => {
        logger.error(error);
        if (error.statusCode === 429) {
            setTooManyRequestsError("page");
        } else {
            addError(ErrorType.Update, ErrorSourceType.Profile, undefined);
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

    setFilter(builder);
    router.push({ path: "/timeline" });
}

function showSensitiveDocumentDownloadModal(): void {
    sensitivedocumentDownloadModal.value?.showModal();
}

function showAddQuickLinkModal(): void {
    addQuickLinkModal.value?.showModal();
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
        </page-title>
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
            <b-col v-if="showHealthConnectButton" class="p-3">
                <hg-card-button
                    data-testid="health-connect-registry-card"
                    @click="handleClickHealthConnectCard()"
                >
                    <template #icon>
                        <img
                            class="health-connect-registry-logo align-self-center"
                            src="@/assets/images/services/health-link-logo.svg"
                            alt="Health Connect Registry Logo"
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
                                        handleClickRemoveHealthConnectCard()
                                    "
                                >
                                    Remove
                                </b-dropdown-item>
                            </b-nav-item-dropdown>
                        </b-nav>
                    </template>
                    <div>
                        Register on the Health Connect Registry to get a family
                        doctor or nurse practitioner in your community.
                    </div>
                </hg-card-button>
            </b-col>
            <b-col v-if="showOrganDonorButton" class="p-3">
                <hg-card-button
                    title="Organ Donor Registration"
                    data-testid="organ-donor-registration-card"
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
                        Check whether you are registered as an organ donor with
                        BC Transplant. If you are registered, you can review the
                        details of your decision.
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
