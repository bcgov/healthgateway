import { defineStore } from "pinia";
import { computed, ref } from "vue";
import { useDisplay } from "vuetify";

import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger } from "@/services/interfaces";

export const useLayoutStore = defineStore("layout", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

    const isHeaderShown = ref<boolean>(true);
    const display = useDisplay();
    const isMobile = computed(() => !display.mdAndUp.value);

    function setHeaderState(isOpen: boolean) {
        logger.verbose(`setHeaderState`);
        isHeaderShown.value = isOpen;
    }

    return {
        isMobile,
        isHeaderShown,
        setHeaderState,
    };
});
