import { beforeEach, describe, expect, it, jest } from "@jest/globals";

import { COPY_DECLINED, recordCache } from "@/services/recordCache";
import { vault } from "@/services/vault";

const mockFiles = new Map<string, string>();

jest.mock("expo-file-system", () => {
    class File {
        name: string;
        constructor(_dir: unknown, id: string) {
            this.name = id;
        }
        get exists() {
            return mockFiles.has(this.name);
        }
        create() {
            mockFiles.set(this.name, "");
        }
        write(text: string) {
            mockFiles.set(this.name, text);
        }
        async text() {
            return mockFiles.get(this.name) ?? "";
        }
        delete() {
            mockFiles.delete(this.name);
        }
    }
    class Directory {
        list() {
            return [...mockFiles.keys()].map((id) => new File("doc", id));
        }
    }
    return { File, Directory, Paths: { document: "doc" } };
});
jest.mock("expo-crypto", () => ({
    CryptoDigestAlgorithm: { SHA256: "SHA-256" },
    digestStringAsync: jest.fn(async (_algorithm: string, text: string) =>
        [...text].map((c) => c.codePointAt(0)?.toString(16)).join("")
    ),
}));
jest.mock("@/services/vault", () => ({
    isVaultCancelled: (error: { code?: string }) =>
        error.code === "ERR_VAULT_CANCELLED",
    vault: {
        encrypt: jest.fn(async (text: string) => `sealed:${text}`),
        decrypt: jest.fn(async (text: string) => text.replace(/^sealed:/, "")),
    },
}));

describe("recordCache", () => {
    const stored = () => [...mockFiles.values()][0];

    beforeEach(() => {
        mockFiles.clear();
    });

    it("removing the copy stops a save already under way from bringing it back", async () => {
        const save = recordCache.save("H", { a: 1 }, 10, 10 + 100);
        await recordCache.remove();
        await expect(save).resolves.toBe(false);
        await expect(recordCache.load("H")).resolves.toBeUndefined();
    });

    it("writes only encrypted text", async () => {
        await expect(
            recordCache.save("H", { a: 1 }, 10, 10 + 100)
        ).resolves.toBe(true);
        expect(stored().split("\n")[1].startsWith("sealed:")).toBe(true);
    });

    it("reads back what it saved", async () => {
        await recordCache.save("H", { a: 1 }, 10, 10 + 100);
        await expect(recordCache.load("H")).resolves.toMatchObject({
            savedAt: 10,
            hdid: "H",
            data: { a: 1 },
        });
    });

    it("says so when the prompt to read is turned down, keeping the copy", async () => {
        await recordCache.save("H", {}, 10, 110);
        jest.mocked(vault.decrypt).mockRejectedValueOnce(
            Object.assign(new Error("no"), { code: "ERR_VAULT_CANCELLED" })
        );
        await expect(recordCache.load("H")).resolves.toBe(COPY_DECLINED);
        expect(mockFiles.size).toBe(1);
        await expect(recordCache.load("H")).resolves.toBeDefined();
    });

    it("doesn't ask to read when there is no copy", async () => {
        jest.mocked(vault.decrypt).mockClear();
        await recordCache.load("H");
        expect(vault.decrypt).not.toHaveBeenCalled();
    });

    it("reads a copy from before this format as none, without asking", async () => {
        jest.mocked(vault.decrypt).mockClear();
        await recordCache.save("H", {}, 10, 110);
        const [name] = [...mockFiles.keys()];
        mockFiles.set(name, "sealed:old format");
        await expect(recordCache.load("H")).resolves.toBeUndefined();
        expect(vault.decrypt).not.toHaveBeenCalled();
    });

    it("keeps only the expiry readable", async () => {
        await recordCache.save("PERSON", { secret: "x" }, 10, 110);
        const [header] = stored().split("\n");
        expect(JSON.parse(header)).toEqual({ version: 3, expiresAt: 110 });
    });

    it("never gives one person's copy to another", async () => {
        await recordCache.save("H", {}, 10, 10 + 100);
        await expect(recordCache.load("OTHER")).resolves.toBeUndefined();
    });

    it("treats an unreadable copy as none", async () => {
        await recordCache.save("H", {}, 10, 10 + 100);
        jest.mocked(vault.decrypt).mockRejectedValueOnce(new Error("key lost"));
        await expect(recordCache.load("H")).resolves.toBeUndefined();
    });

    it("has nothing to read when there is no file", async () => {
        await expect(recordCache.load("H")).resolves.toBeUndefined();
    });

    it("removes the file", async () => {
        await recordCache.save("H", {}, 10, 10 + 100);
        await recordCache.remove();
        expect(mockFiles.size).toBe(0);
    });

    it("keeps a separate copy per person, with no hdid in the file name", async () => {
        await recordCache.save("PERSON-A", { who: "a" }, 10, 10 + 100);
        await recordCache.save("PERSON-B", { who: "b" }, 10, 10 + 100);
        expect(mockFiles.size).toBe(2);
        expect([...mockFiles.keys()].join()).not.toContain("PERSON");
        await expect(recordCache.load("PERSON-A")).resolves.toMatchObject({
            data: { who: "a" },
        });
        await expect(recordCache.load("PERSON-B")).resolves.toMatchObject({
            data: { who: "b" },
        });
    });

    it("removing deletes every person's copy, and nothing else", async () => {
        await recordCache.save("PERSON-A", {}, 10, 10 + 100);
        await recordCache.save("PERSON-B", {}, 10, 10 + 100);
        mockFiles.set("other.txt", "keep");
        await recordCache.remove();
        expect([...mockFiles.keys()]).toEqual(["other.txt"]);
    });

    it("removeFor deletes only that person's copy", async () => {
        await recordCache.save("PERSON-A", {}, 10, 10 + 100);
        await recordCache.save("PERSON-B", {}, 10, 10 + 100);
        await recordCache.removeFor("PERSON-A");
        await expect(recordCache.load("PERSON-A")).resolves.toBeUndefined();
        await expect(recordCache.load("PERSON-B")).resolves.toBeDefined();
    });

    describe("removeExpired", () => {
        it("removes another person's expired copy and keeps the rest, without asking", async () => {
            await recordCache.save("KEEP", {}, 1, 2);
            await recordCache.save("OLD", {}, 1, 50);
            await recordCache.save("RECENT", {}, 1, 51);
            jest.mocked(vault.decrypt).mockClear();
            await recordCache.removeExpired("KEEP", 50);
            expect(vault.decrypt).not.toHaveBeenCalled();
            await expect(recordCache.load("KEEP")).resolves.toBeDefined();
            await expect(recordCache.load("OLD")).resolves.toBeUndefined();
            await expect(recordCache.load("RECENT")).resolves.toBeDefined();
            expect(mockFiles.size).toBe(2);
        });

        it("removes a copy from another version", async () => {
            await recordCache.save("KEEP", {}, 1, 100);
            mockFiles.set("hg-record-cache.enc", "sealed:not json");
            await recordCache.removeExpired("KEEP", 50);
            expect(mockFiles.size).toBe(1);
        });
    });
});
