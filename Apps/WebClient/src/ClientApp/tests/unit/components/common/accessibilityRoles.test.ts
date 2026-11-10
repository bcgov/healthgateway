import { describe, expect, it } from "@jest/globals";
import { readdirSync, readFileSync, statSync } from "fs";
import { join } from "path";
import ts from "typescript";

/** ReactAccessibilityDelegate.AccessibilityRole: Android throws on anything else, crashing the page. */
const ANDROID_ROLES = new Set(
    "none button dropdownlist togglebutton link search image imagebutton keyboardkey text adjustable summary header alert checkbox combobox menu menubar menuitem progressbar radio radiogroup scrollbar spinbutton switch tab tabbar tablist timer list grid pager scrollview horizontalscrollview viewgroup webview drawerlayout slidingdrawer iconmenu toolbar".split(
        " "
    )
);

const sourceFiles = (dir: string): string[] =>
    readdirSync(dir).flatMap((name) => {
        const path = join(dir, name);
        if (statSync(path).isDirectory()) return sourceFiles(path);
        return name.endsWith(".tsx") && !name.endsWith(".web.tsx")
            ? [path]
            : [];
    });

describe("accessibilityRole", () => {
    it("native code only passes roles Android accepts, outside a web-only branch", () => {
        const bad = sourceFiles(join(process.cwd(), "src")).flatMap((file) =>
            [
                ...readFileSync(file, "utf8").matchAll(
                    /accessibilityRole="([a-z]+)"/g
                ),
            ]
                .filter((match) => !ANDROID_ROLES.has(match[1]))
                .map((match) => `${file}: ${match[1]}`)
        );
        expect(bad).toEqual([]);
    });
});

/** Every `.tsx` under a folder, the web variants included. */
const allSourceFiles = (dir: string): string[] =>
    readdirSync(dir).flatMap((name) => {
        const path = join(dir, name);
        if (statSync(path).isDirectory()) return allSourceFiles(path);
        return name.endsWith(".tsx") ? [path] : [];
    });

/** The opening tags (`<Pressable …>`, `<View … />`) in a file, with their place in the tree. */
function openingTags(file: string): ts.JsxOpeningLikeElement[] {
    const source = ts.createSourceFile(
        file,
        readFileSync(file, "utf8"),
        ts.ScriptTarget.Latest,
        true,
        ts.ScriptKind.TSX
    );
    const tags: ts.JsxOpeningLikeElement[] = [];
    const visit = (node: ts.Node) => {
        if (ts.isJsxOpeningElement(node) || ts.isJsxSelfClosingElement(node))
            tags.push(node);
        ts.forEachChild(node, visit);
    };
    visit(source);
    return tags;
}

const literalAttribute = (tag: ts.JsxOpeningLikeElement, name: string) =>
    tag.attributes.properties.find(
        (p): p is ts.JsxAttribute =>
            ts.isJsxAttribute(p) &&
            p.name.getText() === name &&
            p.initializer !== undefined &&
            ts.isStringLiteral(p.initializer)
    )?.initializer as ts.StringLiteral | undefined;

/** An `href` attribute, or a spread of `hrefProps(…)` (which adds one on web). */
const hasHref = (tag: ts.JsxOpeningLikeElement) =>
    tag.attributes.properties.some((p) =>
        ts.isJsxAttribute(p)
            ? p.name.getText() === "href"
            : /\bhrefProps\(/.test(p.getText())
    );

/** The direct child of an expo-router `<Link asChild>`, which gives it the href. */
const inLink = (tag: ts.JsxOpeningLikeElement) => {
    const element = ts.isJsxOpeningElement(tag) ? tag.parent : tag;
    const parent = element.parent;
    return (
        ts.isJsxElement(parent) &&
        parent.openingElement.tagName.getText() === "Link"
    );
};

describe("accessibilityRole link", () => {
    it("a Pressable or View with role link has an href (or sits in a Link): react-native-web leaves Enter on a link to the browser", () => {
        const bad = allSourceFiles(join(process.cwd(), "src")).flatMap((file) =>
            openingTags(file)
                .filter(
                    (tag) =>
                        ["Pressable", "View"].includes(tag.tagName.getText()) &&
                        literalAttribute(tag, "accessibilityRole")?.text ===
                            "link" &&
                        !hasHref(tag) &&
                        !inLink(tag)
                )
                .map(
                    (tag) =>
                        `${file}:${
                            tag
                                .getSourceFile()
                                .getLineAndCharacterOfPosition(tag.getStart())
                                .line + 1
                        }`
                )
        );
        expect(bad).toEqual([]);
    });
});
