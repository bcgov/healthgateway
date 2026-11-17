import { beforeEach, describe, expect, it, jest } from "@jest/globals";

import { COPY_DECLINED, recordCache } from "@/services/recordCache";
import { vault } from "@/services/vault";

/** Files by path: "doc/<name>" is loose in Documents, "doc/hg-record-cache/<name>" is in the copies' folder. */
const mockFiles = new Map<string, string>();
const FOLDER = "doc/hg-record-cache";
const dirOf = (path: string) => path.slice(0, path.lastIndexOf("/"));

jest.mock("expo-file-system", () => {
    const pathOf = (part: unknown) =>
        typeof part === "string" ? part : (part as { path: string }).path;
    class File {
        path: string;
        name: string;
        constructor(dir: unknown, name: string) {
            this.path = `${pathOf(dir)}/${name}`;
            this.name = name;
        }
        get exists() {
            return mockFiles.has(this.path);
        }
        create() {
            mockFiles.set(this.path, "");
        }
        write(text: string) {
            mockFiles.set(this.path, text);
        }
        async text() {
            return mockFiles.get(this.path) ?? "";
        }
        delete() {
            mockFiles.delete(this.path);
        }
        async move(dir: unknown) {
            const text = mockFiles.get(this.path) ?? "";
            mockFiles.delete(this.path);
            this.path = `${pathOf(dir)}/${this.name}`;
            mockFiles.set(this.path, text);
        }
    }
    class Directory {
        path: string;
        constructor(...parts: unknown[]) {
            this.path = parts.map(pathOf).join("/");
        }
        get uri() {
            return `file:///${this.path}/`;
        }
        create() {}
        list() {
            return [...mockFiles.keys()]
                .filter(
                    (path) => path.slice(0, path.lastIndexOf("/")) === this.path
                )
                .map(
                    (path) =>
                        new File(
                            this.path,
                            path.slice(path.lastIndexOf("/") + 1)
                        )
                );
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
        excludeFromBackup: jest.fn(async () => undefined),
    },
}));

