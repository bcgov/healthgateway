// Single source of truth for design tokens. CommonJS so tailwind.config.js can require it.
// Light values mirror Apps/WebClient/src/ClientApp/src/plugins/vuetify.ts in the Health Gateway repo.
// Dark values are mapped from the Admin site's MudBlazor palette (Apps/Admin/Client/Theme/HgTheme.cs,
// PaletteDark): see docs/theming.md for the mapping and contrast ratios.
const colors = {
    background: "#ffffff",
    surface: "#ffffff",
    "app-bar": "#ffffff",
    primary: "#003366",
    "on-primary": "#ffffff",
    secondary: "#38598a",
    success: "#2e8540",
    "success-background": "#ffffff",
    warning: "#7a4f00",
    error: "#d8292f",
    "on-error": "#ffffff",
    info: "#0092f1",
    accent: "#fcba19",
    "on-accent": "#313132",
    link: "#1a5a96",
    focus: "#3b99fc",
    "error-background": "#fbd5d8",
    // ErrorCardComponent's details panel: error-background-darken-1 border on a paler panel
    "error-border": "#ddbabd",
    "error-panel": "#fdf0f1",
    // Vuetify grey-lighten-3 (the dependent card's title bar)
    "card-header": "#eeeeee",
    "warning-background": "#fff4dc",
    "warning-text": "#5c3a00",
    "warning-border": "#f9b23d",
    "info-background": "#e5edf5",
    "info-text": "#1a2e49",
    "border-light": "#e0e0e0",
    "nav-highlight": "#e0e0e0",
    "border-divider": "#d1d5db",
    "surface-hover": "#f3f4f6",
    // Vuetify grey-lighten-4 (the medication active-ingredients table header)
    "table-header": "#f5f5f5",
    // v-tooltip: surface-variant / on-surface-variant
    "tooltip-background": "#424242",
    "tooltip-text": "#eeeeee",
    "section-background": "#f0f5fa",
    "bc-black": "#313132",
    // CommunicationComponent.vue's v-banner: on-surface text, and the browser's own link colour for the
    // unstyled anchors its v-html carries
    "banner-text": "#000000",
    "banner-link": "#0000ee",
    "bc-grey": "#606060",
    "splash-background": "#103362",
    "brand-plate": "#ffffff",
    "disabled-background": "rgba(0,0,0,0.12)",
    "on-disabled": "rgba(0,0,0,0.38)",
    // v-switch: unchecked track = surface-variant (shown at 60 %), thumb = surface-bright
    "switch-track": "#424242",
    "switch-thumb": "#ffffff",
};

// Admin PaletteDark: Background #001E30 (CyanBlue), Surface #123147, AppbarBackground #0D2C41,
// Primary #A7C8FF / #003061, Success #82DA8A, Warning #FCC966, Error #FFB3B1 / #680011, Info #D1ECF1,
// Secondary (gold) #FDBB1B, Tertiary #C4C6CF, LinesDefault rgba(255,255,255,0.12).
const darkColors = {
    background: "#001e30",
    surface: "#123147",
    "app-bar": "#0d2c41",
    primary: "#a7c8ff",
    "on-primary": "#003061",
    secondary: "#c4c6cf",
    success: "#82da8a",
    "success-background": "rgba(130,218,138,0.12)",
    warning: "#fcc966",
    error: "#ffb3b1",
    "on-error": "#680011",
    info: "#d1ecf1",
    accent: "#fdbb1b",
    "on-accent": "#313132",
    link: "#a7c8ff",
    focus: "#a7c8ff",
    "error-background": "rgba(255,179,177,0.12)",
    "warning-background": "rgba(252,201,102,0.12)",
    "warning-text": "#fcc966",
    "warning-border": "#fcc966",
    "info-background": "rgba(209,236,241,0.12)",
    "info-text": "#d1ecf1",
    "error-border": "#ffb3b1",
    "error-panel": "rgba(255,179,177,0.06)",
    "card-header": "rgba(255,255,255,0.08)",
    "border-light": "rgba(255,255,255,0.12)",
    "nav-highlight": "rgba(255,255,255,0.12)",
    "border-divider": "rgba(255,255,255,0.12)",
    "surface-hover": "rgba(255,255,255,0.08)",
    "table-header": "rgba(255,255,255,0.05)",
    "tooltip-background": "#c8c8c8",
    "tooltip-text": "#000000",
    "section-background": "#0d2c41",
    "bc-black": "#ffffff",
    "banner-text": "#ffffff",
    "banner-link": "#a7c8ff",
    "bc-grey": "#b8c1c8",
    "splash-background": "#103362",
    "brand-plate": "#ffffff",
    "disabled-background": "rgba(255,255,255,0.12)",
    "on-disabled": "#b8c1c8",
    "switch-track": "#c8c8c8",
    "switch-thumb": "#ffffff",
};

/** CSS custom property name for a colour token. */
const cssVar = (name) => `--color-${name}`;

/** Tailwind colour map: every token resolves through its CSS variable so the active palette decides the value. */
const tailwindColors = Object.fromEntries(
    Object.keys(colors).map((name) => [name, `var(${cssVar(name)})`])
);

/** The custom properties for one palette, in the shape NativeWind's vars() and the web root style expect. */
const toCssVars = (palette) =>
    Object.fromEntries(
        Object.entries(palette).map(([name, value]) => [cssVar(name), value])
    );

// Vuetify breakpoints restored by the web client (md 960, lg 1280, xl 1920, xxl 2560).
const breakpoints = { sm: 600, md: 960, lg: 1280, xl: 1920, "2xl": 2560 };

// Vuetify "text-*" scale used by the web client: [size, line-height]
const fontSize = {
    "body-large": ["16px", "24px"],
    "body-small": ["12px", "16px"],
    "body-medium": ["14px", "20px"],
    "headline-small": ["24px", "32px"],
    "headline-medium": ["28px", "36px"],
    "headline-large": ["32px", "40px"],
};

const fontFamily = {
    sans: ["BCSans-Regular"],
    light: ["BCSans-Light"],
    bold: ["BCSans-Bold"],
    italic: ["BCSans-Italic"],
};

module.exports = {
    colors,
    darkColors,
    tailwindColors,
    toCssVars,
    breakpoints,
    fontSize,
    fontFamily,
};
