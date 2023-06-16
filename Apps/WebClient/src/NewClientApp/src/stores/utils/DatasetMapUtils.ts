import { LoadStatus } from "@/models/storeOperations";
import { DatasetState } from "@/models/datasetState";
import { ResultError } from "@/models/errors";

export class DatasetMapUtils<T, S extends DatasetState<T>> {
    private readonly defaultState: S;

    /**
     * Creates a new DatasetMapUtils instance. setting the default state for the dataset.
     * @param defaultState should be of the desired dataset's data type.
     */
    constructor(defaultState: S) {
        this.defaultState = defaultState;
    }

    /**
     * Returns a readonly dataset state from the map, or the default state if not found.
     * @param stateMap
     * @param key
     */
    public getDatasetState(stateMap: Map<string, S>, key: string): S {
        return stateMap.get(key) ?? { ...this.defaultState };
    }

    /**
     * Update the dataset state in the map. To indicate that the dataset is being loaded.
     * @param stateMap
     * @param key
     */
    public setStateRequested(stateMap: Map<string, S>, key: string): void {
        const state = this.getDatasetState(stateMap, key);
        stateMap.set(key, {
            ...state,
            status: LoadStatus.REQUESTED,
        });
    }

    /**
     * Update the dataset state in the map. To indicate that the dataset state is in error.
     * @param stateMap
     * @param key
     * @param error
     */
    public setStateError(
        stateMap: Map<string, S>,
        key: string,
        error: ResultError
    ): void {
        const state = this.getDatasetState(stateMap, key);
        stateMap.set(key, {
            ...state,
            error: error,
            statusMessage: error.resultMessage,
            status: LoadStatus.ERROR,
        });
    }

    public setStateData(
        stateMap: Map<string, S>,
        key: string,
        data: T,
        extendedProperties?: Record<string, unknown>
    ): void {
        const state = this.getDatasetState(stateMap, key);
        stateMap.set(key, {
            ...state,
            data: data,
            error: undefined,
            statusMessage: "success",
            status: LoadStatus.LOADED,
            ...extendedProperties,
        });
    }
}
