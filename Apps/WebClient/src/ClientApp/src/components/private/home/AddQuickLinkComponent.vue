<script setup lang="ts">
import { computed, nextTick, ref } from "vue";

import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import TooManyRequestsComponent from "@/components/error/TooManyRequestsComponent.vue";
import {
    EntryType,
    entryTypeMap,
    getEntryTypeByModule,
} from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ServiceName } from "@/constants/serviceName";
import UserPreferenceType from "@/constants/userPreferenceType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { BannerError, isTooManyRequestsError } from "@/models/errors";
import { QuickLink } from "@/models/quickLink";
import { ILogger } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";
import PromiseUtility from "@/utility/promiseUtility";

interface Props {
    disabled?: boolean;
}
withDefaults(defineProps<Props>(), {
    disabled: false,
});

defineExpose({ hideModal });

interface QuickLinkFilter {
    name: string;
    module: string;
    enabled: boolean;
}

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const errorStore = useErrorStore();
const userStore = useUserStore();
const configStore = useConfigStore();

const isVisible = ref(false);
const isLoading = ref(false);
const bannerError = ref<BannerError | null>(null);
const selectedQuickLinks = ref<string[]>([]);

const quickLinks = computed(() => userStore.quickLinks);
const user = computed(() => userStore.user);

const quickLinkFilter = computed<QuickLinkFilter[]>(() =>
    [...entryTypeMap.values()].map((details) => ({
        name: details.name,
        module: details.moduleName,
        enabled: ConfigUtil.isDatasetEnabled(details.type),
    }))
);
const enabledQuickLinkFilter = computed(() =>
    quickLinkFilter.value.filter(
        (filter) =>
            filter.enabled &&
            quickLinks.value?.find(
                (existingLink) =>
                    existingLink.filter.modules.length === 1 &&
                    existingLink.filter.modules[0] === filter.module
            ) === undefined
    )
);
const isAddButtonEnabled = computed(
    () => selectedQuickLinks.value.length > 0 && !isLoading.value
);
const showVaccineCard = computed(
    () =>
        user.value.preferences[UserPreferenceType.HideVaccineCardQuickLink]
            ?.value === "true"
);
const showRecommendationsDialog = computed(
    () =>
        ConfigUtil.isDatasetEnabled(EntryType.Immunization) &&
        configStore.webConfig.featureToggleConfiguration.homepage
            .showRecommendationsLink &&
        user.value.preferences[UserPreferenceType.HideRecommendationsQuickLink]
            ?.value === "true"
);
const showOrganDonorRegistration = computed(
    () =>
        ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration) &&
        user.value.preferences[UserPreferenceType.HideOrganDonorQuickLink]
            ?.value === "true"
);
const showHealthConnectRegistry = computed<boolean>(
    () =>
        ConfigUtil.isServiceEnabled(ServiceName.HealthConnectRegistry) &&
        user.value.preferences[
            UserPreferenceType.HideHealthConnectRegistryQuickLink
        ]?.value === "true"
);
const showImmunizationRecord = computed(
    () =>
        ConfigUtil.isDatasetEnabled(EntryType.Immunization) &&
        ConfigUtil.isImmunizationRecordLinkEnabled() &&
        user.value.preferences[
            UserPreferenceType.HideImmunizationRecordQuickLink
        ]?.value === "true"
);

function clearErrors(): void {
    bannerError.value = null;
    if (errorStore.tooManyRequestsError === "addQuickLinkModal") {
        errorStore.clearTooManyRequestsError();
    }
}

async function handleCancel(): Promise<void> {
    clearErrors();

    // Clear selected quick links
    selectedQuickLinks.value = [];

    // Hide the modal manually
    await nextTick();
    hideModal();
}

function updateSelectedUserPreference(
    quickLinkName: string,
    preferenceType: string,
    preferenceValue = "false"
): Promise<void> | undefined {
    if (!selectedQuickLinks.value.includes(quickLinkName)) {
        return undefined;
    }

    const preference = {
        ...user.value.preferences[preferenceType],
        value: preferenceValue,
    };

    return userStore.setUserPreference(preference).then(() => {
        selectedQuickLinks.value = selectedQuickLinks.value.filter(
            (link) => link !== quickLinkName
        );
    });
}

async function handleSubmit(): Promise<void> {
    clearErrors();

    selectedQuickLinks.value.forEach((element) =>
        logger.debug(`Adding quick link:  ${element}`)
    );

    const links: QuickLink[] = quickLinks.value ?? [];
    selectedQuickLinks.value.forEach((module) => {
        const details = getEntryTypeByModule(module);
        if (details) {
            links.push({
                name: details.name,
                filter: { modules: [module] },
            });
        }
    });

    try {
        isLoading.value = true;

        const promises = [
            userStore.updateQuickLinks(user.value.hdid, links),
            updateSelectedUserPreference(
                "organ-donor-registration",
                UserPreferenceType.HideOrganDonorQuickLink
            ),
            updateSelectedUserPreference(
                "health-connect-registry",
                UserPreferenceType.HideHealthConnectRegistryQuickLink
            ),
            updateSelectedUserPreference(
                "bc-vaccine-card",
                UserPreferenceType.HideVaccineCardQuickLink
            ),
            updateSelectedUserPreference(
                "immunization-record",
                UserPreferenceType.HideImmunizationRecordQuickLink
            ),
            updateSelectedUserPreference(
                "recommendations-dialog",
                UserPreferenceType.HideRecommendationsQuickLink
            ),
        ].filter((p) => p !== undefined);

        await PromiseUtility.withMinimumDelay(Promise.all(promises), 100);

        // Hide the modal manually
        await nextTick();
        hideModal();
    } catch (error) {
        if (isTooManyRequestsError(error)) {
            errorStore.setTooManyRequestsError("addQuickLinkModal");
        } else {
            bannerError.value = ErrorTranslator.toBannerError(
                ErrorType.Update,
                ErrorSourceType.QuickLinks,
                undefined
            );
        }
    } finally {
        // Clear selected quick links
        selectedQuickLinks.value = [];

        isLoading.value = false;
    }
}

