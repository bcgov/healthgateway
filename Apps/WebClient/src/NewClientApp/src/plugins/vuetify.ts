import { aliases, fa } from "vuetify/iconsets/fa-svg";
import { library } from "@fortawesome/fontawesome-svg-core";
import { far } from "@fortawesome/free-regular-svg-icons";
import { fas } from "@fortawesome/free-solid-svg-icons";

// Styles
import "@/assets/styles/main.scss";

// Composables
import { createVuetify } from "vuetify";

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
    },
    icons: {
        defaultSet: "fa",
        aliases,
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
                },
            },
        },
    },
});
