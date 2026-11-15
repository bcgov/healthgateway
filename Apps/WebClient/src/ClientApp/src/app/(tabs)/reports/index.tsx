import { LazyTab } from "@/components/site/LazyTab";
import { PageScroll } from "@/components/site/PageScroll";
import { ReportsView } from "@/views/private/reports/ReportsView";

export default function Reports() {
    return (
        <PageScroll>
            <LazyTab>
                <ReportsView />
            </LazyTab>
        </PageScroll>
    );
}
