import "@/assets/styles/main.scss";

import { library } from "@fortawesome/fontawesome-svg-core";
import { far } from "@fortawesome/free-regular-svg-icons";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { createVuetify } from "vuetify";
import { aliases, fa } from "vuetify/iconsets/fa-svg";

library.add(far, fas);

// Override the default Vuetify icon aliases
const customAliases = {
    ...aliases,
    error: "fas fa-circle-xmark", // Override default error icon replaces 'fa-triangle-exclamation'
    warning: "fas fa-triangle-exclamation", // Re-assert warning icon - 'fa-triangle-exclamation'
    success: "fas fa-check-circle", // Re-assert success icon - 'fa-check-circle'
    info: "fas fa-circle-info", // Re-assert info icon - 'fa-circle-info'
};

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
        aliases: customAliases,
        sets: {
            fa,
        },
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
                    infoBackground: "#e5edf5",
                    infoText: "#1a2e49",
                    borderLight: "#e0e0e0",
                    navHighlight: "#e0e0e0",
                    borderDivider: "#d1d5db",
                    surfaceHover: "#f3f4f6",
                },
            },
        },
    },
});
