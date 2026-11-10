import "./TermsHtml.web.css";

import type { CSSProperties } from "react";

import { useThemeColors } from "@/hooks/useTheme";

/** Web: render the ToS HTML directly (same as v-html on the web client). */
export function TermsHtml({ html }: { html: string }) {
    const colors = useThemeColors();
    return (
        <div
            className="hg-document"
            style={
                {
                    fontFamily: "BCSans-Regular, sans-serif",
                    color: colors["bc-black"],
                    fontSize: 16,
                    lineHeight: 1.5,
                    "--hg-document-link": colors.link,
                } as CSSProperties
            }
            dangerouslySetInnerHTML={{ __html: html }}
        />
    );
}
