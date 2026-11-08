import { type DatasetState, LoadStatus } from "@/models/datasetState";
import type { ResultError } from "@/models/errors";

/**
 * Port of stores/utils/DatasetMapUtils.ts as pure functions over a per-hdid record, so Zustand stores can
 * `set((s) => ({ states: setStateData(s.states, hdid, …) }))`.
 */
export type DatasetMap<S> = Record<string, S>;

export function getDatasetState<S>(
    map: DatasetMap<S>,
    key: string,
    defaultState: S
): S {
    return map[key] ?? { ...defaultState };
}

export function setStateRequested<S extends DatasetState<unknown>>(
    map: DatasetMap<S>,
    key: string,
    defaultState: S
): DatasetMap<S> {
    return {
        ...map,
        [key]: {
            ...getDatasetState(map, key, defaultState),
            status: LoadStatus.REQUESTED,
        },
    };
}

export function setStateError<S extends DatasetState<unknown>>(
    map: DatasetMap<S>,
    key: string,
    defaultState: S,
    error?: ResultError,
    overrideStatusMessage?: string
): DatasetMap<S> {
    return {
        ...map,
        [key]: {
            ...getDatasetState(map, key, defaultState),
            error,
            statusMessage:
                overrideStatusMessage ?? error?.message ?? "Error occurred",
            status: LoadStatus.ERROR,
        },
    };
}

export function setStateData<S extends DatasetState<unknown>>(
    map: DatasetMap<S>,
    key: string,
    defaultState: S,
    data: S["data"],
    extendedProperties?: Partial<S>
): DatasetMap<S> {
    return {
        ...map,
        [key]: {
            ...getDatasetState(map, key, defaultState),
            data,
            error: undefined,
            statusMessage: "success",
            status: LoadStatus.LOADED,
            ...extendedProperties,
        },
    };
}
