<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { useAppStore } from "@/stores/app";

const appStore = useAppStore();
</script>

<template>
    <HgButtonComponent
        id="privacy-statement"
        variant="link"
        size="small"
        prepend-icon="info-circle"
        aria-label="Privacy Statement"
    >
        Privacy Statement
        <v-overlay
            activator="parent"
            location-strategy="connected"
            scroll-strategy="block"
        >
            <v-card
                class="pa-2 text-body-2"
                :width="appStore.isMobile ? 250 : 472"
            >
                <slot />
            </v-card>
        </v-overlay>
    </HgButtonComponent>
</template>
