import { defineStore } from "pinia";
import { computed, ref } from "vue";
import { useDisplay } from "vuetify";

import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger } from "@/services/interfaces";

export const useLayoutStore = defineStore("layout", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

    const isSidebarOpenField = ref<boolean>();
    const isHeaderShown = ref<boolean>(true);
    const display = useDisplay();
    const isMobile = computed(() => !display.mdAndUp.value);

    const isSidebarOpen = computed(
        () => isSidebarOpenField.value ?? !isMobile.value
    );

    function toggleSidebar() {
        logger.verbose(`toggleSidebar`);
        // default state for desktop is open, this ensures the first click toggles the sidebar correctly.
        if (isSidebarOpenField.value === undefined && !isMobile.value) {
            isSidebarOpenField.value = false;
        } else {
            isSidebarOpenField.value = !isSidebarOpenField.value;
        }
    }

    function setHeaderState(isOpen: boolean) {
        logger.verbose(`setHeaderState`);
        isHeaderShown.value = isOpen;
    }

    return {
        isMobile,
        isSidebarOpen,
        isHeaderShown,
        toggleSidebar,
        setHeaderState,
    };
});
