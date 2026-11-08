import { Image, type ImageSourcePropType, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

/**
 * React twin of the native launch screen (myhealthBC-iOS LaunchScreen.storyboard):
 * background #103362, centred logo-with-title (161×136 pt), bc-logo 140×35 pt, 34 pt above the safe area.
 */
export function SplashView() {
    return (
        <SafeAreaView className="flex-1 bg-splash-background">
            <View className="flex-1 items-center justify-center">
                <Image
                    source={
                        require("@/assets/images/splash/splash-logo-with-title.png") as ImageSourcePropType
                    }
                    style={{ width: 161, height: 136 }}
                    resizeMode="contain"
                    accessibilityLabel="Health Gateway"
                />
            </View>
            <View className="items-center" style={{ paddingBottom: 34 }}>
                <Image
                    source={
                        require("@/assets/images/splash/bc-logo.png") as ImageSourcePropType
                    }
                    style={{ width: 140, height: 35 }}
                    resizeMode="contain"
                />
            </View>
        </SafeAreaView>
    );
}
