import { describe, expect, it } from "@jest/globals";

import { parseDocumentHtml } from "@/utils/documentHtml";

const texts = (html: string) =>
    parseDocumentHtml(html).map((b) => b.runs.map((r) => r.text).join(""));

describe("parseDocumentHtml", () => {
    it("splits paragraphs, collapses whitespace and keeps the gap between them", () => {
        const blocks = parseDocumentHtml(
            "<p>\n  One\n  two <strong\n >bold</strong>.\n</p>\n<p>Three</p>"
        );
        expect(
            texts(
                "<p>\n  One\n  two <strong\n >bold</strong>.\n</p>\n<p>Three</p>"
            )
        ).toEqual(["One two bold.", "Three"]);
        expect(blocks[0].gapAfter).toBe(true);
        expect(blocks[0].runs[1]).toMatchObject({ text: "bold", bold: true });
        expect(blocks[1].gapAfter).toBe(false);
    });

    it("keeps a paragraph's id for links to jump to", () => {
        const blocks = parseDocumentHtml(
            '<p><a href="#tos-1">Go</a></p><p id="tos-1">There</p>'
        );
        expect(blocks[0].runs[0].href).toBe("#tos-1");
        expect(blocks[1].id).toBe("tos-1");
    });

    it("numbers ol, letters ol type=a and bullets ul, with the gap after the list", () => {
        const blocks = parseDocumentHtml(
            '<ol><li>A</li><li>B</li></ol><ol type="a"><li>C</li><li>D</li></ol><ul><li>E</li></ul><p>F</p>'
        );
        expect(blocks.map((b) => b.marker)).toEqual([
            "1.",
            "2.",
            "a.",
            "b.",
            "•",
            undefined,
        ]);
        expect(blocks.map((b) => b.gapAfter)).toEqual([
            false,
            true,
            false,
            true,
            true,
            false,
        ]);
        expect(blocks[0].depth).toBe(1);
    });

    it("nests lists and carries on the outer numbering", () => {
        const blocks = parseDocumentHtml(
            "<ol><li>One<ul><li>Inner</li></ul></li><li>Two</li></ol>"
        );
        expect(blocks.map((b) => [b.marker, b.depth])).toEqual([
            ["1.", 1],
            ["•", 2],
            ["2.", 1],
        ]);
    });

    it("decodes entities, keeps br and drops unsafe links", () => {
        const blocks = parseDocumentHtml(
            '<p>A &amp; B<br />C <a href="javascript:alert(1)">x</a> <a href="https://a.test">y</a></p>'
        );
        expect(blocks[0].runs.map((r) => r.text).join("")).toBe("A & B\nC x y");
        expect(
            blocks[0].runs.find((r) => r.text === "x")?.href
        ).toBeUndefined();
        expect(blocks[0].runs.find((r) => r.text === "y")?.href).toBe(
            "https://a.test"
        );
    });
});
