<script setup lang="ts">
import { computed, nextTick, ref } from "vue";

import TooManyRequestsComponent from "@/components/TooManyRequestsComponent.vue";
import { entryTypeMap, getEntryTypeByModule } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ServiceName } from "@/constants/serviceName";
import UserPreferenceType from "@/constants/userPreferenceType";
import { BannerError, isTooManyRequestsError } from "@/models/errors";
import { QuickLink } from "@/models/quickLink";
import { ILogger } from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";
import PromiseUtility from "@/utility/promiseUtility";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";
import HgButtonComponent from "@/components/shared/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/shared/HgIconButtonComponent.vue";

interface props {
    disabled?: boolean;
}

withDefaults(defineProps<props>(), {
    disabled: false,
});

defineExpose({
    hideModal,
});

interface QuickLinkFilter {
    name: string;
    module: string;
    enabled: boolean;
}

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const errorStore = useErrorStore();
const userStore = useUserStore();

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
const showOrganDonorRegistration = computed(
    () =>
        ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration) &&
        user.value.preferences[UserPreferenceType.HideOrganDonorQuickLink]
            ?.value === "true"
);
const showImmunizationRecord = computed(
    () =>
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
                "bc-vaccine-card",
                UserPreferenceType.HideVaccineCardQuickLink
            ),
            updateSelectedUserPreference(
                "immunization-record",
                UserPreferenceType.HideImmunizationRecordQuickLink
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
    >
        <template v-slot:activator="{ props }">
            <HgButtonComponent
                id="add-quick-link-button"
                data-testid="add-quick-link-button"
                :disabled="disabled"
                class="float-right"
                variant="secondary"
                v-bind="props"
            >
                <v-icon icon="plus" size="medium" class="mr-2" />
                <span class="text-body-1">Add Quick Link</span>
            </HgButtonComponent>
        </template>
        <v-row justify="center" data-testid="quick-link-modal-text">
            <v-card min-width="20%">
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
                <v-card-text class="pa-3">
                    <TooManyRequestsComponent location="addQuickLinkModal" />
                    <v-alert
                        v-if="bannerError"
                        data-testid="quick-link-modal-error"
                        type="error"
                        closable
                        class="my-3"
                    >
                        <h5 class="text-body-1 font-weight-bold mb-2">
                            {{ bannerError.title }}
                        </h5>
                        <span class="text-body-1">
                            If you continue to have issues, please contact
                            <a href="mailto:HealthGateway@gov.bc.ca"
                                >HealthGateway@gov.bc.ca</a
                            >.
                        </span>
                    </v-alert>
                    <v-checkbox
                        v-for="quickLink in enabledQuickLinkFilter"
                        :key="quickLink.module"
                        :id="quickLink.module + '-filter'"
                        :data-testid="`${quickLink.module}-filter`"
                        :name="quickLink.module + '-filter'"
                        :value="quickLink.module"
                        :label="quickLink.name"
                        v-model="selectedQuickLinks"
                        density="compact"
                        hide-details
                        color="primary"
                    />
                    <v-checkbox
                        v-if="showOrganDonorRegistration"
                        id="organ-donor-registration-filter"
                        data-testid="organ-donor-registration-filter"
                        name="organ-donor-registration-filter"
                        value="organ-donor-registration"
                        label="Organ Donor Registration"
                        v-model="selectedQuickLinks"
                        density="compact"
                        hide-details
                        color="primary"
                    />
                    <v-checkbox
                        v-if="showVaccineCard"
                        id="bc-vaccine-card-filter"
                        data-testid="bc-vaccine-card-filter"
                        name="bc-vaccine-card-filter"
                        value="bc-vaccine-card"
                        label="BC Vaccine Card"
                        v-model="selectedQuickLinks"
                        density="compact"
                        hide-details
                        color="primary"
                    />
                    <v-checkbox
                        v-if="showImmunizationRecord"
                        id="immunization-record-filter"
                        data-testid="immunization-record-filter"
                        name="immunization-record-filter"
                        value="immunization-record"
                        label="Add Vaccines"
                        v-model="selectedQuickLinks"
                        density="compact"
                        hide-details
                        color="primary"
                    />
                </v-card-text>
                <v-card-actions class="justify-end border-t-sm">
                    <HgButtonComponent
                        data-testid="cancel-add-quick-link-btn"
                        variant="secondary"
                        @click.prevent="handleCancel"
                        text="Cancel"
                    />
                    <HgButtonComponent
                        data-testid="add-quick-link-btn"
                        variant="primary"
                        :disabled="!isAddButtonEnabled"
                        @click.prevent="handleSubmit"
                        :loading="isLoading"
                        text="Add to Home"
                    />
                </v-card-actions>
            </v-card>
        </v-row>
    </v-dialog>
</template>
