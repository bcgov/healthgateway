import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
import type { App } from "vue";

import vuetify from "@/plugins/vuetify";
import initializeRouter from "@/router";
import pinia from "@/stores";

export function registerInitialPlugins(app: App) {
    app.use(vuetify).use(pinia);
    app.component("FontAwesomeIcon", FontAwesomeIcon);
}

export function registerRouterPlugin(app: App) {
    const router = initializeRouter();
    app.use(router);
}
