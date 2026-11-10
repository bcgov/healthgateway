import { describe, expect, it } from "@jest/globals";
import { Platform } from "react-native";

import { SENSITIVE_DOCUMENT_MESSAGE } from "@/constants/sensitiveDocument";

describe("SENSITIVE_DOCUMENT_MESSAGE", () => {
    it("says shared device on native and keeps the web client's wording on web", () => {
        const expected =
            Platform.OS === "web"
                ? "If you are on a public computer, please"
                : "If you are on a shared device, please";
        expect(SENSITIVE_DOCUMENT_MESSAGE).toContain(expected);
    });
});
