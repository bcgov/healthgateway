import { useLocalSearchParams } from "expo-router";

import { PageScroll } from "@/components/site/PageScroll";
import { DependentTimelineView } from "@/views/dependents/DependentTimelineView";

export default function DependentTimeline() {
    const { id } = useLocalSearchParams<{ id: string }>();
    return (
        <PageScroll>
            <DependentTimelineView id={id} />
        </PageScroll>
    );
}
