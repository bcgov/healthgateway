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
                Your information is being collected to provide you with your
                COVID‑19 vaccination status under s. 26(c) of the
                <em>Freedom of Information and Protection of Privacy Act</em>.
                Contact the Ministry Privacy Officer at
                <a
                    href="mailto:MOH.Privacy.Officer@gov.bc.ca"
                    class="text-link"
                >
                    MOH.Privacy.Officer@gov.bc.ca
                </a>
                if you have any questions about this collection.
            </v-card>
        </v-overlay>
    </HgButtonComponent>
</template>
