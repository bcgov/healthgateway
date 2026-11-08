import { Alert } from "react-native";

/** Native: window.confirm becomes the platform alert with Cancel / OK. */
export function confirmAction(message: string): Promise<boolean> {
    return new Promise((resolve) =>
        Alert.alert("", message, [
            { text: "Cancel", style: "cancel", onPress: () => resolve(false) },
            { text: "OK", style: "destructive", onPress: () => resolve(true) },
        ])
    );
}
