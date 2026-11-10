import { decodeHtmlEntities, type HtmlSegment } from "./communicationHtml";

export interface DocumentBlock {
    /** `<p id>`: where a `#id` link in the document goes */
    id?: string;
    /** a list item's number, letter or bullet */
    marker?: string;
    /** how many lists it sits inside */
    depth: number;
    /** the last block of a paragraph or list: the 1em gap follows it */
    gapAfter: boolean;
    runs: HtmlSegment[];
}

interface OpenList {
    ordered: boolean;
    alpha: boolean;
    count: number;
}

const attribute = (attributes: string, name: string) =>
    new RegExp(`\\b${name}\\s*=\\s*["']([^"']*)["']`, "i").exec(
        attributes
    )?.[1];

/** Same-document jumps, the web, mail and phone: nothing else (`javascript:`, data URLs) becomes a link. */
const safeHref = (href: string | undefined) => {
    const value = href?.trim();
    return value && /^(#|https?:|mailto:|tel:)/i.test(value)
        ? value
        : undefined;
};

const marker = (list: OpenList) => {
    if (!list.ordered) return "•";
    list.count += 1;
    return list.alpha
        ? `${String.fromCodePoint(96 + list.count)}.`
        : `${list.count}.`;
};

/**
 * Terms of Service HTML (the web client's `v-html`) as blocks of styled text for native views, which is what lets it scroll
 * with the page. Handles what the document uses: p, ol, ul, li, br, a, strong, em. Whitespace collapses as in a browser.
 */
export function parseDocumentHtml(html: string): DocumentBlock[] {
    const blocks: DocumentBlock[] = [];
    const lists: OpenList[] = [];
    let current: DocumentBlock | undefined;
    let bold = 0;
    let italic = 0;
    let href: string | undefined;

    const flush = (gapAfter = false) => {
        if (current) {
            const last = current.runs.at(-1);
            if (last) last.text = last.text.trimEnd();
            current.runs = current.runs.filter((run) => run.text);
            if (current.runs.length > 0) blocks.push(current);
        }
        current = undefined;
        const previous = blocks.at(-1);
        if (gapAfter && previous) previous.gapAfter = true;
    };
    const open = (id?: string, itemMarker?: string) => {
        flush();
        current = {
            id,
            marker: itemMarker,
            depth: lists.length,
            gapAfter: false,
            runs: [],
        };
    };
    const addText = (raw: string) => {
        const collapsed = raw.replace(/\s+/g, " ");
        if (!collapsed || (!current && collapsed === " ")) return;
        current ??= { depth: lists.length, gapAfter: false, runs: [] };
        const previous = current.runs.at(-1);
        const atLineStart = !previous || previous.text.endsWith("\n");
        const text = atLineStart ? collapsed.trimStart() : collapsed;
        if (!text) return;
        current.runs.push({
            text: decodeHtmlEntities(text),
            bold: bold > 0 || undefined,
            italic: italic > 0 || undefined,
            href,
        });
    };

    let last = 0;
    const tag = /<(\/?)([a-z][a-z0-9]*)\b([^>]*)>/gi;
    for (let m = tag.exec(html); m; m = tag.exec(html)) {
        addText(html.slice(last, m.index));
        last = tag.lastIndex;
        const closing = m[1] === "/";
        const name = m[2].toLowerCase();
        const attributes = m[3];
        if (name === "p") {
            if (closing) flush(true);
            else open(attribute(attributes, "id"));
        } else if (name === "ol" || name === "ul") {
            flush();
            if (closing) {
                lists.pop();
                if (lists.length === 0) flush(true);
            } else
                lists.push({
                    ordered: name === "ol",
                    alpha: attribute(attributes, "type") === "a",
                    count: 0,
                });
        } else if (name === "li") {
            if (closing) flush();
            else {
                const list = lists.at(-1);
                open(undefined, list ? marker(list) : undefined);
            }
        } else if (name === "strong" || name === "b") bold += closing ? -1 : 1;
        else if (name === "em" || name === "i") italic += closing ? -1 : 1;
        else if (name === "a")
            href = closing
                ? undefined
                : safeHref(attribute(attributes, "href"));
        else if (name === "br") current?.runs.push({ text: "\n" });
    }
    addText(html.slice(last));
    flush();
    const final = blocks.at(-1);
    if (final) final.gapAfter = false;
    return blocks;
}
