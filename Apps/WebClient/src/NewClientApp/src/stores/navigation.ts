import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger } from "@/services/interfaces";
import { useAppStore } from "@/stores/app";

export const useNavigationStore = defineStore("navigation", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const appStore = useAppStore();

    const isSidebarOpenField = ref<boolean>();
    const isHeaderShown = ref<boolean>(true);

    const isSidebarOpen = computed(
        () => isSidebarOpenField.value ?? !appStore.isMobile
    );

    function toggleSidebar() {
        logger.verbose(`toggleSidebar`);
        isSidebarOpenField.value = !isSidebarOpenField.value;
    }

    function setHeaderState(isOpen: boolean) {
        logger.verbose(`setHeaderState`);
        isHeaderShown.value = isOpen;
    }

    return {
        isSidebarOpen,
        isHeaderShown,
        toggleSidebar,
        setHeaderState,
    };
});
