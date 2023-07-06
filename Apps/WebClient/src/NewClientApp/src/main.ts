import { createApp } from "vue";

import App from "@/App.vue";
import AppErrorView from "@/components/error/AppErrorView.vue";
import { AppErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { initializeServices } from "@/ioc/initialization";
import { isTooManyRequestsError } from "@/models/errors";
import { registerInitialPlugins, registerRouterPlugin } from "@/plugins";
import { ILogger } from "@/services/interfaces";
import { useAppStore } from "@/stores/app";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useNotificationStore } from "@/stores/notification";
import { useUserStore } from "@/stores/user";

const app = createApp(App);

registerInitialPlugins(app);

const configStore = useConfigStore();

// Retrieve configuration, initialize services, and mount app
configStore
    .retrieve()
    .then(initializeServices)
    .then(async () => {
        if (window.location.pathname !== "/loginCallback") {
            const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
            // Services are not available until after the services are initialized
            const authStore = useAuthStore();
            const userStore = useUserStore();
            const notificationStore = useNotificationStore();

            const signedIn = await authStore.checkStatus();
            if (signedIn) {
                logger.verbose("User is signed in");
            } else {
                logger.verbose("User is not signed in");
            }
            const isValidIdentityProvider: boolean =
                userStore.isValidIdentityProvider;
            if (userStore.user.hdid && isValidIdentityProvider) {
                await userStore.retrieveEssentialData();
                notificationStore
                    .retrieve()
                    .catch((error) => logger.warn(error.message));
            }
        }
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
