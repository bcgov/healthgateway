import { createApp } from "vue";

import App from "@/App.vue";
import AppErrorView from "@/components/error/AppErrorView.vue";
import { AppErrorType } from "@/constants/errorType";
import { initializeServices } from "@/ioc/initialization";
import { isTooManyRequestsError } from "@/models/errors";
import { registerInitialPlugins, registerRouterPlugin } from "@/plugins";
import { useAppStore } from "@/stores/app";
import { useConfigStore } from "@/stores/config";

const app = createApp(App);

registerInitialPlugins(app);

const configStore = useConfigStore();

// Retrieve configuration, initialize services, and mount app
configStore
    .retrieve()
    .then(initializeServices)
    .then(async () => {
        registerRouterPlugin(app);
        app.mount("#app-root");
    })
    .catch((error) => {
        // logger may not be initialized yet
        console.error(`An error occurred while initializing the app`, error);
        let errorType = AppErrorType.General;
        if (isTooManyRequestsError(error)) {
            errorType = AppErrorType.TooManyRequests;
        }

        const appStore = useAppStore();
        appStore.setAppError(errorType);

        const errorApp = createApp(AppErrorView);
        registerInitialPlugins(errorApp);
        errorApp.mount("#app-root");
    });
