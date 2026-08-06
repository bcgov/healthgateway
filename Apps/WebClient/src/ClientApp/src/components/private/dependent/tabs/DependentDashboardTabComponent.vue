<script setup lang="ts">
import { computed, ref } from "vue";
import { useRouter } from "vue-router";

import HgCardComponent from "@/components/common/HgCardComponent.vue";
import RecommendationsDialogComponent from "@/components/private/reports/RecommendationsDialogComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import type { Dependent } from "@/models/dependent";
import { Action, Destination, Origin, Text } from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { useGrid } from "@/utility/useGrid";

interface Props {
    dependent: Dependent;
}
const props = defineProps<Props>();

const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const configStore = useConfigStore();

const router = useRouter();
const { columns } = useGrid();

const recommendationsDialogComponent =
    ref<InstanceType<typeof RecommendationsDialogComponent>>();

const showRecommendations = computed(
    () =>
        configStore.webConfig.featureToggleConfiguration.homepage
            .showRecommendationsLink
);

function handleClickHealthRecordsButton(): void {
    trackingService.trackEvent({
        action: Action.Visit,
        text: Text.InternalLink,
        destination: Destination.Timeline,
        origin: Origin.Dependents,
    });
    router.push({
        path: `/dependents/${props.dependent.ownerId}/timeline`,
    });
}

function showRecommendationsDialog(): void {
    trackingService.trackEvent({
        action: Action.Visit,
        text: Text.InternalLink,
        destination: Destination.ImmunizationRecommendationDialog,
        origin: Origin.Dependents,
    });
    recommendationsDialogComponent.value?.showDialog();
}
</script>

<template>
    <v-row>
        <v-col :cols="columns" class="d-flex">
            <HgCardComponent
                title="Health Records"
                density="compact"
                class="flex-grow-1 ma-1"
                :data-testid="`dependent-health-records-button-${dependent.ownerId}`"
                @click="handleClickHealthRecordsButton"
            >
                <template #icon>
                    <img
                        src="@/assets/images/gov/health-gateway-logo.svg"
                        alt="Health Gateway Logo"
                        :height="25"
                    />
                </template>
                <template #action-icon>
                    <v-icon icon="chevron-right" color="primary" size="small" />
                </template>
            </HgCardComponent>
        </v-col>
        <v-col v-if="showRecommendations" :cols="columns" class="d-flex">
            <HgCardComponent
                title="Vaccine Recommendations"
                density="compact"
                class="flex-grow-1 ma-1"
                :data-testid="`recommendations-card-${dependent.ownerId}`"
                @click="showRecommendationsDialog()"
            >
                <template #icon>
                    <v-icon
                        icon="calendar-check"
                        color="primary"
                        size="small"
                    />
                </template>
            </HgCardComponent>
        </v-col>
    </v-row>
    <RecommendationsDialogComponent
        ref="recommendationsDialogComponent"
        :hdid="dependent.ownerId"
        :is-dependent="true"
    />
</template>
