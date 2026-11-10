import { describeError, logger } from "@/services/logger";
import { isVaultCancelled, vault } from "@/services/vault";

const FILE_PREFIX = "hg-record-cache";
/** 3: a new key per save, and a readable first line. Older copies read as none. */
const VERSION = 3;

/** The user turned down the prompt: there may be a copy, unread, which a save must not overwrite. */
export const COPY_DECLINED = "declined";

/**
 * Readable without the prompt, so tidying another person's old copy doesn't ask for it. Says when it expires and nothing
 * else; whose it is and what it holds are encrypted.
 */
interface CopyHeader {
    version: number;
    expiresAt: number;
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

const cacheFile = async (hdid: string) => {
    const { File, Paths } = await import("expo-file-system");
    return new File(Paths.document, await fileName(hdid));
};

/**
 * The offline copy of each person's records: one file each, encrypted by the vault, so nothing can read it without biometrics
 * or the device PIN / passcode. Saving doesn't ask for them; reading does. Only people with device security have one.
 */
export const recordCache = {
    async save(
        hdid: string,
        data: unknown,
        now: number,
        expiresAt: number
    ): Promise<boolean> {
        const removalsAtStart = removals;
        try {
            const encrypted = await vault.encrypt(
                JSON.stringify({ version: VERSION, savedAt: now, hdid, data })
            );
            const file = await cacheFile(hdid);
            if (removalsAtStart !== removals) return false;
            if (!file.exists) file.create();
            const header: CopyHeader = { version: VERSION, expiresAt };
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
     * Someone logged in: anyone else's copy goes once it has expired. One from another version can never open again, so it
     * goes too. Reads only each file's first line, so it never asks.
     */
    async removeExpired(keepHdid: string, now: number): Promise<void> {
        try {
            const { Directory, Paths, File } = await import("expo-file-system");
            const keep = await fileName(keepHdid);
            for (const entry of new Directory(Paths.document).list()) {
                if (
                    !entry.name.startsWith(FILE_PREFIX) ||
                    entry.name === keep ||
                    !(entry instanceof File)
                )
                    continue;
                const copy = splitCopy(await entry.text());
                if (!copy || copy.header.expiresAt <= now) entry.delete();
            }
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
            const { Directory, Paths } = await import("expo-file-system");
            for (const entry of new Directory(Paths.document).list())
                if (entry.name.startsWith(FILE_PREFIX)) entry.delete();
        } catch (error) {
            logger.warn(
                `Could not delete the record cache: ${describeError(error)}`
            );
        }
    },
};
