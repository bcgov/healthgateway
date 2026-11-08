export interface HtmlSegment {
    text: string;
    bold?: boolean;
    italic?: boolean;
    href?: string;
}

const entities: Record<string, string> = {
    amp: "&",
    lt: "<",
    gt: ">",
    quot: '"',
    apos: "'",
    nbsp: " ",
    "#39": "'",
};

export const decodeHtmlEntities = (text: string) =>
    text.replace(/&(#\d+|[a-z]+);/gi, (match, name: string) => {
        const key = name.toLowerCase();
        if (entities[key] !== undefined) return entities[key];
        if (key.startsWith("#"))
            return String.fromCodePoint(Number.parseInt(key.slice(1), 10));
        return match;
    });

/** Only links a banner may legitimately hold; anything else (`javascript:`, data URLs) stays plain text. */
const safeHref = (href: string | undefined) => {
    const value = href?.trim();
    if (!value) return undefined;
    if (/^(https?:|mailto:|tel:)/i.test(value)) return value;
    // dev's banner links a bare address: on native it can only mean mail
    return /^[^\s/:@]+@[^\s/:@]+\.[^\s/:@]+$/.test(value)
        ? `mailto:${value}`
        : undefined;
};

/**
 * The communication banner's HTML (the web client's `v-html`) as styled runs for a native `<Text>`: links, bold,
 * italic and line breaks. Other tags are dropped and their text kept. Web renders the HTML itself.
 */
export function parseCommunicationHtml(html: string): HtmlSegment[] {
    const segments: HtmlSegment[] = [];
    const push = (segment: HtmlSegment) => {
        if (segment.text) segments.push(segment);
    };
    let bold = 0;
    let italic = 0;
    let href: string | undefined;
    let last = 0;
    const tag = /<(\/?)([a-z][a-z0-9]*)\b([^>]*)>/gi;
    for (let m = tag.exec(html); m; m = tag.exec(html)) {
        push({
            text: decodeHtmlEntities(html.slice(last, m.index)),
            bold: bold > 0 || undefined,
            italic: italic > 0 || undefined,
            href,
        });
        last = tag.lastIndex;
        const closing = m[1] === "/";
        const name = m[2].toLowerCase();
        if (name === "b" || name === "strong") bold += closing ? -1 : 1;
        else if (name === "i" || name === "em") italic += closing ? -1 : 1;
        else if (name === "a")
            href = closing
                ? undefined
                : safeHref(/href\s*=\s*["']([^"']*)["']/i.exec(m[3])?.[1]);
        else if (name === "br" || (closing && name === "p"))
            push({ text: "\n" });
    }
    push({
        text: decodeHtmlEntities(html.slice(last)),
        bold: bold > 0 || undefined,
        italic: italic > 0 || undefined,
        href,
    });
    // a closing </p> at the very end is not a blank line
    while (segments.at(-1)?.text === "\n") segments.pop();
    return segments;
}
