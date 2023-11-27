<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import TileComponent from "@/components/common/TileComponent.vue";
import { InfoTile } from "@/models/infoTile";
import { useDelegationStore } from "@/stores/delegation";

const delegationStore = useDelegationStore();

const sharingInfoTiles: InfoTile[] = [
    {
        type: "sharingControl",
        name: "You're in control",
        description: "You choose when and what to share.",
        icon: "list-check",
        active: true,
    },
    {
        type: "sharingTransparent",
        name: "Transparent",
        description:
            "Easily see who you are sharing with and what you are sharing.",
        icon: "bell",
        active: true,
    },
    {
        type: "sharingPrivate",
        name: "Private and Secure",
        description: "Your data is only shared with those you choose.",
        icon: "lock",
        active: true,
    },
];
</script>

<template>
    <div data-testid="empty-sharing-page">
        <v-card class="my-4">
            <v-card-text>
                <p class="text-body-1">
                    Sharing your data with those you trust has never been
                    easier.
                </p>
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
        <HgButtonComponent
            class="w-100"
            size="large"
            text="Share with Someone"
            data-testid="start-new-delegation"
            @click="() => delegationStore.startNewDelegation()"
        />
    </div>
</template>
