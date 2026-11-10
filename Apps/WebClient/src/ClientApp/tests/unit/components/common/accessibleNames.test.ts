import { describe, expect, it } from "@jest/globals";
import { readdirSync, readFileSync, statSync } from "fs";
import { join, relative } from "path";
import ts from "typescript";

/**
 * Static guards for names and roles (WCAG 4.1.2), in the style of accessibilityRoles.test.ts: every pressable thing
 * tells the screen reader what it is, and an icon-only button says what it does.
 */

const sourceFiles = (dir: string): string[] =>
    readdirSync(dir).flatMap((name) => {
        const path = join(dir, name);
        if (statSync(path).isDirectory()) return sourceFiles(path);
        return name.endsWith(".tsx") ? [path] : [];
    });

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

const attributeNames = (tag: ts.JsxOpeningLikeElement) =>
    tag.attributes.properties.flatMap((p) =>
        ts.isJsxAttribute(p) ? [p.name.getText()] : []
    );
const attribute = (tag: ts.JsxOpeningLikeElement, name: string) =>
    tag.attributes.properties.find(
        (p): p is ts.JsxAttribute =>
            ts.isJsxAttribute(p) && p.name.getText() === name
    );
const literal = (tag: ts.JsxOpeningLikeElement, name: string) => {
    const init = attribute(tag, name)?.initializer;
    if (!init) return undefined;
    if (ts.isStringLiteral(init)) return init.text;
    if (
        ts.isJsxExpression(init) &&
        init.expression &&
        (ts.isStringLiteral(init.expression) ||
            ts.isNumericLiteral(init.expression) ||
            init.expression.kind === ts.SyntaxKind.TrueKeyword ||
            init.expression.kind === ts.SyntaxKind.FalseKeyword ||
            ts.isPrefixUnaryExpression(init.expression))
    )
        return init.expression.getText();
    return undefined;
};
const spreadsInclude = (tag: ts.JsxOpeningLikeElement, pattern: RegExp) =>
    tag.attributes.properties.some(
        (p) => ts.isJsxSpreadAttribute(p) && pattern.test(p.getText())
    );
const where = (file: string, tag: ts.Node) =>
    `${relative(process.cwd(), file)}:${
        tag.getSourceFile().getLineAndCharacterOfPosition(tag.getStart()).line +
        1
    }`;

/** The JSX children of an opening tag (none for a self-closing one), ignoring whitespace-only text. */
const childElements = (tag: ts.JsxOpeningLikeElement): ts.JsxChild[] =>
    ts.isJsxOpeningElement(tag)
        ? tag.parent.children.filter(
              (child) =>
                  !(ts.isJsxText(child) && child.containsOnlyTriviaWhiteSpaces)
          )
        : [];

describe("accessible names and roles", () => {
    const files = sourceFiles(join(process.cwd(), "src"));

    it("a Pressable with onPress has a role, or is a link, or is kept out of the accessibility tree", () => {
        const bad = files.flatMap((file) =>
            openingTags(file)
                .filter((tag) => tag.tagName.getText() === "Pressable")
                .filter((tag) => attribute(tag, "onPress") !== undefined)
                .filter((tag) => {
                    const names = attributeNames(tag);
                    if (names.includes("accessibilityRole")) return false;
                    if (names.includes("role")) return false;
                    if (names.includes("href")) return false;
                    if (spreadsInclude(tag, /\bhrefProps\(/)) return false;
                    if (literal(tag, "tabIndex") === "-1") return false;
                    if (literal(tag, "accessible") === "false") return false;
                    return true;
                })
                .map((tag) => where(file, tag))
        );
        expect(bad).toEqual([]);
    });

    it("a button whose only content is an icon has a label", () => {
        const bad = files.flatMap((file) =>
            openingTags(file)
                .filter(
                    (tag) =>
                        literal(tag, "accessibilityRole") === "button" &&
                        !attribute(tag, "accessibilityLabel") &&
                        !attribute(tag, "aria-label")
                )
                .filter((tag) => {
                    const children = childElements(tag);
                    const [only] = children;
                    if (children.length !== 1 || !only) return false;
                    const element = ts.isJsxElement(only)
                        ? only.openingElement
                        : ts.isJsxSelfClosingElement(only)
                          ? only
                          : undefined;
                    return element?.tagName.getText() === "Icon";
                })
                .map((tag) => where(file, tag))
        );
        expect(bad).toEqual([]);
    });
});
