<script setup lang="ts">
import { computed, nextTick, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import TooManyRequestsComponent from "@/components/TooManyRequestsComponent.vue";
import { entryTypeMap, getEntryTypeByModule } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ServiceName } from "@/constants/serviceName";
import UserPreferenceType from "@/constants/userPreferenceType";
import { BannerError, isTooManyRequestsError } from "@/models/errors";
import { QuickLink } from "@/models/quickLink";
import User from "@/models/user";
import { UserPreference } from "@/models/userPreference";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";
import PromiseUtility from "@/utility/promiseUtility";

defineExpose({
    showModal,
    hideModal,
});

interface QuickLinkFilter {
    name: string;
    module: string;
    enabled: boolean;
}

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const store = useStore();

const isVisible = ref(false);
const isLoading = ref(false);
const bannerError = ref<BannerError | null>(null);
const selectedQuickLinks = ref<string[]>([]);

const tooManyRequestsError = computed<string | undefined>(
    () => store.getters["errorBanner/tooManyRequestsError"]
);
const quickLinks = computed<QuickLink[] | undefined>(
    () => store.getters["user/quickLinks"]
);
const user = computed<User>(() => store.getters["user/user"]);

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
const isAddButtonEnabled = computed<boolean>(
    () => selectedQuickLinks.value.length > 0 && !isLoading.value
);
const showVaccineCard = computed<boolean>(
    () =>
        user.value.preferences[UserPreferenceType.HideVaccineCardQuickLink]
            ?.value === "true"
);
const showOrganDonorRegistration = computed<boolean>(
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
const showImmunizationRecord = computed<boolean>(
    () =>
        user.value.preferences[
            UserPreferenceType.HideImmunizationRecordQuickLink
        ]?.value === "true"
);

function setTooManyRequestsError(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsError", { key });
}

function clearTooManyRequestsError(): void {
    store.dispatch("errorBanner/clearTooManyRequestsError");
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

function clearErrors(): void {
    bannerError.value = null;
    if (tooManyRequestsError.value === "addQuickLinkModal") {
        clearTooManyRequestsError();
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

    return setUserPreference(preference).then(() => {
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
            updateQuickLinks(user.value.hdid, links),
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
        ].filter((p) => p !== undefined);

        await PromiseUtility.withMinimumDelay(Promise.all(promises), 100);

        // Hide the modal manually
        await nextTick();
        hideModal();
    } catch (error) {
        if (isTooManyRequestsError(error)) {
            setTooManyRequestsError("addQuickLinkModal");
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

function showModal(): void {
    isVisible.value = true;
}

function hideModal(): void {
    isVisible.value = false;
}
</script>

<template>
    <b-modal
        id="add-quick-link-modal"
        v-model="isVisible"
        data-testid="add-quick-link-modal"
        title="Add a quick link"
        size="lg"
        header-bg-variant="primary"
        header-text-variant="light"
        centered
        @close.prevent="handleCancel"
    >
        <form data-testid="quick-link-modal-text">
            <TooManyRequestsComponent location="addQuickLinkModal" />
            <b-alert
                v-if="bannerError"
                data-testid="quick-link-modal-error"
                variant="danger"
                dismissible
                show
            >
                <p>{{ bannerError.title }}</p>
                <span>
                    If you continue to have issues, please contact
                    <a href="mailto:HealthGateway@gov.bc.ca"
                        >HealthGateway@gov.bc.ca</a
                    >.
                </span>
            </b-alert>
            <b-form-checkbox-group v-model="selectedQuickLinks">
                <b-row
                    v-for="quickLink in enabledQuickLinkFilter"
                    :key="quickLink.module"
                >
                    <b-col cols="8" align-self="start">
                        <b-form-checkbox
                            :id="quickLink.module + '-filter'"
                            :data-testid="`${quickLink.module}-filter`"
                            :name="quickLink.module + '-filter'"
                            :value="quickLink.module"
                        >
                            {{ quickLink.name }}
                        </b-form-checkbox>
                    </b-col>
                </b-row>
                <b-row v-if="showOrganDonorRegistration">
                    <b-col cols="8" align-self="start">
                        <b-form-checkbox
                            id="organ-donor-registration-filter"
                            data-testid="organ-donor-registration-filter"
                            name="organ-donor-registration-filter"
                            value="organ-donor-registration"
                        >
                            Organ Donor Registration
                        </b-form-checkbox>
                    </b-col>
                </b-row>
                <b-row v-if="showHealthConnectRegistry">
                    <b-col cols="8" align-self="start">
                        <b-form-checkbox
                            id="health-connect-registry-filter"
                            data-testid="health-connect-registry-filter"
                            name="health-connect-registry-filter"
                            value="health-connect-registry"
                        >
                            Health Connect Registry
                        </b-form-checkbox>
                    </b-col>
                </b-row>
                <b-row v-if="showVaccineCard">
                    <b-col cols="8" align-self="start">
                        <b-form-checkbox
                            id="bc-vaccine-card-filter"
                            data-testid="bc-vaccine-card-filter"
                            name="bc-vaccine-card-filter"
                            value="bc-vaccine-card"
                        >
                            BC Vaccine Card
                        </b-form-checkbox>
                    </b-col>
                </b-row>
                <b-row v-if="showImmunizationRecord">
                    <b-col cols="8" align-self="start">
                        <b-form-checkbox
                            id="immunization-record-filter"
                            data-testid="immunization-record-filter"
                            name="immunization-record-filter"
                            value="immunization-record"
                        >
                            Add Vaccines
                        </b-form-checkbox>
                    </b-col>
                </b-row>
            </b-form-checkbox-group>
        </form>
        <template #modal-footer>
            <b-row>
                <div class="mr-2">
                    <hg-button
                        data-testid="cancel-add-quick-link-btn"
                        variant="secondary"
                        @click.prevent="handleCancel"
                        >Cancel</hg-button
                    >
                </div>
                <div>
                    <hg-button
                        data-testid="add-quick-link-btn"
                        variant="primary"
                        :disabled="!isAddButtonEnabled"
                        @click.prevent="handleSubmit"
                    >
                        <b-spinner v-if="isLoading" small class="mr-2" />
                        <span>Add to Home</span>
                    </hg-button>
                </div>
            </b-row>
        </template>
    </b-modal>
</template>
