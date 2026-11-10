import { Platform } from "react-native";

/** v-elevation 1 (MD3): key + ambient shadows */
export const elevation1 =
    Platform.OS === "web"
        ? {
              boxShadow:
                  "0px 1px 2px 0px rgba(0,0,0,0.3), 0px 1px 3px 1px rgba(0,0,0,0.15)",
          }
        : {
              elevation: 1,
              shadowColor: "#000",
              shadowOpacity: 0.2,
              shadowRadius: 2,
              shadowOffset: { width: 0, height: 1 },
          };
