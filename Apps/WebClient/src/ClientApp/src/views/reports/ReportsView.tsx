import { View } from "react-native";

import { Breadcrumb } from "@/components/site/Breadcrumb";
import { OfflineNotice } from "@/components/site/OfflineNotice";
import { PageTitle } from "@/components/ui";
import { Path } from "@/constants/path";
import {
    selectIsLive,
    useConnectivityStore,
} from "@/stores/connectivity.store";
import { useUserStore } from "@/stores/user.store";

import { ReportsComponent } from "./ReportsComponent";

/** Port of ReportsView.vue */
export function ReportsView() {
    const hdid = useUserStore((s) => s.user.hdid);
    const isLive = useConnectivityStore(selectIsLive);
    return (
        <View>
            <Breadcrumb
                items={[
                    {
                        text: "Download",
                        to: Path.Reports,
                        active: true,
                        testId: "breadcrumb-download-records",
                    },
                ]}
            />
            <PageTitle title="Download" />
            {isLive ? (
                <ReportsComponent hdid={hdid} />
            ) : (
                <OfflineNotice testID="downloadOfflineNotice">
                    {
                        "Downloads aren't available right now. Health Gateway creates your reports, so this page needs a connection and a working login."
                    }
                </OfflineNotice>
            )}
        </View>
    );
}
