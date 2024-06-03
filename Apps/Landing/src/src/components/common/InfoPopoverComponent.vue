<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { useLayoutStore } from "@/stores/layout";

const layoutStore = useLayoutStore();

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
    buttonTestId: "info-popover-button",
    popoverText: undefined,
    popoverTestId: "info-popover",
});
</script>

<template>
    <HgButtonComponent
        :id="buttonId"
        variant="transparent"
        class="text-primary"
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
                :width="layoutStore.isMobile ? 250 : 472"
            >
                <slot />
            </v-card>
        </v-overlay>
    </HgButtonComponent>
</template>
