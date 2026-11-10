import { View } from "react-native";

import { useColorScheme } from "@/hooks/useTheme";

/**
 * Third-party brand logos keep their own colours, so in dark mode they sit on a small white plate
 * (as brand guidelines usually require); in light mode the children render unchanged.
 */
export function LogoPlate({ children }: { children: React.ReactNode }) {
    if (useColorScheme() !== "dark") return <>{children}</>;
    return (
        <View className="self-start rounded bg-brand-plate px-1.5 py-1">
            {children}
        </View>
    );
}
