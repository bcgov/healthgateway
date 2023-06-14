// Components
import App from "@/App.vue";

// Composables
import { createApp } from "vue";

// Plugins
import { registerPlugins } from "@/plugins";
import { AppErrorType } from "@/constants/errorType";
import { isTooManyRequestsError } from "@/models/errors";
import { useConfigStore } from "@/stores/config";
import { useAppStore } from "@/stores/app";
import { initializeServices } from "@/ioc/initialization";

const app = createApp(App);

registerPlugins(app);

const configStore = useConfigStore();

// Retrieve configuration, initialize services, and mount app
configStore
    .retrieve()
    .then(initializeServices)
    .then(async () => {
        if (window.location.pathname !== "/loginCallback") {
            // const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
            // const signedIn = await store.dispatch("auth/checkStatus");
            // if (signedIn) {
            //     logger.verbose("User is signed in");
            // } else {
            //     logger.verbose("User is not signed in");
            // }
            // const isValidIdentityProvider: boolean =
            //     store.getters["user/isValidIdentityProvider"];
            // const user: User = store.getters["user/user"];
            // if (user.hdid && isValidIdentityProvider) {
            //     await store.dispatch("user/retrieveEssentialData");
            //     store
            //         .dispatch("notification/retrieve")
            //         .catch((error) => logger.warn(error.message));
            // }
        }
    })
    .catch((error) => {
        let errorType = AppErrorType.General;
        if (isTooManyRequestsError(error)) {
            errorType = AppErrorType.TooManyRequests;
        }

        const appStore = useAppStore();
        appStore.setAppError(errorType);
    })
    .finally(() => app.mount("#app-root"));
