import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import type { SaveDocumentsOptions } from "@react-native-documents/picker";
import { Platform } from "react-native";

import { endSessionGeneration } from "@/services/sessionFence";
import { clearSavedFiles, saveBase64File } from "@/utils/saveFile";

const written: { name?: string; content?: string; encoding?: string } = {};
let fileOnDisk = false;
const mockFolders = new Set<string>();
class MockDirectory {
    uri: string;
    constructor(parent: string | { uri: string }, name: string) {
        this.uri = `${typeof parent === "string" ? parent : parent.uri}${name}/`;
    }
    get exists() {
        return mockFolders.has(this.uri);
    }
    create() {
        mockFolders.add(this.uri);
    }
    delete() {
        for (const uri of [...mockFolders])
            if (uri.startsWith(this.uri)) mockFolders.delete(uri);
    }
    list() {
        return [...mockFolders]
            .filter(
                (uri) =>
                    uri !== this.uri &&
                    uri.startsWith(this.uri) &&
                    !uri.slice(this.uri.length, -1).includes("/")
            )
            .map((uri) => {
                const entry = new MockDirectory("", "");
                entry.uri = uri;
                return entry;
            });
    }
}
jest.mock("expo-file-system", () => ({
    Paths: { cache: "cache://" },
    Directory: MockDirectory,
    File: class {
        uri: string;
        exists = false;
        constructor(dir: string | { uri: string }, name: string) {
            this.uri = `${typeof dir === "string" ? dir : dir.uri}${name}`;
            written.name = name;
        }
        delete() {}
        // expo-file-system 58: write is asynchronous (writeSync is the blocking form)
        async write(content: string, options: { encoding: string }) {
            await Promise.resolve();
            written.content = content;
            written.encoding = options.encoding;
            fileOnDisk = true;
        }
    },
}));
const mockSaveDocuments = jest.fn(async (_options: SaveDocumentsOptions) => {
    // the native side refuses a file that is not there yet
    if (!fileOnDisk) throw new Error("The file does not exist");
    return [{ uri: "files://report.pdf", name: "report.pdf", error: null }];
});
const mockPickerModule = () => ({
    errorCodes: { OPERATION_CANCELED: "OPERATION_CANCELED" },
    isErrorWithCode: (e: unknown) =>
        typeof e === "object" && e !== null && "code" in e,
    saveDocuments: (options: SaveDocumentsOptions) =>
        mockSaveDocuments(options),
});
jest.mock("@react-native-documents/picker", () => mockPickerModule());

const mockUnableToOpen = () =>
    Object.assign(new Error("no app"), { code: "UNABLE_TO_OPEN_FILE_TYPE" });
const showFile = async (_options: object) => {
    // the native side shows the file: it must be there
    if (!fileOnDisk) throw new Error("The file does not exist");
    return null;
};
const mockViewDocument = jest.fn(showFile);
const mockViewerModule = () => ({
    errorCodes: { UNABLE_TO_OPEN_FILE_TYPE: "UNABLE_TO_OPEN_FILE_TYPE" },
    isErrorWithCode: (e: unknown) =>
        typeof e === "object" && e !== null && "code" in e,
    viewDocument: (options: object) => mockViewDocument(options),
});
jest.mock("@react-native-documents/viewer", () => mockViewerModule());

