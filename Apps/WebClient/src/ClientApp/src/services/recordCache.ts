import type { Directory } from "expo-file-system";

import { describeError, logger } from "@/services/logger";
import { isVaultCancelled, vault } from "@/services/vault";

const FILE_PREFIX = "hg-record-cache";
/** The copies' own folder in Documents, so it can be kept out of iOS backups as a whole. */
const FOLDER = "hg-record-cache";
/** 3: a new key per save, and a readable first line. Older copies read as none. */
const VERSION = 3;

/** The user turned down the prompt: there may be a copy, unread, which a save must not overwrite. */
export const COPY_DECLINED = "declined";

/**
 * Readable without the prompt, so tidying a copy from another version doesn't ask for it. Says nothing else: whose it is
 * and what it holds are encrypted. (Copies saved before 2026-11-16 also carry an `expiresAt`, which is no longer read.)
 */
interface CopyHeader {
    version: number;
}

const splitCopy = (text: string) => {
    const end = text.indexOf("\n");
    if (end < 0) return undefined;
    try {
        const header = JSON.parse(text.slice(0, end)) as CopyHeader;
        return header.version === VERSION
            ? { header, sealed: text.slice(end + 1) }
            : undefined;
    } catch {
        return undefined;
    }
};

export interface RecordSnapshot<D = Record<string, unknown>> {
    version: number;
    savedAt: number;
    hdid: string;
    data: D;
}

/** Raised by removing the copy: a save already under way must not bring it back. */
let removals = 0;

/** One file per person. The name is a hash, so the file system doesn't show whose it is. */
const fileName = async (hdid: string) => {
    const Crypto = await import("expo-crypto");
    const name = await Crypto.digestStringAsync(
        Crypto.CryptoDigestAlgorithm.SHA256,
        hdid
    );
    return `${FILE_PREFIX}-${name}.enc`;
};

let folderReady: Promise<Directory> | undefined;

/**
 * The folder the copies live in: made on first use and kept out of iCloud and computer backups, as a copy can only be read
 * on this device. Copies saved before the folder existed, loose in Documents, move into it. (Android's backup rules
 * already leave app files out.) Once per launch; a failure tries again on the next use.
 */
function cacheFolder(): Promise<Directory> {
    folderReady ??= (async () => {
        const fs = await import("expo-file-system");
        const folder = new fs.Directory(fs.Paths.document, FOLDER);
        folder.create({ intermediates: true, idempotent: true });
        try {
            await vault.excludeFromBackup(folder.uri);
        } catch (error) {
            logger.warn(
                `Could not keep the record cache out of backups: ${describeError(error)}`
            );
        }
        for (const entry of new fs.Directory(fs.Paths.document).list()) {
            if (
                !(entry instanceof fs.File) ||
                !entry.name.startsWith(FILE_PREFIX)
            )
                continue;
            try {
                if (new fs.File(folder, entry.name).exists) entry.delete();
                else await entry.move(folder);
            } catch (error) {
                logger.warn(
                    `Could not move a record cache file: ${describeError(error)}`
                );
            }
        }
        return folder;
    })().catch((error: unknown) => {
        folderReady = undefined;
        throw error;
    });
    return folderReady;
}

const cacheFile = async (hdid: string) => {
    const { File } = await import("expo-file-system");
    return new File(await cacheFolder(), await fileName(hdid));
};

/** Every copy in the folder, whoever's. */
const cacheFiles = async () => {
    const { File } = await import("expo-file-system");
    return (await cacheFolder())
        .list()
        .filter(
            (entry): entry is InstanceType<typeof File> =>
                entry instanceof File && entry.name.startsWith(FILE_PREFIX)
        );
};

/**
 * The offline copy of each person's records: one file each, encrypted by the vault, so nothing can read it without biometrics
 * or the device PIN / passcode. Saving doesn't ask for them; reading does. Only people with device security have one. Each
 * stays until that person clears it, Reset App, or turning device security off: logging out, or someone else logging
 * in, leaves it for that person's next login.
 */
export const recordCache = {
    async save(hdid: string, data: unknown, now: number): Promise<boolean> {
        const removalsAtStart = removals;
        try {
            const encrypted = await vault.encrypt(
                JSON.stringify({ version: VERSION, savedAt: now, hdid, data })
            );
            const file = await cacheFile(hdid);
            if (removalsAtStart !== removals) return false;
            if (!file.exists) file.create();
            const header: CopyHeader = { version: VERSION };
            await file.write(`${JSON.stringify(header)}\n${encrypted}`);
            if (removalsAtStart !== removals) {
                if (file.exists) file.delete();
                return false;
            }
            return true;
        } catch (error) {
            logger.warn(
                `Could not save the record cache: ${describeError(error)}`
            );
            return false;
        }
    },

    /** The copy for this person, if there is a readable one: the OS may ask first. Anything else counts as none. */
    async load<D>(
        hdid: string
    ): Promise<RecordSnapshot<D> | typeof COPY_DECLINED | undefined> {
        try {
            const file = await cacheFile(hdid);
            if (!file.exists) return undefined;
            const copy = splitCopy(await file.text());
            if (!copy) return undefined;
            const snapshot = JSON.parse(
                await vault.decrypt(copy.sealed)
            ) as RecordSnapshot<D>;
            if (snapshot.version !== VERSION || snapshot.hdid !== hdid)
                return undefined;
            return snapshot;
        } catch (error) {
            if (isVaultCancelled(error)) return COPY_DECLINED;
            logger.warn(
                `Could not read the record cache: ${describeError(error)}`
            );
            return undefined;
        }
    },

    /**
     * Someone logged in: a copy from another version can never open again, so it goes. Everyone's current copies stay.
     * Reads only each file's first line, so it never asks.
     */
    async removeOutdated(): Promise<void> {
        try {
            for (const file of await cacheFiles())
                if (!splitCopy(await file.text())) file.delete();
        } catch (error) {
            logger.warn(
                `Could not tidy the record cache: ${describeError(error)}`
            );
        }
    },

    /** One person's copy: their own Clear All. */
    async removeFor(hdid: string): Promise<void> {
        removals += 1;
        try {
            const file = await cacheFile(hdid);
            if (file.exists) file.delete();
        } catch (error) {
            logger.warn(
                `Could not delete the record cache: ${describeError(error)}`
            );
        }
    },

    /** Every copy on this device, whoever's: Reset App, or turning device security off. */
    async remove(): Promise<void> {
        removals += 1;
        try {
            for (const file of await cacheFiles()) file.delete();
        } catch (error) {
            logger.warn(
                `Could not delete the record cache: ${describeError(error)}`
            );
        }
    },
};
