import { View } from "react-native";

import { LiveRegion, VisuallyHidden } from "./LiveRegion";

/**
 * v-skeleton-loader type "heading, text, heading, paragraph": grey bars standing in for the content while it
 * reloads (ProfileView.vue shows this instead of the profile while Loader.UserProfile is active). A status region
 * that says "Loading", so the screen reader hears it instead of six empty boxes.
 */
export function Skeleton({ testID }: { testID?: string }) {
    const bar = (w: string, h: number, mb: number) => (
        <View
            className="rounded bg-border-light"
            style={{ width: w as `${number}%`, height: h, marginBottom: mb }}
        />
    );
    return (
        <LiveRegion politeness="polite" testID={testID} className="py-4">
            <View aria-busy>
                {bar("50%", 24, 16)}
                {bar("70%", 12, 24)}
                {bar("50%", 24, 16)}
                {bar("100%", 12, 8)}
                {bar("100%", 12, 8)}
                {bar("60%", 12, 0)}
            </View>
            <VisuallyHidden>Loading</VisuallyHidden>
        </LiveRegion>
    );
}
