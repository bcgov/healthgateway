import { View } from "react-native";

/**
 * Web has no screens over the app (no lock screen, intro or Update required: it always runs the latest), so no bar for
 * them: the content as it is. Native: `OverAppScreen.tsx`.
 */
export function OverAppScreen({ children }: { children: React.ReactNode }) {
    return (
        <View testID="overAppScreen" className="flex-1 bg-background">
            {children}
        </View>
    );
}