function isSelectedVariant(linkName: string): "elevated" | "outlined" {
    return selectedQuickLinks.value.includes(linkName)
        ? "elevated"
        : "outlined";
}

function hideModal(): void {
    isVisible.value = false;
}
</script>

<template>
    <v-dialog
        id="add-quick-link-modal"
        v-model="isVisible"
        data-testid="add-quick-link-modal"
        persistent
        no-click-animation
        max-width="500px"
    >
        <template #activator="slotProps">
            <HgButtonComponent
                id="add-quick-link-button"
                data-testid="add-quick-link-button"
                :disabled="disabled"
                variant="secondary"
                v-bind="slotProps.props"
                prepend-icon="plus"
                text="Add Quick Link"
            />
        </template>
        <div class="d-flex justify-center" data-testid="quick-link-modal-text">
            <v-card>
                <v-card-title class="bg-primary text-white px-0">
                    <v-toolbar
                        title="Add a quick link"
                        density="compact"
                        color="primary"
                    >
                        <HgIconButtonComponent
                            id="add-quick-link-modal-close-button"
                            data-testid="add-quick-link-modal-close-button"
                            icon="fas fa-close"
                            @click="handleCancel"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text class="pa-4">
                    <TooManyRequestsComponent location="addQuickLinkModal" />
                    <HgAlertComponent
                        v-if="bannerError"
                        data-testid="quick-link-modal-error"
                        type="error"
                        closable
                        class="my-3"
                        variant="outlined"
                    >
                        <h5 class="text-body-1 font-weight-bold mb-2">
                            {{ bannerError.title }}
                        </h5>
                        <span class="text-body-1">
                            If you continue to have issues, please contact
                            <a
                                href="mailto:HealthGateway@gov.bc.ca"
                                class="text-link"
                                >HealthGateway@gov.bc.ca</a
                            >.
                        </span>
                    </HgAlertComponent>
                    <v-chip-group v-model="selectedQuickLinks" column multiple>
                        <v-chip
                            v-for="quickLink in enabledQuickLinkFilter"
                            :id="`${quickLink.module}-filter`"
                            :key="quickLink.module"
                            :data-testid="`${quickLink.module}-filter`"
                            :name="`${quickLink.module}-filter`"
                            :value="quickLink.module"
                            :text="quickLink.name"
                            color="primary"
                            :variant="isSelectedVariant(quickLink.module)"
                        />
                        <v-chip
                            v-if="showOrganDonorRegistration"
                            id="organ-donor-registration-filter"
                            data-testid="organ-donor-registration-filter"
                            name="organ-donor-registration-filter"
                            value="organ-donor-registration"
                            text="Organ Donor Registration"
                            color="primary"
                            :variant="
                                isSelectedVariant('organ-donor-registration')
                            "
                        />
                        <v-chip
                            v-if="showHealthConnectRegistry"
                            id="health-connect-registry-filter"
                            data-testid="health-connect-registry-filter"
                            name="health-connect-registry-filter"
                            value="health-connect-registry"
                            text="Health Connect Registry"
                            color="primary"
                            :variant="
                                isSelectedVariant('health-connect-registry')
                            "
                        />
                        <v-chip
                            v-if="showVaccineCard"
                            id="bc-vaccine-card-filter"
                            data-testid="bc-vaccine-card-filter"
                            name="bc-vaccine-card-filter"
                            value="bc-vaccine-card"
                            text="BC Vaccine Card"
                            color="primary"
                            :variant="isSelectedVariant('bc-vaccine-card')"
                        />
                        <v-chip
                            v-if="showImmunizationRecord"
                            id="immunization-record-filter"
                            data-testid="immunization-record-filter"
                            name="immunization-record-filter"
                            value="immunization-record"
                            text="Immunization Record"
                            color="primary"
                            :variant="isSelectedVariant('immunization-record')"
                        />
                        <v-chip
                            v-if="showRecommendationsDialog"
                            id="recommendations-dialog-filter"
                            data-testid="recommendations-dialog-filter"
                            name="recommendations-dialog-filter"
                            value="recommendations-dialog"
                            text="Vaccine Recommendations"
                            color="primary"
                            :variant="
                                isSelectedVariant('recommendations-dialog')
                            "
                        />
                    </v-chip-group>
                </v-card-text>
                <v-card-actions class="justify-end border-t-sm pa-4">
                    <HgButtonComponent
                        data-testid="cancel-add-quick-link-btn"
                        variant="secondary"
                        text="Cancel"
                        @click.prevent="handleCancel"
                    />
                    <HgButtonComponent
                        data-testid="add-quick-link-btn"
                        variant="primary"
                        :disabled="!isAddButtonEnabled"
                        :loading="isLoading"
                        text="Save"
                        @click.prevent="handleSubmit"
                    />
                </v-card-actions>
            </v-card>
        </div>
    </v-dialog>
</template>
