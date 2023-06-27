import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";

// Plugins
import vuetify from "@/plugins/vuetify";
import pinia from "@/stores";
import router from "@/router";

// Types
import type { App } from "vue";
import { vMaska } from "maska";

export function registerInitialPlugins(app: App) {
    app.use(vuetify).use(pinia);
    app.component("font-awesome-icon", FontAwesomeIcon);
    app.directive("maska", vMaska);
}

export function registerRouterPlugin(app: App) {
    app.use(router);
}
