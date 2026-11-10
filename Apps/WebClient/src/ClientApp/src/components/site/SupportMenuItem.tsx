import { useRouter } from "expo-router";
import { Platform, Pressable, View } from "react-native";

import { AppText, Icon } from "@/components/common";
import { useThemeColors } from "@/hooks/useTheme";
import { toHref } from "@/navigation/href";
import {
    allSupportLinks,
    followSupportLink,
    type SupportLink,
} from "@/navigation/supportLinks";
import { hrefProps } from "@/utils/hrefProps";

import { ExpandableMenuItem } from "./ExpandableMenuItem";

/** Native header menu entry holding the footer's links, which native does not show (stated deviation). */
export function SupportMenuItem({ onClose }: { onClose: () => void }) {
    const router = useRouter();
    const colors = useThemeColors();

    const follow = (link: SupportLink) => {
        onClose();
        // let the menu modal dismiss before the route changes (iOS otherwise can leave a stuck invisible window)
        setTimeout(
            () => followSupportLink(link, (path) => router.push(toHref(path))),
            Platform.OS === "ios" ? 400 : 0
        );
    };

    return (
        <ExpandableMenuItem
            label="Support"
            icon="circle-question"
            testID="menu-btn-support-link"
        >
            <View testID="support-options" className="pl-6">
                {allSupportLinks.map((link) => (
                    <Pressable
                        key={link.testID}
                        testID={link.testID}
                        accessibilityRole="link"
                        // where the link goes (a real anchor should this ever render on web); the press does the work
                        {...hrefProps(link.to ?? link.href)}
                        onPress={(event) => {
                            event.preventDefault();
                            follow(link);
                        }}
                        className="flex-row items-center px-4 py-3 active:bg-surface-hover"
                    >
                        <Icon
                            name={
                                link.to
                                    ? "file-lines"
                                    : "arrow-up-right-from-square"
                            }
                            size={14}
                            color={colors["bc-grey"]}
                            style={{ width: 24 }}
                        />
                        <AppText className="shrink">{link.text}</AppText>
                    </Pressable>
                ))}
            </View>
        </ExpandableMenuItem>
    );
}
