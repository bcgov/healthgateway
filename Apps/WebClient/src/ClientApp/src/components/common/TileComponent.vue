<script setup lang="ts">
import { InfoTile } from "@/models/infoTile";

interface Props {
    tile: InfoTile;
    color?: string;
}
withDefaults(defineProps<Props>(), {
    color: "primary",
});
</script>

<template>
    <div>
        <div class="ma-4" style="height: 60px">
            <v-img
                v-if="tile.logoUri"
                :src="tile.logoUri"
                :alt="`logo for ${tile.name}`"
            />
            <v-icon v-else :icon="tile.icon" :color="color" :size="50" />
        </div>
        <h3 class="text-primary text-h5 font-weight-bold mb-2">
            {{ tile.name }}
        </h3>
        <p class="text-body-1">{{ tile.description }}</p>
    </div>
</template>
