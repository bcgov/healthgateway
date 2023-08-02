<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { useAppStore } from "@/stores/app";

const appStore = useAppStore();

interface Props {
    buttonId?: string;
    buttonPrependIcon?: string;
    buttonText?: string;
    buttonSize?: string;
    buttonTestId?: string;
    popoverText?: string;
    popoverTestId?: string;
}
withDefaults(defineProps<Props>(), {
    buttonId: "info-popover-button",
    buttonPrependIcon: "info-circle",
    buttonText: undefined,
    buttonSize: "small",
    buttonTestId: undefined,
    popoverText: undefined,
    popoverTestId: undefined,
});
</script>

<template>
    <HgButtonComponent
        :id="buttonId"
        variant="link"
        :prepend-icon="buttonPrependIcon"
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
                <slot />
            </v-card>
        </v-overlay>
    </HgButtonComponent>
</template>
