import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { Dependent } from "@/models/dependent";
import { ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import { IDependentService, ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";

const dependentSort = (a: Dependent, b: Dependent) => {
    const firstDate = new DateWrapper(a.dependentInformation.dateOfBirth);
    const secondDate = new DateWrapper(b.dependentInformation.dateOfBirth);

    if (firstDate.isBefore(secondDate)) {
        return 1;
    }

    if (firstDate.isAfter(secondDate)) {
        return -1;
    }

    return 0;
};

export const useDependentStore = defineStore("dependent", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const dependentService = container.get<IDependentService>(
        SERVICE_IDENTIFIER.DependentService
    );

    const errorStore = useErrorStore();

    const dependents = ref<Dependent[]>([]);
    const status = ref(LoadStatus.NONE);
    const statusMessage = ref("");
    const error = ref<ResultError>();

    const dependentsAreLoading = computed(
        () => status.value === LoadStatus.REQUESTED
    );

    function setDependentsLoading() {
        status.value = LoadStatus.REQUESTED;
    }

    function setDependents(incomingDependents: Dependent[]) {
        dependents.value = incomingDependents;
        error.value = undefined;
        status.value = LoadStatus.LOADED;
    }

    function filterDependent(dependent: Dependent) {
        dependents.value = dependents.value.filter(
            (d) =>
                d.delegateId !== dependent.delegateId ||
                d.ownerId !== dependent.ownerId
        );
        error.value = undefined;
        status.value = LoadStatus.LOADED;
    }

    function setDependentsError(incomingError: ResultError) {
        error.value = incomingError;
        statusMessage.value = incomingError.resultMessage;
        status.value = LoadStatus.ERROR;
    }

    function retrieveDependents(
        hdid: string,
        bypassCache: boolean
    ): Promise<void> {
        if (status.value === LoadStatus.LOADED && !bypassCache) {
            logger.debug("Dependents found stored, not querying!");
            return Promise.resolve();
        }

        logger.debug("Retrieving dependents");
        setDependentsLoading();
        return dependentService
            .getAll(hdid)
            .then((result) => {
                result.sort(dependentSort);
                setDependents(result);
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Retrieve);
                throw error;
            });
    }

    function removeDependent(
        hdid: string,
        dependent: Dependent
    ): Promise<void> {
        logger.debug("Removing dependent");
        setDependentsLoading();
        return dependentService
            .removeDependent(hdid, dependent)
            .then(() => {
                filterDependent(dependent);
            })
            .catch((resultError: ResultError) => {
                handleError(resultError, ErrorType.Delete);
                throw resultError;
            });
    }

    function handleError(incomingError: ResultError, errorType: ErrorType) {
        logger.error(`Error: ${JSON.stringify(incomingError)}`);
        setDependentsError(incomingError);
        if (incomingError.statusCode === 429) {
            const errorKey = "page";
            if (errorType === ErrorType.Retrieve) {
                errorStore.setTooManyRequestsWarning(errorKey);
            } else {
                errorStore.setTooManyRequestsError(errorKey);
            }
        } else {
            errorStore.addError(
                errorType,
                ErrorSourceType.Dependent,
                incomingError.traceId
            );
        }
    }

    return {
        dependents,
        dependentsAreLoading,
        retrieveDependents,
        removeDependent,
    };
});
