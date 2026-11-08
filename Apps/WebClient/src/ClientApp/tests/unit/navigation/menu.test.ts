import { describe, expect, it } from "@jest/globals";

import type { FeatureToggleConfiguration } from "@/models/configData";
import { menuRouteName, visibleMenuItems } from "@/navigation/menu";

const toggles = (dependents: boolean, services: boolean) =>
    ({
        dependents: { enabled: dependents },
        services: { enabled: services },
    }) as unknown as FeatureToggleConfiguration;

describe("visibleMenuItems", () => {
    it("shows every item when the toggles allow it", () => {
        expect(
            visibleMenuItems(toggles(true, true)).map((m) => m.title)
        ).toEqual([
            "Home",
            "Health Records",
            "Dependents",
            "Services",
            "Download",
        ]);
    });

    it("drops Dependents and Services when their toggles are off", () => {
        expect(
            visibleMenuItems(toggles(false, false)).map((m) => m.title)
        ).toEqual(["Home", "Health Records", "Download"]);
    });

    it("shows nothing before the configuration is loaded", () => {
        expect(visibleMenuItems(undefined)).toEqual([]);
    });
});

describe("menuRouteName", () => {
    it("maps the web path to the route name inside the (tabs) group", () => {
        expect(menuRouteName("/home")).toBe("(home)");
        expect(menuRouteName("/dependents")).toBe("dependents");
    });
});
