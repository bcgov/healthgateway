<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import TileComponent from "@/components/site/TileComponent.vue";
import { InfoTile } from "@/models/infoTile";
import { useDelegateStore } from "@/stores/delegate";

const delegateStore = useDelegateStore();

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
</script>

<template>
    <div>
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
        <HgButtonComponent
            class="w-100"
            size="large"
            text="Share with Someone"
            @click="() => delegateStore.startNewInvitation()"
        />
    </div>
</template>
