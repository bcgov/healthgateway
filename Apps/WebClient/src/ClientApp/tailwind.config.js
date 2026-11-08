const {
    tailwindColors,
    breakpoints,
    fontSize,
    fontFamily,
} = require("./src/theme/tokens");

/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["./src/**/*.{js,jsx,ts,tsx}"],
    presets: [require("nativewind/preset")],
    // NativeWind on web throws when expo toggles the html color-scheme unless dark mode is class-based.
    darkMode: "class",
    theme: {
        screens: Object.fromEntries(
            Object.entries(breakpoints).map(([k, v]) => [k, `${v}px`])
        ),
        extend: {
            // Tokens resolve through CSS variables (see src/theme/tokens.js) so the active palette decides.
            colors: tailwindColors,
            fontFamily,
            fontSize,
        },
    },
    plugins: [],
};
