import { useState } from "react";
import { View } from "react-native";

import { Breadcrumb } from "@/components/site/Breadcrumb";
import { HgAlert, Loading, PageTitle, Skeleton } from "@/components/ui";
import { Path } from "@/constants/path";
import {
    Loader,
    selectLoaderActive,
    useLoadingStore,
} from "@/stores/loading.store";
import { selectUserIsActive, useUserStore } from "@/stores/user.store";

import { ActiveProfile } from "./ActiveProfile";
import { InactiveProfile } from "./InactiveProfile";

/**
 * Port of ProfileView.vue. While Loader.UserProfile is active (saving email / SMS, closing or recovering the
 * account) the page shows the loading overlay and a skeleton, and hides — but keeps mounted — the profile.
 */
export function ProfileView() {
    const isActive = useUserStore(selectUserIsActive);
    const loadingUser = useUserStore((s) => s.isLoading);
    const profileBusy = useLoadingStore(selectLoaderActive(Loader.UserProfile));
    const isLoading = loadingUser || profileBusy;
    const [showCheckEmailAlert, setShowCheckEmailAlert] = useState(false);

    return (
        <View>
            <Breadcrumb
                items={[
                    {
                        text: "Profile",
                        to: Path.Profile,
                        active: true,
                        testId: "breadcrumb-profile",
                    },
                ]}
            />
            <Loading isLoading={isLoading} />
            {isActive && showCheckEmailAlert ? (
                <HgAlert
                    testID="verifyEmailTxt"
                    type="info"
                    live="polite"
                    closable
                    onClose={() => setShowCheckEmailAlert(false)}
                    title="Please check your email"
                    text="Please check your email for an email verification link. If you didn't receive one, please check your junk mail."
                />
            ) : null}
            <PageTitle title="Profile" />
            {isLoading ? <Skeleton testID="profile-skeleton" /> : null}
            {/* v-show: hidden, not unmounted, so edits in progress survive the reload */}
            <View style={isLoading ? { display: "none" } : undefined}>
                {isActive ? (
                    <ActiveProfile
                        onEmailUpdated={(email) =>
                            setShowCheckEmailAlert(Boolean(email))
                        }
                    />
                ) : (
                    <InactiveProfile />
                )}
            </View>
        </View>
    );
}