describe("saveBase64File", () => {
    if (Platform.OS !== "web") {
        const save = () => saveBase64File("QUJD", "application/pdf", "r.pdf");
        const exports = () =>
            [...mockFolders].filter((uri) => uri !== "cache://hg-exports/");

        beforeEach(() => {
            mockFolders.clear();
            fileOnDisk = false;
            mockViewDocument.mockClear();
            mockSaveDocuments.mockClear();
        });

        it("previews the file and keeps it while the viewer shows it", async () => {
            await save();
            expect(mockViewDocument).toHaveBeenCalledWith({
                uri: expect.stringMatching(
                    /^cache:\/\/hg-exports\/\d+\/r\.pdf$/
                ),
                mimeType: "application/pdf",
                headerTitle: "r.pdf",
                presentationStyle: "fullScreen",
            });
            expect(mockSaveDocuments).not.toHaveBeenCalled();
            expect(exports()).toHaveLength(1);
        });

        it("the next export clears the previewed file", async () => {
            await save();
            const previewed = exports()[0];
            await save();
            expect(mockFolders.has(previewed)).toBe(false);
            expect(exports()).toHaveLength(1);
        });

        it("clearSavedFiles removes the previewed file", async () => {
            await save();
            await clearSavedFiles();
            expect(exports()).toEqual([]);
        });

        it("clearSavedFiles leaves an export that is still being written", async () => {
            let finish: () => void = () => undefined;
            mockViewDocument.mockImplementationOnce(async () => {
                await new Promise<void>((resolve) => {
                    finish = resolve;
                });
                return null;
            });
            const first = save();
            await new Promise((resolve) => setImmediate(resolve));
            await clearSavedFiles();
            expect(exports()).toHaveLength(1);
            finish();
            await first;
        });

        it("signing out while the file is written shows nothing and removes it", async () => {
            const export_ = save();
            // sign-out: the session ends and clearSavedFiles leaves the folder being written
            endSessionGeneration();
            await clearSavedFiles();
            await export_;
            expect(mockViewDocument).not.toHaveBeenCalled();
            expect(mockSaveDocuments).not.toHaveBeenCalled();
            expect(exports()).toEqual([]);
        });

        it("signing out while the viewer opens removes the previewed file", async () => {
            mockViewDocument.mockImplementationOnce(async (options) => {
                const shown = await showFile(options);
                endSessionGeneration();
                await clearSavedFiles();
                return shown;
            });
            await save();
            expect(mockViewDocument).toHaveBeenCalledTimes(1);
            expect(exports()).toEqual([]);
        });

        it("signing out while the viewer opens does not fall back to the save sheet", async () => {
            mockViewDocument.mockImplementationOnce(async () => {
                endSessionGeneration();
                throw mockUnableToOpen();
            });
            await save();
            expect(mockSaveDocuments).not.toHaveBeenCalled();
            expect(exports()).toEqual([]);
        });

        it("a viewer failure is reported and the file removed", async () => {
            mockViewDocument.mockRejectedValueOnce(new Error("no presenter"));
            await expect(save()).rejects.toThrow("no presenter");
            expect(mockSaveDocuments).not.toHaveBeenCalled();
            expect(exports()).toEqual([]);
        });

        /**
         * Runs the export against a fresh module registry whose viewer or picker module comes from the given factory,
         * which runs when saveFile lazily imports it. resetModules, not isolateModules: an isolated registry still
         * returns the mock already made.
         */
        async function saveWithFreshModules(factories: {
            viewer?: () => unknown;
            picker?: () => unknown;
        }) {
            jest.resetModules();
            if (factories.viewer)
                jest.doMock("@react-native-documents/viewer", factories.viewer);
            if (factories.picker)
                jest.doMock("@react-native-documents/picker", factories.picker);
            try {
                // the fresh registry has its own logger, which jest.setup did not silence
                const { logger } = await import("@/services/logger");
                const warn = jest
                    .spyOn(logger, "warn")
                    .mockImplementation(() => undefined);
                const fresh = await import("@/utils/saveFile");
                await fresh.saveBase64File("QUJD", "application/pdf", "r.pdf");
                return warn;
            } finally {
                jest.resetModules();
                jest.doMock("@react-native-documents/viewer", () =>
                    mockViewerModule()
                );
                jest.doMock("@react-native-documents/picker", () =>
                    mockPickerModule()
                );
            }
        }
        // the fresh registry's session, which the fresh saveFile reads
        const endFreshSession = () =>
            jest
                .requireActual<typeof import("@/services/sessionFence")>(
                    "@/services/sessionFence"
                )
                .endSessionGeneration();

        it("without the viewer's native module, opens the save sheet", async () => {
            // a dev client built before the viewer was added: importing it throws (TurboModuleRegistry.getEnforcing)
            const warn = await saveWithFreshModules({
                viewer: () => {
                    throw new Error("RNDocumentViewer could not be found");
                },
            });
            expect(warn).toHaveBeenCalledWith(
                expect.stringContaining("No document viewer")
            );
            expect(mockViewDocument).not.toHaveBeenCalled();
            expect(mockSaveDocuments).toHaveBeenCalledTimes(1);
            expect(exports()).toEqual([]);
        });

        it("signing out while the viewer loads shows nothing and removes the file", async () => {
            await saveWithFreshModules({
                viewer: () => {
                    endFreshSession();
                    return mockViewerModule();
                },
            });
            expect(mockViewDocument).not.toHaveBeenCalled();
            expect(mockSaveDocuments).not.toHaveBeenCalled();
            expect(exports()).toEqual([]);
        });

        it("signing out while the save sheet loads opens nothing and removes the file", async () => {
            mockViewDocument.mockImplementationOnce(async () => {
                throw mockUnableToOpen();
            });
            await saveWithFreshModules({
                picker: () => {
                    endFreshSession();
                    return mockPickerModule();
                },
            });
            expect(mockViewDocument).toHaveBeenCalledTimes(1);
            expect(mockSaveDocuments).not.toHaveBeenCalled();
            expect(exports()).toEqual([]);
        });
    }
});

