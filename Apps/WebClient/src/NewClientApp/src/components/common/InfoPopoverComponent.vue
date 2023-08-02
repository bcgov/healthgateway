<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { useAppStore } from "@/stores/app";

const appStore = useAppStore();

interface Props {
    buttonText?: string;
    buttonSize?: string;
    buttonTestId?: string;
    popoverText?: string;
    popoverTestId?: string;
}
withDefaults(defineProps<Props>(), {
    text: undefined,
    buttonText: undefined,
    buttonSize: "small",
    buttonTestId: undefined,
    popoverText: undefined,
    popoverTestId: undefined,
});
</script>

<template>
    <HgButtonComponent
        id="info-popover-button"
        variant="link"
        prepend-icon="info-circle"
        :size="buttonSize"
        :aria-label="buttonText"
        :data-testid="buttonTestId"
    >
        {{ buttonText }}
        <v-overlay
            id="info-popover"
            activator="parent"
            location-strategy="connected"
            scroll-strategy="block"
            :data-testid="popoverTestId"
        >
            <v-card
                class="pa-2 text-body-2"
                :width="appStore.isMobile ? 250 : 472"
            >
                <!-- eslint-disable vue/no-v-html -->
                <div v-html="popoverText" />
                <!-- eslint-enable vue/no-v-html -->
            </v-card>
        </v-overlay>
    </HgButtonComponent>
</template>
