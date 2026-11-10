import { useCallback, useRef } from "react";
import { ScrollView, View } from "react-native";

import { ScrollToViewContext } from "@/components/site/PageScrollContext";
import { TermsHtml } from "@/views/public/termsOfService/TermsHtml";

/** HtmlTextAreaComponent.vue inside a bordered, scrollable box (RegistrationView: 330px; AcceptTerms: 460px max). */
export function TermsOfServiceBox({
    html,
    height,
    testID,
}: {
    html: string;
    height: number;
    testID?: string;
}) {
    const scroll = useRef<React.ComponentRef<typeof ScrollView>>(null);
    const scrollToView = useCallback(
        (view: React.ComponentRef<typeof View>) => {
            const content = scroll.current;
            const inner = content?.getInnerViewRef();
            if (!content || !inner) return;
            view.measureLayout(inner, (_x, y) =>
                content.scrollTo({ y, animated: true })
            );
        },
        []
    );
    return (
        <View
            testID={testID}
            className="border border-border-divider mb-3"
            style={{ height }}
        >
            <ScrollView
                ref={scroll}
                nestedScrollEnabled
                contentContainerClassName="p-6"
            >
                <ScrollToViewContext.Provider value={scrollToView}>
                    <TermsHtml html={html} />
                </ScrollToViewContext.Provider>
            </ScrollView>
        </View>
    );
}