describe("saveBase64File with no app for the file type", () => {
    if (Platform.OS !== "web") {
        const save = () => saveBase64File("QUJD", "application/pdf", "r.pdf");
        const exports = () =>
            [...mockFolders].filter((uri) => uri !== "cache://hg-exports/");

        beforeEach(() => {
            mockFolders.clear();
            fileOnDisk = false;
            mockSaveDocuments.mockClear();
            mockViewDocument.mockImplementation(async () => {
                throw mockUnableToOpen();
            });
        });
        afterEach(() => {
            mockViewDocument.mockImplementation(showFile);
        });

        it("opens the save sheet instead", async () => {
            await save();
            expect(mockSaveDocuments).toHaveBeenCalledWith({
                sourceUris: [
                    expect.stringMatching(
                        /^cache:\/\/hg-exports\/\d+\/r\.pdf$/
                    ),
                ],
                mimeType: "application/pdf",
                fileName: "r.pdf",
            });
        });

        it("removes the file after saving", async () => {
            await save();
            expect(exports()).toEqual([]);
        });

        it("removes the file when the sheet is cancelled", async () => {
            mockSaveDocuments.mockRejectedValueOnce(
                Object.assign(new Error("no"), { code: "OPERATION_CANCELED" })
            );
            await expect(save()).resolves.toBeUndefined();
            expect(exports()).toEqual([]);
        });

        it("removes the file when saving fails, and reports it", async () => {
            mockSaveDocuments.mockRejectedValueOnce(new Error("disk full"));
            await expect(save()).rejects.toThrow("disk full");
            expect(exports()).toEqual([]);
        });

        it("clears what an app that died left behind", async () => {
            mockFolders.add("cache://hg-exports/");
            mockFolders.add("cache://hg-exports/left-behind/");
            await save();
            expect(exports()).toEqual([]);
        });

        it("two exports at once leave each other's file alone", async () => {
            let finishFirst: () => void = () => undefined;
            mockSaveDocuments.mockImplementationOnce(async () => {
                await new Promise<void>((resolve) => {
                    finishFirst = resolve;
                });
                return [{ uri: "f", name: "a", error: null }];
            });
            const first = saveBase64File("QUJD", "application/pdf", "a.pdf");
            await new Promise((resolve) => setImmediate(resolve));
            expect(exports()).toHaveLength(1);
            const open = exports()[0];

            await saveBase64File("REVG", "application/pdf", "b.pdf");
            expect(mockFolders.has(open)).toBe(true);

            finishFirst();
            await first;
            expect(exports()).toEqual([]);
        });
    }

    afterEach(() => {
        jest.restoreAllMocks();
    });

    it("native: writes the base64 file to the cache and previews it; web: downloads it", async () => {
        if (Platform.OS === "web") {
            const g = globalThis as unknown as {
                URL: { createObjectURL: unknown; revokeObjectURL: unknown };
                fetch: unknown;
            };
            g.URL.createObjectURL = jest.fn(() => "blob:report");
            g.URL.revokeObjectURL = jest.fn();
            g.fetch = jest.fn(async () => ({
                blob: async () => new Blob(["ABC"]),
            }));
            const click = jest
                .spyOn(HTMLAnchorElement.prototype, "click")
                .mockImplementation(() => undefined);
            let anchor: HTMLAnchorElement | undefined;
            click.mockImplementation(function (this: HTMLAnchorElement) {
                // the element the click was on
                // eslint-disable-next-line @typescript-eslint/no-this-alias
                anchor = this;
            });
            await saveBase64File("QUJD", "application/pdf", "report.pdf");
            expect(g.fetch).toHaveBeenCalledWith(
                "data:application/pdf;base64,QUJD"
            );
            expect(anchor?.download).toBe("report.pdf");
            expect(anchor?.href).toBe("blob:report");
        } else {
            await saveBase64File("QUJD", "application/pdf", "report.pdf");
            expect(written).toEqual({
                name: "report.pdf",
                content: "QUJD",
                encoding: "base64",
            });
            expect(mockViewDocument).toHaveBeenCalledWith(
                expect.objectContaining({
                    uri: expect.stringMatching(
                        /^cache:\/\/hg-exports\/\d+\/report\.pdf$/
                    ),
                    mimeType: "application/pdf",
                })
            );
        }
    });

    it("native: cancelling the save sheet is not an error; other failures are", async () => {
        if (Platform.OS === "web") return;
        mockSaveDocuments.mockRejectedValueOnce(
            Object.assign(new Error("cancelled"), {
                code: "OPERATION_CANCELED",
            })
        );
        await expect(
            saveBase64File("QUJD", "application/pdf", "report.pdf")
        ).resolves.toBeUndefined();
        mockSaveDocuments.mockRejectedValueOnce(new Error("disk full"));
        await expect(
            saveBase64File("QUJD", "application/pdf", "report.pdf")
        ).rejects.toThrow("disk full");
    });
});
