import { describe, expect, it } from "@jest/globals";

import { services } from "@/services/container";
import { configFixture, fakeAuth } from "@/test/fixtures";

describe("service container", () => {
    it("exposes http and config from the start and the rest only after initialisation", () => {
        expect(services.http).toBeDefined();
        expect(services.config).toBeDefined();
        expect(services.tracking).toBeDefined();
        if (!services.isInitialized) {
            expect(() => services.userProfile).toThrow(/before initialisation/);
        }
        services.initialize(configFixture, fakeAuth());
        expect(services.isInitialized).toBe(true);
        expect(services.patient).toBeDefined();
        expect(services.userProfile).toBeDefined();
        expect(services.communication).toBeDefined();
        expect(services.userFeedback).toBeDefined();
        expect(services.auth).toBeDefined();
    });
});
