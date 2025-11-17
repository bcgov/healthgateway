<script setup lang="ts">
import { InfoTile } from "@/models/infoTile";

interface Props {
    tile: InfoTile;
}
defineProps<Props>();
</script>

<template>
    <div v-if="tile.logoUri || tile.icon" class="ma-4">
        <v-img
            v-if="tile.logoUri"
            :src="tile.logoUri"
            :alt="`logo for ${tile.name}`"
            height="60"
        />
        <v-icon v-else :icon="tile.icon" color="primary" :size="50" />
    </div>
    <h3 class="tile-title text-primary text-h5 font-weight-bold mb-2">
        {{ tile.name }}
    </h3>
    <p class="text-body-1 mb-0">{{ tile.description }}</p>
</template>
