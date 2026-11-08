import { useContext, useMemo, useRef } from "react";
import { View } from "react-native";

import { ScrollToViewContext } from "@/components/site/PageScrollContext";
import { AppText } from "@/components/ui";
import { parseDocumentHtml } from "@/utils/documentHtml";
import { openExternal } from "@/utils/openExternal";

/** A list item's indent, as the web client's 40px `padding-left` on ol and ul. */
const LIST_INDENT = 40;

/** Native: the ToS HTML as native text, so it scrolls with whatever holds it instead of inside a web view. */
export function TermsHtml({ html }: { html: string }) {
    const blocks = useMemo(() => parseDocumentHtml(html), [html]);
    const scrollToView = useContext(ScrollToViewContext);
    const anchors = useRef(new Map<string, React.ComponentRef<typeof View>>());

    const follow = (href: string) => {
        if (!href.startsWith("#")) {
            void openExternal(href);
            return;
        }
        const target = anchors.current.get(href.slice(1));
        if (target) scrollToView(target);
    };

    return (
        <View testID="termsDocument">
            {blocks.map((block, i) => (
                <View
                    key={i}
                    ref={(node) => {
                        if (block.id && node)
                            anchors.current.set(block.id, node);
                    }}
                    collapsable={false}
                    className={`flex-row ${block.gapAfter ? "mb-4" : ""}`}
                    style={
                        block.depth > 0
                            ? { paddingLeft: LIST_INDENT * block.depth }
                            : undefined
                    }
                >
                    {block.marker ? (
                        <AppText
                            className="absolute text-right"
                            style={{
                                left: LIST_INDENT * block.depth - 32,
                                width: 26,
                            }}
                        >
                            {block.marker}
                        </AppText>
                    ) : null}
                    <AppText className="flex-1">
                        {block.runs.map((run, j) => {
                            const { href } = run;
                            return (
                                <AppText
                                    key={j}
                                    className={[
                                        run.bold ? "font-bold" : "",
                                        run.italic ? "italic" : "",
                                        href ? "text-link underline" : "",
                                    ].join(" ")}
                                    accessibilityRole={
                                        href ? "link" : undefined
                                    }
                                    onPress={
                                        href ? () => follow(href) : undefined
                                    }
                                >
                                    {run.text}
                                </AppText>
                            );
                        })}
                    </AppText>
                </View>
            ))}
        </View>
    );
}
