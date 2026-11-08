import { describe, expect, it } from "@jest/globals";
import { existsSync, readFileSync } from "fs";
import { join } from "path";

import { fonts } from "@/theme";

const root = process.cwd(); // jest runs from the project root
const css = readFileSync(join(root, "src/fonts.web.css"), "utf8");

describe("web fonts", () => {
    it.each(Object.values(fonts))(
        "declares %s as a woff2 face that exists in public/fonts",
        (family) => {
            expect(css).toContain(`font-family: "${family}";`);
            expect(css).toContain(`url("/fonts/${family}.woff2")`);
            expect(
                existsSync(join(root, "public/fonts", `${family}.woff2`))
            ).toBe(true);
        }
    );

    it("swaps fonts in instead of blocking text", () => {
        const faces = css.match(/@font-face/g) ?? [];
        const swaps = css.match(/font-display: swap/g) ?? [];
        // one per family, plus Bold, Italic and BoldItalic again under the regular family
        expect(faces).toHaveLength(Object.keys(fonts).length + 3);
        expect(swaps).toHaveLength(faces.length);
    });

    // The weight and style each file really has. `font-bold` asks for the BCSans-Bold family *and* weight 700: a
    // face declared lighter or upright than it is makes the browser fake bold or slant on top of the real glyphs.
    const real: Record<string, [weight: string, style: string]> = {
        "BCSans-Regular": ["normal", "normal"],
        "BCSans-Bold": ["bold", "normal"],
        "BCSans-Italic": ["normal", "italic"],
        "BCSans-BoldItalic": ["bold", "italic"],
        "BCSans-Light": ["300", "normal"],
        "BCSans-LightItalic": ["300", "italic"],
    };
    const face = (family: string, file: string) =>
        new RegExp(
            `font-family: "${family}";\\s+src: url\\("/fonts/${file}\\.woff2"\\) format\\("woff2"\\);\\s+` +
                `font-weight: ${real[file][0]};\\s+font-style: ${real[file][1]};`
        );

    it.each(Object.values(fonts))(
        "declares the %s face with its real weight and style",
        (family) => {
            expect(css).toMatch(face(family, family));
        }
    );

    it.each(["BCSans-Bold", "BCSans-Italic", "BCSans-BoldItalic"])(
        "gives bold and italic on the regular family the real %s face",
        (file) => {
            expect(css).toMatch(face("BCSans-Regular", file));
        }
    );
});
