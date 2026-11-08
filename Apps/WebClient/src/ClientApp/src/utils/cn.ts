import { extendTailwindMerge } from "tailwind-merge";

import { colors, fontSize } from "@/theme";

/**
 * Merges Tailwind class lists so a later class replaces an earlier one of the same kind, instead of the
 * two competing by stylesheet order on web (`text-primary` + `text-bc-black`, `text-body-large` +
 * `text-[14px]`, `self-start` + `self-center`). tailwind-merge needs to know the project's own tokens:
 * colour names are read from the theme and the Vuetify type scale from `fontSize`, so `text-bc-black`
 * is a colour and `text-body-large` a size.
 */
export const cn = extendTailwindMerge({
    extend: {
        theme: { colors: Object.keys(colors) },
        classGroups: { "font-size": [{ text: Object.keys(fontSize) }] },
        // `font-bold` is also the BC Sans Bold family (theme fontFamily.bold): `font-sans` must not win over it, or iOS
        // shows regular text, having no bold of the regular family to synthesise
        conflictingClassGroups: { "font-weight": ["font-family"] },
    },
});
