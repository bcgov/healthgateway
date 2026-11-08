import { useRouter } from "expo-router";
import { View } from "react-native";

import { Breadcrumb } from "@/components/site/Breadcrumb";
import { AppText, HgButton, Icon, PageTitle } from "@/components/ui";
import { Path } from "@/constants/path";
import { useThemeColors } from "@/hooks/useTheme";
import type { BreadcrumbItem } from "@/models/breadcrumbItem";
import { useAuthStore } from "@/stores/auth.store";

interface Props {
    title: string;
    path: string;
    testID?: string;
    showLogout?: boolean;
    hideBreadcrumb?: boolean;
}

/** Classic "under construction" page used for every route not implemented in Phase 1. */
export function UnderConstructionView({
    title,
    path,
    testID,
    showLogout,
    hideBreadcrumb,
}: Props) {
    const colors = useThemeColors();
    const router = useRouter();
    const items: BreadcrumbItem[] = [
        {
            text: title,
            to: path,
            active: true,
            testId: `breadcrumb-${testID ?? title}`,
        },
    ];
    return (
        <View testID={testID}>
            {!hideBreadcrumb ? <Breadcrumb items={items} /> : null}
            <PageTitle title={title} />
            <View className="items-center py-12 px-4 rounded-md bg-section-background">
                <Icon name="person-digging" size={56} color={colors.accent} />
                <AppText variant="h3" className="mt-4 text-bc-black">
                    Under construction
                </AppText>
                <AppText className="mt-2 text-center">
                    {title} is not available in this version of the app yet.
                    Please check back soon.
                </AppText>
                <View className="flex-row gap-3 mt-6">
                    <HgButton
                        text="Go to home"
                        variant="secondary"
                        onPress={() => router.push(Path.Home)}
                    />
                    {showLogout ? (
                        <HgButton
                            text="Log Out"
                            icon="right-from-bracket"
                            onPress={() => {
                                void useAuthStore.getState();
                                router.push(Path.Logout);
                            }}
                        />
                    ) : null}
                </View>
            </View>
        </View>
    );
}
