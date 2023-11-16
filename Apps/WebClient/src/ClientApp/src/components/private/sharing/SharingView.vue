<!-- Empty Vue component using setup script -->
<script setup lang="ts">
import { computed } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
import TileComponent from "@/components/site/TileComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { InfoTile } from "@/models/infoTile";
import { useDelegateStore } from "@/stores/delegate";

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Sharing",
        to: "/sharing",
        active: true,
        dataTestId: "breadcrumb-sharing",
    },
];

const sharingInfoTiles: InfoTile[] = [
    {
        type: "sharingControl",
        name: "You're in control",
        description: "Placeholder text",
        icon: "list-check",
        active: true,
    },
    {
        type: "sharingTransparent",
        name: "Transparent",
        description: "Placeholder text",
        icon: "bell",
        active: true,
    },
    {
        type: "sharingPrivate",
        name: "Private and Secure",
        description: "Placeholder text",
        icon: "lock",
        active: true,
    },
];

const delegateStore = useDelegateStore();

const invitationsAreLoading = computed(
    () => delegateStore.invitationsAreLoading
);

// TODO: call for invitations
</script>

<template>
    <BreadcrumbComponent :items="breadcrumbItems" />
    <LoadingComponent
        :is-loading="invitationsAreLoading"
        text="Loading invitations"
    />
    <PageTitleComponent title="Sharing" />
    <v-card class="my-4">
        <v-card-text class="">
            <p class="text-body-1">Something goes here</p>
            <v-row>
                <v-col
                    v-for="tile in sharingInfoTiles"
                    :key="tile.type"
                    class="text-center"
                    :data-testid="`infotile-${tile.type}`"
                    cols="12"
                    md="4"
                >
                    <TileComponent
                        class="ma-4"
                        :tile="tile"
                        color="secondary"
                    />
                </v-col>
            </v-row>
        </v-card-text>
    </v-card>
    <HgButtonComponent class="w-100" size="large" text="Share with Someone" />
</template>
