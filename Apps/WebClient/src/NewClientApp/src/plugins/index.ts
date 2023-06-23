import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";

// Plugins
import vuetify from "@/plugins/vuetify";
import pinia from "@/stores";
import router from "@/router";

// Types
import type { App } from "vue";

export function registerPlugins(app: App) {
    app.use(vuetify).use(router).use(pinia);
    app.component("font-awesome-icon", FontAwesomeIcon);
}
