/** Each person's data APIs refresh on their own, so one that fails or is slow holds nothing else back. */
export const datasetKey = (person: string, name: string) => `${person}:${name}`;

/** Each data API's saved records are good for this long (owner, 2026-11-01). */
export const REFRESH_AFTER_MS = 24 * 60 * 60 * 1000;

let refreshedTimes: Record<string, number> = {};
let failedTimes: Record<string, number> = {};
/** What is on disk: a data API being fetched keeps its last good records in the copy. */
let lastCopy: unknown;
/** Cleared by hand: stays cleared until the next launch or a resync by hand. */
let clearedKeys = new Set<string>();
/** Whose copy has been read: saving before that would overwrite it with next to nothing. */
let copyReadFor: string | undefined;
export const getCopyReadFor = () => copyReadFor;
export const setCopyReadFor = (hdid: string | undefined) => {
    copyReadFor = hdid;
};
/** Set when the prompt to read the copy was turned down: the first save asks once more, as it can't overwrite an unread copy. */
let copyReadRetry: (() => void) | undefined;
export const setCopyReadRetry = (retry: (() => void) | undefined) => {
    copyReadRetry = retry;
};
export const retryCopyRead = () => {
    const retry = copyReadRetry;
    copyReadRetry = undefined;
    retry?.();
};
let version = 0;
const listeners = new Set<() => void>();
const changed = () => {
    version += 1;
    listeners.forEach((listener) => listener());
};
/** For the Security page, which shows these times live. */
export const subscribeSavedTimes = (listener: () => void) => {
    listeners.add(listener);
    return () => listeners.delete(listener);
};
export const getSavedTimesVersion = () => version;
export const isDatasetCleared = (key: string) => clearedKeys.has(key);

export const getRefreshedTimes = () => refreshedTimes;
export const getFailedTimes = () => failedTimes;
export const getLastCopy = () => lastCopy;
export const setLastCopy = (copy: unknown) => {
    lastCopy = copy;
};
export const setSavedTimes = (
    refreshed: Record<string, number>,
    failed: Record<string, number>
) => {
    refreshedTimes = refreshed;
    failedTimes = failed;
    changed();
};

const without = (times: Record<string, number>, key: string) =>
    Object.fromEntries(Object.entries(times).filter(([k]) => k !== key));

export const datasetRefreshedAt = (key: string) => refreshedTimes[key];
export const datasetFailedAt = (key: string) => failedTimes[key];
export const markDatasetRefreshed = (key: string, at: number) => {
    refreshedTimes = { ...refreshedTimes, [key]: at };
    failedTimes = without(failedTimes, key);
    clearedKeys.delete(key);
    changed();
};
export const markDatasetFailed = (key: string, at: number) => {
    failedTimes = { ...failedTimes, [key]: at };
    changed();
};
/** A clear by hand: no times, and no background refresh bringing it straight back. */
export const forgetDataset = (key: string) => {
    refreshedTimes = without(refreshedTimes, key);
    failedTimes = without(failedTimes, key);
    clearedKeys.add(key);
    changed();
};
/** Asked for by hand: a cleared one may refresh again. */
export const allowDataset = (key: string) => {
    clearedKeys.delete(key);
};
export const clearDatasetFailures = () => {
    failedTimes = {};
    changed();
};
/** Nothing of whoever was here before carries over. */
export const resetSavedCopyState = () => {
    refreshedTimes = {};
    failedTimes = {};
    lastCopy = undefined;
    clearedKeys = new Set();
    copyReadFor = undefined;
    copyReadRetry = undefined;
    changed();
};
