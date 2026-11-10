import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { readdirSync, readFileSync, statSync } from "fs";
import { join } from "path";

import { HttpClient } from "@/services/http/httpClient";
import { describeError, logger } from "@/services/logger";
import { createTrackingService as nativeTracker } from "@/services/tracking/trackingService.impl.native";
import { createTrackingService as webTracker } from "@/services/tracking/trackingService.impl.web";
import { UserCommentService } from "@/services/userComment.service";
import { UserNoteService } from "@/services/userNote.service";
import { UserProfileService } from "@/services/userProfile.service";
import { configFixture, FakeHttp } from "@/test/fixtures";

jest.mock("@snowplow/browser-tracker", () => ({
    newTracker: jest.fn(),
    trackPageView: jest.fn(),
    trackSelfDescribingEvent: jest.fn(),
    enableActivityTracking: jest.fn(),
    addPlugin: jest.fn(),
}));
jest.mock("@snowplow/browser-plugin-link-click-tracking", () => ({
    LinkClickTrackingPlugin: jest.fn(),
    enableLinkClickTracking: jest.fn(),
}));
jest.mock("@snowplow/browser-plugin-performance-timing", () => ({
    PerformanceTimingPlugin: jest.fn(),
}));

const SECRET = "my private note about my diagnosis";

describe("logs at their most detailed hold nothing personal", () => {
    const lines: string[] = [];
    beforeEach(() => {
        lines.length = 0;
        const capture = (...args: unknown[]) => {
            lines.push(args.map(String).join(" "));
        };
        jest.spyOn(console, "debug").mockImplementation(capture);
        jest.spyOn(console, "info").mockImplementation(capture);
        jest.spyOn(console, "warn").mockImplementation(capture);
        jest.spyOn(console, "error").mockImplementation(capture);
        logger.setLevel("verbose");
    });
    afterEach(() => {
        logger.setLevel("silent");
        jest.restoreAllMocks();
    });

    const reply = () =>
        new FakeHttp().reply({
            resourcePayload: { text: SECRET },
            resultStatus: 1,
        });

    it("notes", async () => {
        const note = { title: SECRET, text: SECRET } as never;
        for (const call of ["createNote", "updateNote", "deleteNote"] as const)
            await new UserNoteService(reply(), configFixture)[call](
                "HDID",
                note
            );
        expect(lines.join("\n")).not.toContain(SECRET);
    });

    it("comments", async () => {
        const comment = { text: SECRET } as never;
        for (const call of [
            "createComment",
            "updateComment",
            "deleteComment",
        ] as const)
            await new UserCommentService(reply(), configFixture)[call](
                "HDID",
                comment
            );
        expect(lines.join("\n")).not.toContain(SECRET);
    });

    it("a server error's text stays out", async () => {
        jest.spyOn(global, "fetch").mockResolvedValue(
            new Response(JSON.stringify({ detail: SECRET, traceId: "t-1" }), {
                status: 500,
            })
        );
        const service = new UserProfileService(new HttpClient(), configFixture);
        await expect(service.getProfile("HDID")).rejects.toBeDefined();
        const logged = lines.join("\n");
        expect(logged).not.toContain(SECRET);
        expect(logged).toContain("500");
        expect(logged).toContain("t-1");
    });

    it("a tracking event's free text stays out, on both trackers", () => {
        for (const tracker of [nativeTracker(), webTracker()]) {
            tracker.trackEvent({
                action: "click",
                text: SECRET,
                url: `https://x/${SECRET}`,
            } as never);
            tracker.trackPageView(`/dependents/${SECRET}/timeline`);
        }
        expect(lines.join("\n")).toContain("click");
        expect(lines.join("\n")).not.toContain(SECRET);
    });

    it.each([
        ["a server message", new Error(SECRET)],
        ["an object", { detail: SECRET }],
        ["text", SECRET],
    ])("describeError leaves out %s", (_name, error) => {
        expect(describeError(error)).not.toContain(SECRET);
    });
});

describe("source guard", () => {
    const files = (dir: string): string[] =>
        readdirSync(dir).flatMap((name) => {
            const path = join(dir, name);
            if (statSync(path).isDirectory()) return files(path);
            return /\.tsx?$/.test(name) ? [path] : [];
        });

    /** What is known to hold nothing personal. A new value in a log needs that decided, here. */
    const SAFE_IN_LOGS = new Set([
        "canAccess",
        "config.collector",
        "configured",
        "data.action",
        "detail",
        "e?.error",
        "errorCode(error)",
        "errorName",
        "getDataSource(entryType)",
        "isValidState",
        "kind",
        "name",
        "options.name",
        "response.status",
        "result.type",
        "state",
        "this.redirectUri",
        "tile.type",
    ]);

    it("logs interpolate only values known to be safe", () => {
        const bad: string[] = [];
        for (const file of files(join(process.cwd(), "src"))) {
            if (file.endsWith("services/logger.ts")) continue;
            const text = readFileSync(file, "utf8");
            for (const match of text.matchAll(
                /logger\.(?:verbose|debug|info|warn|error)\(/g
            )) {
                let depth = 1;
                let i = match.index + match[0].length;
                const start = i;
                while (depth > 0 && i < text.length) {
                    if (text[i] === "(") depth += 1;
                    if (text[i] === ")") depth -= 1;
                    i += 1;
                }
                const call = text.slice(start, i - 1);
                for (const value of call.matchAll(/\$\{([^}]*)\}/g)) {
                    const name = value[1].replace(/\s+/g, "");
                    if (
                        !SAFE_IN_LOGS.has(name) &&
                        !name.startsWith("describeError(")
                    )
                        bad.push(name);
                }
            }
        }
        expect([...new Set(bad)].sort()).toEqual([]);
    });

    it("nothing logs a raw error, its message, or a whole object", () => {
        const bad: string[] = [];
        for (const file of files(join(process.cwd(), "src"))) {
            if (file.endsWith("services/logger.ts")) continue;
            const text = readFileSync(file, "utf8");
            for (const match of text.matchAll(
                /logger\.(?:verbose|debug|info|warn|error)\(/g
            )) {
                let depth = 1;
                let i = match.index + match[0].length;
                const start = i;
                while (depth > 0 && i < text.length) {
                    if (text[i] === "(") depth += 1;
                    if (text[i] === ")") depth -= 1;
                    i += 1;
                }
                const call = text.slice(start, i - 1);
                if (/JSON\.stringify|\.message|String\(/.test(call))
                    bad.push(`${file}: ${call.trim().slice(0, 60)}`);
            }
        }
        expect(bad).toEqual([]);
    });
});
