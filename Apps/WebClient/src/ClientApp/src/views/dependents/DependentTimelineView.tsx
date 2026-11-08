import { useRouter } from "expo-router";
import { useEffect } from "react";
import { Platform, Pressable, View } from "react-native";

import { Breadcrumb } from "@/components/site/Breadcrumb";
import { Icon, Loading, PageTitle } from "@/components/ui";
import { entryTypes as allEntryTypes } from "@/constants/entryType";
import { Path } from "@/constants/path";
import { useThemeColors } from "@/hooks/useTheme";
import { formatDependentName } from "@/models/dependent";
import { describeError, logger } from "@/services/logger";
import {
    selectDependentsAreLoading,
    useDependentStore,
} from "@/stores/dependent.store";
import { useUserStore } from "@/stores/user.store";
import { ConfigUtil } from "@/utils/configUtil";

import { Timeline } from "../timeline/Timeline";

/** Port of DependentTimelineView.vue */
export function DependentTimelineView({ id }: { id: string }) {
    const router = useRouter();
    const colors = useThemeColors();
    const hdid = useUserStore((s) => s.user.hdid);
    const dependents = useDependentStore((s) => s.dependents);
    const isLoading = useDependentStore(selectDependentsAreLoading);
    const retrieveDependents = useDependentStore((s) => s.retrieveDependents);
    const dependent = dependents.find((d) => d.ownerId === id);
    const formattedName = formatDependentName(dependent?.dependentInformation);
    const entryTypes = allEntryTypes
        .filter((d) => ConfigUtil.isDependentDatasetEnabled(d.type))
        .map((d) => d.type);

    useEffect(() => {
        if (!hdid) return;
        retrieveDependents(hdid, false)
            .then(() => {
                const known = useDependentStore
                    .getState()
                    .dependents.some((d) => d.ownerId === id);
                if (!known) router.push(Path.Unauthorized);
            })
            .catch((error: unknown) => logger.error(describeError(error)));
    }, [hdid, id, retrieveDependents, router]);

    return (
        <View>
            <Loading isLoading={isLoading} />
            {!isLoading && dependent !== undefined ? (
                <View>
                    <Breadcrumb
                        items={[
                            {
                                text: "Dependents",
                                to: Path.Dependents,
                                active: false,
                                testId: "breadcrumb-dependents",
                            },
                            {
                                text: formattedName,
                                active: true,
                                testId: "breadcrumb-dependent-name",
                            },
                        ]}
                    />
                    <PageTitle
                        testID="page-title"
                        title={`Health Records for ${formattedName}`}
                        prepend={
                            <Pressable
                                testID="backBtn"
                                accessibilityRole="button"
                                accessibilityLabel="Back"
                                hitSlop={8}
                                onPress={() =>
                                    // native: back down the stack, not another list on top of it
                                    Platform.OS === "web"
                                        ? router.push(Path.Dependents)
                                        : router.dismissTo(Path.Dependents)
                                }
                                className="p-2"
                            >
                                <Icon
                                    name="arrow-left"
                                    size={18}
                                    color={colors["bc-grey"]}
                                />
                            </Pressable>
                        }
                    />
                    <Timeline hdid={id} entryTypes={entryTypes} />
                </View>
            ) : null}
        </View>
    );
}
