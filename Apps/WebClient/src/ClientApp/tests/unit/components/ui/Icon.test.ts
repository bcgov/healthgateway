import { describe, expect, it } from "@jest/globals";
import { readdirSync, readFileSync, statSync } from "fs";
import { join } from "path";

import { webIconNames } from "@/components/ui/Icon.web";

/** Every icon name written in src: <Icon name="x">, name={c ? "x" : "y"}, icon="x" props and `icon: "x"` tables. */
function iconNamesInSource(): string[] {
    const names = new Set<string>();
    const walk = (dir: string) => {
        for (const entry of readdirSync(dir)) {
            const p = join(dir, entry);
            if (statSync(p).isDirectory()) {
                if (entry !== "test") walk(p); // src/test holds fixtures, not UI
            } else if (/\.tsx?$/.test(entry) && !/\.test\.tsx?$/.test(entry)) {
                const s = readFileSync(p, "utf8");
                for (const m of s.matchAll(
                    /<Icon\b[^>]*?\bname="([a-z0-9-]+)"/gs
                ))
                    names.add(m[1]);
                for (const m of s.matchAll(
                    /<Icon\b[^>]*?\bname=\{[^}]*?"([a-z0-9-]+)"[^}]*?:[^}]*?"([a-z0-9-]+)"/gs
                )) {
                    names.add(m[1]);
                    names.add(m[2]);
                }
                for (const m of s.matchAll(/\bicon[:=] ?"([a-z0-9-]+)"/g))
                    names.add(m[1]);
            }
        }
    };
    walk(join(process.cwd(), "src"));
    return [...names].sort();
}

describe("web icons", () => {
    const used = iconNamesInSource();

    it("finds the icons used by the app", () => {
        expect(used.length).toBeGreaterThan(20);
        expect(used).toContain("bars");
        expect(used).toContain("square-check");
    });

    it.each(used)(
        "maps %s to an SVG so the icon font is not needed",
        (name) => {
            // regular-style icons (e.g. `far fa-comment`) are drawn from the regular set
            expect([...webIconNames.solid, ...webIconNames.regular]).toContain(
                name
            );
        }
    );

    it("maps the outline variants drawn with solid={false}", () => {
        expect(webIconNames.regular).toEqual(
            expect.arrayContaining(["square", "star"])
        );
    });
});
