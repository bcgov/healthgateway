import * as SecureStore from "expo-secure-store";

import { describeError, logger } from "@/services/logger";
import { vault } from "@/services/vault";

import {
    inOrder,
    SEALED_STORAGE_KEY,
    STORAGE_KEY,
} from "./storedSession.native";

export interface StoredSession {
    accessToken: string;
    refreshToken?: string;
    idToken: string;
    /** Unix seconds. */
    expiresAt: number;
    /**
     * Unix seconds, when the tokens were issued: which of two stored copies is newer (`isNewerThan`). Not `expiresAt`,
     * which Keycloak can bring forward to the end of its session. Missing from a login stored by an earlier build.
     */
    issuedAt?: number;
    sessionState?: string;
}

/**
 * What is stored under `SEALED_STORAGE_KEY`: the session encrypted by the vault, and when its tokens were issued, in
 * the clear, so it can be compared with an unsealed copy without the prompt.
 */
interface SealedCopy {
    issuedAt?: number;
    data: string;
}

/** Each device logs in for itself: never restored onto another from a backup. */
const STORE_OPTIONS: SecureStore.SecureStoreOptions = {
    keychainAccessible: SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY,
};

/** The stored text as an object, if it is one. */
function parseObject<T>(text: string): Partial<T> | undefined {
    try {
        return (JSON.parse(text) as Partial<T> | null) ?? undefined;
    } catch {
        return undefined;
    }
}

/** What was stored or unsealed, if it is a session: a damaged copy must not go into memory as one. */
function parseSession(text: string): StoredSession | undefined {
    const value = parseObject<StoredSession>(text);
    if (
        !value ||
        typeof value.accessToken !== "string" ||
        !value.accessToken ||
        typeof value.idToken !== "string" ||
        !value.idToken ||
        typeof value.expiresAt !== "number" ||
        (value.issuedAt !== undefined && typeof value.issuedAt !== "number") ||
        (value.refreshToken !== undefined &&
            typeof value.refreshToken !== "string")
    )
        return undefined;
    return value as StoredSession;
}

/** The stored sealed copy, if it is one: a damaged one has nothing to decrypt. */
function parseSealedCopy(text: string): SealedCopy | undefined {
    const value = parseObject<SealedCopy>(text);
    if (
        !value ||
        typeof value.data !== "string" ||
        (value.issuedAt !== undefined && typeof value.issuedAt !== "number")
    )
        return undefined;
    return value as SealedCopy;
}

/**
 * The unsealed copy left beside a sealed one holds newer tokens: sealing them failed, and so did removing the sealed
 * copy (`write`). Otherwise it is left over from a move, or from a save the sealed copy has since replaced. Without
 * a time to go by, or with a damaged sealed copy, it is not taken for newer.
 */
function isNewerThan(unsealed: StoredSession, sealed?: SealedCopy): boolean {
    if (unsealed.issuedAt === undefined || !sealed) return false;
    // only a login stored by an earlier build has no time, and every save of new tokens since has one
    return sealed.issuedAt === undefined || unsealed.issuedAt > sealed.issuedAt;
}

/** What a launch finds stored. */
export interface StoredAtLaunch {
    /** A sealed copy is the login: it opens after the lock's prompt (`unseal`). */
    sealed: boolean;
    /** Otherwise, the unsealed copy, if there is one. */
    session?: StoredSession;
}

/**
 * The session's two stored copies (SecureStore): unsealed under `STORAGE_KEY`, and sealed by the vault under
 * `SEALED_STORAGE_KEY` when device security is on. There should only ever be one. This keeps track of which, and of an
 * other copy that could not be removed, so `NativeAuthService` only has to know whether the session is in memory.
 */
export class StoredCopies {
    /**
     * The session in memory is stored sealed. Undefined until one has been stored or unsealed here: a copy of the other
     * kind may be left from an earlier login, and the first save removes it (`write`).
     */
    private storedSealed?: boolean;
    /** An unsealed copy could not be removed from beside the sealed login: the next save, or unsealing, tries again. */
    private unsealedCopyLeft = false;

    /** The session in memory is stored sealed, so it may leave memory; one that could only be stored unsealed may not. */
    get isSealed(): boolean {
        return this.storedSealed === true;
    }

    /** The session has gone: nothing is known of what is stored for the next one. */
    forget(): void {
        this.storedSealed = undefined;
        this.unsealedCopyLeft = false;
    }

    /**
     * `readSealed`: device security is on. Off, a sealed copy is not read: the next login replaces it.
     *
     * A copy left unsealed beside a sealed login is usually older, and goes. One with newer tokens is the login
     * (`isNewerThan`), and is returned for the caller to seal in place of the old one, or the refresh token it holds
     * would be lost.
     *
     * Rejects when the sealed copy could not be read: taken as "none", a sealed login would look signed out, and the
     * next login would replace it. Start-up stops instead, and can be tried again. `ended`: the session went
     * meanwhile, so nothing more is read or removed.
     */
    async readAtLaunch(
        readSealed: boolean,
        ended: () => boolean
    ): Promise<StoredAtLaunch> {
        const sealed = readSealed ? await this.readSealedCopy() : null;
        if (ended()) return { sealed: false };
        const unsealed = await this.readUnsealedCopy();
        if (ended()) return { sealed: false };
        if (
            sealed &&
            !(unsealed && isNewerThan(unsealed, parseSealedCopy(sealed)))
        ) {
            await this.removeUnsealedCopy();
            return { sealed: true };
        }
        return { sealed: false, session: unsealed };
    }

