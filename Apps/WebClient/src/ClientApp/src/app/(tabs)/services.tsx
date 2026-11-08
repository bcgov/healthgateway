import { LazyTab } from "@/components/site/LazyTab";
import { PageScroll } from "@/components/site/PageScroll";
import { ServicesView } from "@/views/services/ServicesView";

export default function Services() {
    return (
        <PageScroll>
            <LazyTab>
                <ServicesView />
            </LazyTab>
        </PageScroll>
    );
}
