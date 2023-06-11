import { WebClientConfiguration } from "@/models/configData";
import { ref } from "vue";
import { defineStore } from "pinia";

export const useConfigStore = defineStore("config", () => {
    const webConfig = ref<WebClientConfiguration>({} as WebClientConfiguration);

    return {
        webConfig,
    };
});
