import { View } from "react-native";

import { LiveRegion, VisuallyHidden } from "./LiveRegion";

/**
 * v-skeleton-loader type="table-thead, table-tbody" (HgDataTable while loading). A status region that says
 * "Loading", as `Skeleton` does, so the screen reader hears it instead of a label on empty bars.
 */
export function TableSkeleton() {
    return (
        <LiveRegion politeness="polite" testID="table-skeleton-loader">
            <View aria-busy>
                {[0, 1, 2, 3].map((i) => (
                    <View
                        key={i}
                        className="h-3 rounded bg-border-light my-4"
                        style={{ width: i === 0 ? "60%" : "100%" }}
                    />
                ))}
            </View>
            <VisuallyHidden>Loading</VisuallyHidden>
        </LiveRegion>
    );
}
