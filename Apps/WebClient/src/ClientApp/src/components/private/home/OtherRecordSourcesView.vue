<script setup lang="ts">
import { computed } from "vue";

import HgCardComponent from "@/components/common/HgCardComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
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

function trackOtherRecordSourceClick(tile: InfoTile) {
    const resourceLinkType = tile.type as ResourceLinkType;

    if (!(resourceLinkType in ResourceLinkText)) {
        logger.warn(
            `openExternalLink: unsupported resource link type: ${tile.type}`
        );
        return;
    }

    trackingService.trackEvent({
        action: Action.ExternalLink,
        text: ResourceLinkText[resourceLinkType],
        origin: Origin.Landing,
        destination: ResourceLinkDestination[resourceLinkType],
        type: Type.RecordSourceTile,
        url:
            tile.type === AccessLinkType.AccessMyHealth
                ? webClientConfig.value.accessMyHealthUrl
                : tile.link,
    });
}
</script>

<template>
    <BreadcrumbComponent :items="breadcrumbItems" />
    <PageTitleComponent title="Other record sources" />
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
                >
                    <template #icon>
                        <img v-if="tile.logoUri" :src="tile.logoUri" />
                    </template>
                    <p class="text-body-1">{{ tile.description }}</p>
                    <div class="mt-auto pt-3 text-start">
                        <a
                            rel="noopener"
                            target="_blank"
                            class="text-link"
                            :href="
                                tile.type === AccessLinkType.AccessMyHealth
                                    ? webClientConfig.accessMyHealthUrl
                                    : tile.link
                            "
                            :data-testid="`other-record-sources-link-${tile.type}`"
                            @click="trackOtherRecordSourceClick(tile)"
                            >{{ tile.linkText }}</a
                        >
                        <div
                            class="text-body-2 mt-2"
                            :class="{ 'font-weight-bold': !!tile.bottomText }"
                        >
                            {{ tile.bottomText ?? "\u00A0" }}
                        </div>
                    </div>
                </HgCardComponent>
            </v-col>
        </v-row>
        <v-row class="mt-6">
            <v-col cols="12" class="pt-2 text-body-2">
                For more information on these websites and more
                <a
                    :href="ExternalUrl.YourHealthInformation"
                    target="_blank"
                    rel="noopener"
                    class="text-link"
                    @click="
                        trackingService.trackEvent({
                            action: Action.ExternalLink,
                            text: Text.HealthLinkBC,
                            origin: Origin.RecordSources,
                            destination: Destination.HealthLinkBC,
                            url: ExternalUrl.YourHealthInformation,
                        })
                    "
                >
                    visit HealthLink BC </a
                >.
            </v-col>
        </v-row>
    </div>
</template>
