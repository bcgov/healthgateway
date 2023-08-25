<!-- Empty vue 3 component using setup script in typescript -->
<script setup lang="ts">
import { InfoTile } from "@/models/infoTile";

interface Props {
    tile: InfoTile;
}

defineProps<Props>();
</script>

<template>
    <div class="ma-4" style="height: 60px">
        <v-img
            v-if="tile.logoUrl"
            :src="tile.logoUrl"
            :alt="`logo for ${tile.name}`"
        />
        <v-icon v-else :icon="tile.icon" color="primary" :size="50" />
    </div>
    <h3 class="text-primary text-h5 font-weight-bold mb-2">
        {{ tile.name }}
    </h3>
    <p class="text-body-1 mb-0">{{ tile.description }}</p>
</template>
