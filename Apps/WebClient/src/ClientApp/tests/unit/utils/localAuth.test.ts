import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import * as LocalAuthentication from "expo-local-authentication";
import { Platform } from "react-native";

import { getLocalAuthMethod } from "@/utils/localAuth";

jest.mock("expo-local-authentication", () => ({
    SecurityLevel: {
        NONE: 0,
        SECRET: 1,
        BIOMETRIC_WEAK: 2,
        BIOMETRIC_STRONG: 3,
    },
    AuthenticationType: { FINGERPRINT: 1, FACIAL_RECOGNITION: 2, IRIS: 3 },
    getEnrolledLevelAsync: jest.fn(),
    supportedAuthenticationTypesAsync: jest.fn(),
}));

const module_ = jest.mocked(LocalAuthentication);
const { SecurityLevel, AuthenticationType } = LocalAuthentication;
const originalOS = Platform.OS;
const setOS = (os: string) =>
    Object.defineProperty(Platform, "OS", { configurable: true, value: os });

describe("localAuth", () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    afterEach(() => {
        setOS(originalOS);
    });

    describe("getLocalAuthMethod", () => {
        it("reports Face ID on an iPhone with it", async () => {
            setOS("ios");
            module_.getEnrolledLevelAsync.mockResolvedValue(
                SecurityLevel.BIOMETRIC_STRONG
            );
            module_.supportedAuthenticationTypesAsync.mockResolvedValue([
                AuthenticationType.FACIAL_RECOGNITION,
            ]);
            await expect(getLocalAuthMethod()).resolves.toBe("faceId");
        });

        it("reports Touch ID on an iPhone with a fingerprint sensor", async () => {
            setOS("ios");
            module_.getEnrolledLevelAsync.mockResolvedValue(
                SecurityLevel.BIOMETRIC_STRONG
            );
            module_.supportedAuthenticationTypesAsync.mockResolvedValue([
                AuthenticationType.FINGERPRINT,
            ]);
            await expect(getLocalAuthMethod()).resolves.toBe("touchId");
        });

        it.each([
            ["fingerprint", SecurityLevel.BIOMETRIC_STRONG],
            ["camera face unlock", SecurityLevel.BIOMETRIC_WEAK],
        ])("reports biometrics on Android for %s", async (_name, level) => {
            setOS("android");
            module_.getEnrolledLevelAsync.mockResolvedValue(level);
            await expect(getLocalAuthMethod()).resolves.toBe("biometrics");
        });

        it("reports a passcode-only device", async () => {
            setOS("ios");
            module_.getEnrolledLevelAsync.mockResolvedValue(
                SecurityLevel.SECRET
            );
            await expect(getLocalAuthMethod()).resolves.toBe("passcode");
        });

        it("reports a device with nothing set up", async () => {
            setOS("android");
            module_.getEnrolledLevelAsync.mockResolvedValue(SecurityLevel.NONE);
            await expect(getLocalAuthMethod()).resolves.toBe("none");
        });

        it("reports nothing when the native module is missing", async () => {
            setOS("ios");
            module_.getEnrolledLevelAsync.mockRejectedValue(
                new Error("no native module")
            );
            await expect(getLocalAuthMethod()).resolves.toBe("none");
        });

        it("reports nothing on web without touching the module", async () => {
            setOS("web");
            await expect(getLocalAuthMethod()).resolves.toBe("none");
            expect(module_.getEnrolledLevelAsync).not.toHaveBeenCalled();
        });
    });
});
