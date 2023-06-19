import { computed, ref } from "vue";
import { useAppStore } from "@/stores/app";
import { ILogger } from "@/services/interfaces";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { defineStore } from "pinia";

export const useNavbarStore = defineStore("navbar", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

    const appStore = useAppStore();

    const isSidebarOpenField = ref<boolean>();
    const isSidebarAnimating = ref<boolean>(false);
    const isHeaderShown = ref<boolean>(true);

    const isSidebarOpen = computed(() => isSidebarOpenField.value ?? !appStore.isMobile);

    function toggleSidebar() {
        logger.verbose(`useNavbarStore:setSidebarState`);
        isSidebarOpenField.value = !isSidebarOpenField.value;
        isSidebarAnimating.value = true;
    }

    function setSidebarStoppedAnimating() {
        logger.verbose(`useNavbarStore:setSidebarStoppedAnimating`);
        isSidebarAnimating.value = false;
    }

    function setHeaderState(isOpen: boolean) {
        logger.verbose(`useNavbarStore:setHeaderState`);
        isHeaderShown.value = isOpen;
    }

    return {
        isSidebarOpen,
        isSidebarAnimating,
        isHeaderShown,
        toggleSidebar,
        setSidebarStoppedAnimating,
        setHeaderState,
    };
});
