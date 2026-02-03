<script setup lang="ts">
import { computed } from "vue";
import { ref } from "vue";

import HgCardComponent from "@/components/common/HgCardComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
import ExternalLinkConfirmationDialog from "@/components/site/ExternalLinkConfirmationDialog.vue";
import {
    AccessLinkType,
    getOtherRecordSourcesLinks,
} from "@/constants/accessLinks";
import { CardName } from "@/constants/cardName";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { InfoTile } from "@/models/infoTile";
import {
    Action,
    Destination,
    ExternalUrl,
    InternalUrl,
    Origin,
    ResourceLinkDestination,
    ResourceLinkText,
    ResourceLinkType,
    Text,
    Type,
} from "@/plugins/extensions";
import { ILogger, ITrackingService } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import ConfigUtil from "@/utility/configUtil";
import { useGrid } from "@/utility/useGrid";

const isExternalLinkDialogOpen = ref(false);
const pendingAction = ref<Action | undefined>(undefined);
const pendingTile = ref<InfoTile | undefined>(undefined);

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Other record sources",
        to: "/otherRecordSources",
        active: true,
        dataTestId: "breadcrumb-other-record-sources",
    },
];

const { columns } = useGrid();

const configStore = useConfigStore();

const webClientConfig = computed(() => configStore.webConfig);

const showAccessMyHealthCard = computed(() =>
    ConfigUtil.isOtherRecordSourcesCardEnabled(CardName.AccessMyHealth)
);

const otherRecordSourcesLinks = computed(() => {
    const list = getOtherRecordSourcesLinks();
    return list.filter((t) =>
        t.type === AccessLinkType.AccessMyHealth
            ? showAccessMyHealthCard.value
            : true
    );
});

function getRecordSourceUrl(tile: InfoTile): string | undefined {
    return tile.type === AccessLinkType.AccessMyHealth
        ? webClientConfig.value.accessMyHealthUrl
        : tile.link;
}

function handleRecordSourceClick(tile: InfoTile, text: Text): void {
    const url = getRecordSourceUrl(tile);
    if (!url) return;

    // Always track the tile click
    trackRecordSourceClick(tile, text, url);

    // AccessMyHealth tile prompts dialog
    if (tile.type === AccessLinkType.AccessMyHealth) {
        pendingTile.value = tile;
        pendingAction.value = Action.ExternalLink;
        isExternalLinkDialogOpen.value = true;
        return;
    }

    // All other tiles: open immediately
    window.open(url, "_blank", "noopener");
}

function cancelExternalNavigation(): void {
    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.AccessMyHealthDialogCancel,
        origin: Origin.OtherRecordSources,
        type: Type.RecordSourceTile,
        url: InternalUrl.OtherRecordSources,
    });

    pendingTile.value = undefined;
    isExternalLinkDialogOpen.value = false;
}

function confirmExternalNavigation(): void {
    const tile = pendingTile.value;
    const action = pendingAction.value;

    if (!tile || !action) return;

    const url = getRecordSourceUrl(tile);
    if (!url) return;

    window.open(url, "_blank", "noopener");

    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.AccessMyHealthDialogSignin,
        origin: Origin.OtherRecordSources,
        destination: Destination.AccessMyHealth,
        type: Type.RecordSourceTile,
        url,
    });

    pendingTile.value = undefined;
    isExternalLinkDialogOpen.value = false;
}

function trackRecordSourceClick(tile: InfoTile, text: Text, url: string) {
    const resourceLinkType = tile.type as ResourceLinkType;

    if (!(resourceLinkType in ResourceLinkText)) {
        logger.warn(
            `openExternalLink: unsupported resource link type: ${tile.type}`
        );
        return;
    }

    trackingService.trackEvent({
        action: Action.ExternalLink,
        text: text,
        origin: Origin.OtherRecordSources,
        destination: ResourceLinkDestination[resourceLinkType],
        type: Type.RecordSourceTile,
        url: url,
    });
}
</script>

<template>
    <BreadcrumbComponent :items="breadcrumbItems" />
    <PageTitleComponent title="Other record sources" />
    <ExternalLinkConfirmationDialog
        v-model="isExternalLinkDialogOpen"
        title="Open AccessMyHealth"
        :body="[
            'This will sign you into AccessMyHealth directly. You must have at least one record in AccessMyHealth for this to work.',
            {
                prefix: 'To find out more, visit ',
                text: ExternalUrl.AccessMyHealth,
                trackingText: Text.AccessMyHealthDialogUrl,
                href: ExternalUrl.AccessMyHealth,
                suffix: '.',
            },
        ]"
        confirm-label="Sign in"
        cancel-label="Cancel"
        :origin="Origin.OtherRecordSources"
        :tracking-text="Text.AccessMyHealthDialogUrl"
        @confirm="confirmExternalNavigation"
        @cancel="cancelExternalNavigation"
    />
    <p>
        Health Gateway helps bring your records together in one place. It
        connects to many record sources, but not all. Included below are trusted
        regional patient websites where you can find more records and health
        services.
    </p>
    <div class="mt-4 mt-md-6">
        <v-row>
            <v-col
                v-for="tile in otherRecordSourcesLinks"
                :key="tile.name"
                :cols="columns"
                class="d-flex"
            >
                <HgCardComponent
                    variant="outlined"
                    elevation="1"
                    border="thin grey-lighten-2"
                    class="flex-grow-1"
                    compact-header
                    fill-body
                    :title="tile.name"
                    :data-testid="`other-record-sources-card-${tile.type}`"
                    @click="
                        handleRecordSourceClick(tile, Text.AccessMyHealthTile)
                    "
                >
                    <template #icon>
                        <img v-if="tile.logoUri" :src="tile.logoUri" />
                    </template>
                    <p class="text-body-1">{{ tile.description }}</p>
                    <div class="mt-auto pt-3 text-start">
                        <a
                            class="text-link"
                            role="link"
                            :data-testid="`other-record-sources-link-${tile.type}`"
                            @click.prevent.stop="
                                handleRecordSourceClick(
                                    tile,
                                    Text.AccessMyHealthURL
                                )
                            "
                            >{{ tile.linkText }}</a
                        >
                    </div>
                </HgCardComponent>
            </v-col>
        </v-row>
        <v-row class="mt-6">
            <v-col cols="12" class="pt-2 text-body-2">
                For more information on these websites and more visit
                <a
                    :href="ExternalUrl.YourHealthInformation"
                    target="_blank"
                    rel="noopener"
                    class="text-link"
                    @click="
                        trackingService.trackEvent({
                            action: Action.ExternalLink,
                            text: Text.HealthLinkBC,
                            origin: Origin.OtherRecordSources,
                            destination: Destination.HealthLinkBC,
                            url: ExternalUrl.YourHealthInformation,
                        })
                    "
                >
                    HealthLink BC</a
                >.
            </v-col>
        </v-row>
    </div>
</template>