describe("recordCache", () => {
    const stored = () => [...mockFiles.values()][0];

    beforeEach(() => {
        mockFiles.clear();
    });

    it("removing the copy stops a save already under way from bringing it back", async () => {
        const save = recordCache.save("H", { a: 1 }, 10);
        await recordCache.remove();
        await expect(save).resolves.toBe(false);
        await expect(recordCache.load("H")).resolves.toBeUndefined();
    });

    it("writes only encrypted text", async () => {
        await expect(recordCache.save("H", { a: 1 }, 10)).resolves.toBe(true);
        expect(stored().split("\n")[1].startsWith("sealed:")).toBe(true);
    });

    it("reads back what it saved", async () => {
        await recordCache.save("H", { a: 1 }, 10);
        await expect(recordCache.load("H")).resolves.toMatchObject({
            savedAt: 10,
            hdid: "H",
            data: { a: 1 },
        });
    });

    it("keeps the copies in their own folder, kept out of backups", async () => {
        await recordCache.save("H", {}, 10);
        expect(
            [...mockFiles.keys()].every((path) => dirOf(path) === FOLDER)
        ).toBe(true);
        expect(vault.excludeFromBackup).toHaveBeenCalledWith(
            "file:///doc/hg-record-cache/"
        );
    });

    it("says so when the prompt to read is turned down, keeping the copy", async () => {
        await recordCache.save("H", {}, 10);
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
        await recordCache.save("H", {}, 10);
        const [path] = [...mockFiles.keys()];
        mockFiles.set(path, "sealed:old format");
        await expect(recordCache.load("H")).resolves.toBeUndefined();
        expect(vault.decrypt).not.toHaveBeenCalled();
    });

    it("keeps only the version readable", async () => {
        await recordCache.save("PERSON", { secret: "x" }, 10);
        const [header] = stored().split("\n");
        expect(JSON.parse(header)).toEqual({ version: 3 });
    });

    it("still reads a copy saved with an expiry, ignoring it", async () => {
        await recordCache.save("H", { a: 1 }, 10);
        const [path] = [...mockFiles.keys()];
        const [, sealed] = (mockFiles.get(path) ?? "").split("\n");
        mockFiles.set(
            path,
            `${JSON.stringify({ version: 3, expiresAt: 1 })}\n${sealed}`
        );
        await expect(recordCache.load("H")).resolves.toMatchObject({
            data: { a: 1 },
        });
    });

    it("never gives one person's copy to another", async () => {
        await recordCache.save("H", {}, 10);
        await expect(recordCache.load("OTHER")).resolves.toBeUndefined();
    });

    it("treats an unreadable copy as none", async () => {
        await recordCache.save("H", {}, 10);
        jest.mocked(vault.decrypt).mockRejectedValueOnce(new Error("key lost"));
        await expect(recordCache.load("H")).resolves.toBeUndefined();
    });

    it("has nothing to read when there is no file", async () => {
        await expect(recordCache.load("H")).resolves.toBeUndefined();
    });

    it("removes the file", async () => {
        await recordCache.save("H", {}, 10);
        await recordCache.remove();
        expect(mockFiles.size).toBe(0);
    });

    it("keeps a separate copy per person, with no hdid in the file name", async () => {
        await recordCache.save("PERSON-A", { who: "a" }, 10);
        await recordCache.save("PERSON-B", { who: "b" }, 10);
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
        await recordCache.save("PERSON-A", {}, 10);
        await recordCache.save("PERSON-B", {}, 10);
        mockFiles.set("doc/other.txt", "keep");
        await recordCache.remove();
        expect([...mockFiles.keys()]).toEqual(["doc/other.txt"]);
    });

    it("removeFor deletes only that person's copy", async () => {
        await recordCache.save("PERSON-A", {}, 10);
        await recordCache.save("PERSON-B", {}, 10);
        await recordCache.removeFor("PERSON-A");
        await expect(recordCache.load("PERSON-A")).resolves.toBeUndefined();
        await expect(recordCache.load("PERSON-B")).resolves.toBeDefined();
    });

    describe("removeOutdated", () => {
        it("keeps everyone's current copies, however old, without asking", async () => {
            await recordCache.save("KEEP", {}, 1);
            await recordCache.save("SOMEONE-ELSE", {}, 1);
            mockFiles.forEach((text, path) =>
                mockFiles.set(
                    path,
                    text.replace('{"version":3}', '{"version":3,"expiresAt":2}')
                )
            );
            jest.mocked(vault.decrypt).mockClear();
            await recordCache.removeOutdated();
            expect(vault.decrypt).not.toHaveBeenCalled();
            expect(mockFiles.size).toBe(2);
        });

        it("removes a copy from another version", async () => {
            await recordCache.save("KEEP", {}, 1);
            mockFiles.set(`${FOLDER}/hg-record-cache.enc`, "sealed:not json");
            await recordCache.removeOutdated();
            expect(mockFiles.size).toBe(1);
        });
    });

    it("moves copies saved before the folder existed into it, once per launch", async () => {
        await jest.isolateModulesAsync(async () => {
            const fresh = (await import("@/services/recordCache")).recordCache;
            await recordCache.save("H", { a: 1 }, 10);
            const [path] = [...mockFiles.keys()];
            const name = path.slice(path.lastIndexOf("/") + 1);
            mockFiles.delete(path);
            mockFiles.set(`doc/${name}`, "loose");
            mockFiles.set("doc/other.txt", "keep");
            await fresh.removeOutdated();
            expect(mockFiles.has(`doc/${name}`)).toBe(false);
            expect(mockFiles.has("doc/other.txt")).toBe(true);
            // "loose" isn't a current copy, so tidying then removed it from the folder
            expect(mockFiles.has(`${FOLDER}/${name}`)).toBe(false);
        });
    });

    it("moves a loose current copy into the folder, where it still opens", async () => {
        await jest.isolateModulesAsync(async () => {
            await recordCache.save("H", { a: 1 }, 10);
            const [path] = [...mockFiles.keys()];
            const text = mockFiles.get(path) ?? "";
            const name = path.slice(path.lastIndexOf("/") + 1);
            mockFiles.delete(path);
            mockFiles.set(`doc/${name}`, text);
            const fresh = (await import("@/services/recordCache")).recordCache;
            await expect(fresh.load("H")).resolves.toMatchObject({
                data: { a: 1 },
            });
            expect([...mockFiles.keys()]).toEqual([`${FOLDER}/${name}`]);
        });
    });

    it("still saves when the folder can't be kept out of backups", async () => {
        await jest.isolateModulesAsync(async () => {
            // the isolated modules get their own vault mock
            const freshVault = (await import("@/services/vault")).vault;
            jest.mocked(freshVault.excludeFromBackup).mockRejectedValueOnce(
                new Error("no")
            );
            const fresh = (await import("@/services/recordCache")).recordCache;
            await expect(fresh.save("H", {}, 10)).resolves.toBe(true);
        });
    });
});