    private async readSealedCopy(): Promise<string | null> {
        try {
            return await inOrder(() =>
                SecureStore.getItemAsync(SEALED_STORAGE_KEY)
            );
        } catch (error) {
            logger.warn(
                `Unable to read the sealed session: ${describeError(error)}`
            );
            throw error;
        }
    }

    /**
     * The stored unsealed session, if it holds one. One that could not be read is taken as none: signed out, or older
     * than a sealed copy beside it.
     */
    private async readUnsealedCopy(): Promise<StoredSession | undefined> {
        try {
            const raw = await inOrder(() =>
                SecureStore.getItemAsync(STORAGE_KEY)
            );
            return raw ? parseSession(raw) : undefined;
        } catch (error) {
            logger.warn(
                `Unable to read stored session: ${describeError(error)}`
            );
            return undefined;
        }
    }

    /**
     * The unsealed copy left beside a sealed login goes. One that could not be removed is remembered, so unsealing and
     * every save after it try again: otherwise the tokens would stay outside the vault for as long as the login lasts.
     */
    private async removeUnsealedCopy(): Promise<void> {
        try {
            await inOrder(() => SecureStore.deleteItemAsync(STORAGE_KEY));
            this.unsealedCopyLeft = false;
        } catch (error) {
            this.unsealedCopyLeft = true;
            logger.warn(
                `Unable to delete the unsealed session: ${describeError(error)}`
            );
        }
    }

    /**
     * The sealed copy, decrypted: the OS asks first unless the vault's prompt has just passed. Undefined when it is
     * missing or damaged. Rejects when the store or the vault could not be used (`isVaultKeyGone` and the like say why).
     */
    async unseal(): Promise<StoredSession | undefined> {
        const stored = await inOrder(() =>
            SecureStore.getItemAsync(SEALED_STORAGE_KEY)
        );
        const copy = stored ? parseSealedCopy(stored) : undefined;
        return copy ? parseSession(await vault.decrypt(copy.data)) : undefined;
    }

    /** What `unseal` returned is now the session in memory: an unsealed copy still beside it is tried again. */
    async unsealed(): Promise<void> {
        this.storedSealed = true;
        if (this.unsealedCopyLeft) await this.removeUnsealedCopy();
    }

    /**
     * Call in order (`inOrder`). The other copy goes once this one is saved, so a failure between never leaves neither.
     * It must not leave both for long either: an unsealed copy beside a sealed one is a login on disk without the
     * protection the setting promises, and a sealed one left beside a newer unsealed one is older than the login (the next
     * start tells them apart by `issuedAt`: `readAtLaunch`). So an other copy that could not be removed is remembered,
     * and removed at the next save.
     *
     * `move`: Device Security is being turned on or off, and the caller must know whether it worked. The session is the
     * same in both copies, so a move that can't finish is undone and rejects. A save of new tokens is never undone,
     * which would put the old ones back on disk: it stands, and only the clearing up is left for later.
     */
    async write(session: StoredSession, sealed: boolean, move = false) {
        const text = JSON.stringify(session);
        if (sealed) {
            const copy: SealedCopy = {
                issuedAt: session.issuedAt,
                data: await vault.encrypt(text),
            };
            await SecureStore.setItemAsync(
                SEALED_STORAGE_KEY,
                JSON.stringify(copy),
                STORE_OPTIONS
            );
        } else {
            await SecureStore.setItemAsync(STORAGE_KEY, text, STORE_OPTIONS);
        }
        if (this.storedSealed === sealed && !(sealed && this.unsealedCopyLeft))
            return;
        const [saved, other] = sealed
            ? [SEALED_STORAGE_KEY, STORAGE_KEY]
            : [STORAGE_KEY, SEALED_STORAGE_KEY];
        try {
            await SecureStore.deleteItemAsync(other);
        } catch (error) {
            if (move && this.storedSealed === !sealed) {
                let undone = false;
                try {
                    await SecureStore.deleteItemAsync(saved);
                    undone = true;
                } catch (undoError) {
                    logger.warn(
                        `Unable to undo the stored session's move: ${describeError(undoError)}`
                    );
                }
                // as it was before the move
                if (undone) throw error;
            }
            // both are there. Sealed, the new copy is the login and the unsealed one is only left over. Unsealed, it
            // is no longer known that this is the only copy, so every save tries again, and the tokens stay in memory
            // under the lock (`isSealed`)
            if (sealed) {
                this.storedSealed = true;
                this.unsealedCopyLeft = true;
            } else {
                this.storedSealed = undefined;
            }
            if (move) throw error;
            logger.warn(
                `Unable to delete the stored session's other copy: ${describeError(error)}`
            );
            return;
        }
        this.storedSealed = sealed;
        if (sealed) this.unsealedCopyLeft = false;
    }
}
