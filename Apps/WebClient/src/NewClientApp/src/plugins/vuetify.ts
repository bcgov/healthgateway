import "@/assets/styles/main.scss";

import { library } from "@fortawesome/fontawesome-svg-core";
import { far } from "@fortawesome/free-regular-svg-icons";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { createVuetify } from "vuetify";
import { aliases, fa } from "vuetify/iconsets/fa-svg";
import { VSkeletonLoader } from "vuetify/labs/VSkeletonLoader";

library.add(far, fas);

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
export default createVuetify({
    defaults: {
        global: {
            ripple: false,
        },
        VIcon: {
            style: "width: auto",
        },
        VSelect: {
            variant: "solo",
        },
        VTextarea: {
            variant: "solo",
        },
        VTextField: {
            variant: "solo",
        },
        VTooltip: {
            location: "bottom",
            contentClass: "bg-grey-darken-3",
            openOnClick: true,
        },
    },
    icons: {
        defaultSet: "fa",
        aliases,
        sets: {
            fa,
        },
    },
    components: {
        VSkeletonLoader,
    },
    theme: {
        themes: {
            light: {
                colors: {
                    background: "#ffffff",
                    surface: "#ffffff",
                    primary: "#003366",
                    secondary: "#38598a",
                    success: "#2e8540",
                    // warning: "",
                    error: "#d8292f",
                    info: "#0092f1",
                    // custom colours below
                    accent: "#fcba19",
                    link: "#1a5a96",
                    focus: "#3b99fc",
                },
            },
        },
    },
});
