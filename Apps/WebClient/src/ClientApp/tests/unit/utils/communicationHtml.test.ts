import { describe, expect, it } from "@jest/globals";

import { parseCommunicationHtml } from "@/utils/communicationHtml";

describe("parseCommunicationHtml", () => {
    it("keeps plain text and decodes entities", () => {
        expect(parseCommunicationHtml("Tom &amp; Jerry&#39;s")).toEqual([
            { text: "Tom & Jerry's" },
        ]);
    });

    it("marks bold, italic and links, dropping other tags", () => {
        expect(
            parseCommunicationHtml(
                '<p><span>Read <strong>this</strong> <em>now</em> <a href="https://x.test">here</a></span></p>'
            )
        ).toEqual([
            { text: "Read " },
            { text: "this", bold: true },
            { text: " " },
            { text: "now", italic: true },
            { text: " " },
            { text: "here", href: "https://x.test" },
        ]);
    });

    it("turns line breaks and paragraph ends into new lines, not at the very end", () => {
        expect(
            parseCommunicationHtml("<p>One</p><p>Two<br>Three</p>").map(
                (s) => s.text
            )
        ).toEqual(["One", "\n", "Two", "\n", "Three"]);
    });

    it("only allows http, mail and phone links", () => {
        const [link] = parseCommunicationHtml(
            '<a href="javascript:alert(1)">bad</a>'
        );
        expect(link).toEqual({ text: "bad" });
        expect(
            parseCommunicationHtml('<a href="mailto:a@b.ca">m</a>')[0].href
        ).toBe("mailto:a@b.ca");
        expect(parseCommunicationHtml('<a href="a@b.ca">m</a>')[0].href).toBe(
            "mailto:a@b.ca"
        );
    });
});
