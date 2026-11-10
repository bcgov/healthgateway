import { LazyTab } from "@/components/site/LazyTab";
import { PageScroll } from "@/components/site/PageScroll";
import { TimelineView } from "@/views/private/timeline/TimelineView";

export default function Timeline() {
    return (
        <PageScroll>
            <LazyTab>
                <TimelineView />
            </LazyTab>
        </PageScroll>
    );
}
