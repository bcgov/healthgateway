import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";
import VaccineRecordState from "@/models/vaccineRecordState";
import { ILogger, IVaccinationStatusService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { defineStore } from "pinia";
import { computed, ref } from "vue";

export const useVaccinationStatusStore = defineStore(
    "vaccinationStatusAuthenticated",
    () => {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const vaccinationStatusService =
            container.get<IVaccinationStatusService>(
                SERVICE_IDENTIFIER.VaccinationStatusService
            );

        const errorStore = useErrorStore();

        const defaultVaccineRecordState: VaccineRecordState = {
            hdid: "",
            download: false,
            status: LoadStatus.NONE,
            statusMessage: "",
            resultMessage: "",
        };

        interface AuthenticatedVaccinationStatus {
            vaccinationStatus?: VaccinationStatus;
            error?: ResultError;
            status: LoadStatus;
            statusMessage: string;
        }

        // Refs
        const authenticatedVaccination = ref<AuthenticatedVaccinationStatus>({
            vaccinationStatus: undefined,
            error: undefined,
            status: LoadStatus.NONE,
            statusMessage: "",
        });

        const authenticatedVaccinationRecordStates = ref(
            new Map<string, VaccineRecordState>()
        );

        // Computed
        const authenticatedVaccinationStatus = computed(
            () => authenticatedVaccination.value.vaccinationStatus
        );

        const authenticatedIsLoading = computed(
            () => authenticatedVaccination.value.status === LoadStatus.REQUESTED
        );

        const authenticatedError = computed(
            () => authenticatedVaccination.value.error
        );

        const authenticatedStatusMessage = computed(
            () => authenticatedVaccination.value.statusMessage
        );

        // Mutations
        // Authenticated Vaccination Status
        function setAuthenticatedRequested() {
            authenticatedVaccination.value.error = undefined;
            authenticatedVaccination.value.status = LoadStatus.REQUESTED;
            authenticatedVaccination.value.statusMessage = "";
        }

        function setAuthenticatedVaccinationStatus(
            vaccinationStatus: VaccinationStatus
        ) {
            authenticatedVaccination.value.vaccinationStatus = {
                ...vaccinationStatus,
                issueddate: new DateWrapper().toISO(),
            };
            authenticatedVaccination.value.status = LoadStatus.LOADED;
            authenticatedVaccination.value.statusMessage = "";
        }

        function setAuthenticatedVaccinationStatusError(error: ResultError) {
            authenticatedVaccination.value.vaccinationStatus = undefined;
            authenticatedVaccination.value.error = error;
            authenticatedVaccination.value.status = LoadStatus.ERROR;
            authenticatedVaccination.value.statusMessage = "";
        }

        function setAuthenticatedVaccinationStatusMessage(
            statusMessage: string
        ) {
            authenticatedVaccination.value.statusMessage = statusMessage;
        }

        // Authenticated Vaccination Record
        function setAuthenticatedVaccineRecordRequested(hdid: string) {
            const currentState = getAuthenticatedVaccineRecordState(hdid);
            const nextState: VaccineRecordState = {
                ...currentState,
                record: undefined,
                download: true,
                error: undefined,
                status: LoadStatus.REQUESTED,
                statusMessage: "",
                resultMessage: "",
            };
            setAuthenticatedVaccineRecordState(hdid, nextState);
        }

        function setAuthenticatedVaccinationRecord(
            hdid: string,
            record: CovidVaccineRecord
        ) {
            const currentState = getAuthenticatedVaccineRecordState(hdid);
            const nextState: VaccineRecordState = {
                ...currentState,
                record,
                error: undefined,
                status: LoadStatus.LOADED,
                statusMessage: "",
            };
            setAuthenticatedVaccineRecordState(hdid, nextState);
        }

        function setAuthenticatedVaccinationRecordError(
            hdid: string,
            error: ResultError
        ) {
            const currentState = getAuthenticatedVaccineRecordState(hdid);
            const nextState: VaccineRecordState = {
                ...currentState,
                record: undefined,
                error,
                status: LoadStatus.ERROR,
                statusMessage: "",
            };
            setAuthenticatedVaccineRecordState(hdid, nextState);
        }

        function setAuthenticatedVaccineRecordStatusMessage(
            hdid: string,
            statusMessage: string
        ) {
            const currentState = getAuthenticatedVaccineRecordState(hdid);
            const nextState: VaccineRecordState = {
                ...currentState,
                statusMessage,
            };
            setAuthenticatedVaccineRecordState(hdid, nextState);
        }

        function setAuthenticatedVaccineRecordResultMessage(
            hdid: string,
            resultMessage: string
        ) {
            const currentState = getAuthenticatedVaccineRecordState(hdid);
            const nextState: VaccineRecordState = {
                ...currentState,
                resultMessage,
            };
            setAuthenticatedVaccineRecordState(hdid, nextState);
        }

        function setAuthenticatedVaccineRecordDownload(
            hdid: string,
            download: boolean
        ) {
            const currentState = getAuthenticatedVaccineRecordState(hdid);
            const nextState: VaccineRecordState = {
                ...currentState,
                download,
            };
            setAuthenticatedVaccineRecordState(hdid, nextState);
        }

        // Mutation Helpers
        function getAuthenticatedVaccineRecordState(
            hdid: string
        ): VaccineRecordState {
            return (
                authenticatedVaccinationRecordStates.value.get(hdid) ?? {
                    ...defaultVaccineRecordState,
                    hdid,
                }
            );
        }

        function setAuthenticatedVaccineRecordState(
            hdid: string,
            vaccineRecordState: VaccineRecordState
        ) {
            authenticatedVaccinationRecordStates.value.set(
                hdid,
                vaccineRecordState
            );
        }

        // Action Helpers
        function handleAuthenticatedError(
            error: ResultError,
            errorType: ErrorType
        ) {
            logger.error(`ERROR: ${JSON.stringify(error)}`);
            setAuthenticatedVaccinationStatusError(error);

            if (error.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("page");
            } else {
                errorStore.addError(
                    errorType,
                    ErrorSourceType.VaccineCard,
                    error.traceId
                );
            }
        }

        function handleAuthenticatedPdfError(
            hdid: string,
            error: ResultError,
            errorType: ErrorType
        ) {
            logger.error(`ERROR: ${JSON.stringify(error)}`);
            setAuthenticatedVaccinationRecordError(hdid, error);

            if (error.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("vaccineCardComponent");
            } else {
                if (error.actionCode === ActionType.Invalid) {
                    setAuthenticatedVaccineRecordResultMessage(
                        hdid,
                        "No records found"
                    );
                } else {
                    errorStore.addError(
                        errorType,
                        ErrorSourceType.VaccineCard,
                        error.traceId
                    );
                }
            }
        }

        // Actions
        function retrieveAuthenticatedVaccineStatus(
            hdid: string
        ): Promise<void> {
            if (authenticatedVaccination.value.status === LoadStatus.LOADED) {
                logger.debug(
                    `Authenticated vaccination status found stored, not querying!`
                );
                return Promise.resolve();
            } else {
                logger.debug(`Retrieving authenticated vaccination status`);
                setAuthenticatedRequested();
                return vaccinationStatusService
                    .getAuthenticatedVaccineStatus(hdid)
                    .then((result) => {
                        const payload = result.resourcePayload;
                        if (result.resultStatus === ResultType.Success) {
                            setAuthenticatedVaccinationStatus(payload);
                        } else if (
                            result.resultError?.actionCode ===
                                ActionType.Refresh &&
                            !payload.loaded &&
                            payload.retryin > 0
                        ) {
                            logger.info(
                                "Authenticated vaccination status not loaded"
                            );
                            setAuthenticatedVaccinationStatusMessage(
                                "Please wait a moment while we retrieve your proof of vaccination."
                            );
                            setTimeout(() => {
                                logger.info(
                                    "Re-querying for authenticated proof of vaccination"
                                );
                                retrieveAuthenticatedVaccineStatus(hdid);
                            }, payload.retryin);
                        } else {
                            if (result.resultError) {
                                throw result.resultError;
                            }
                            logger.warn(
                                `Authenticated vaccination status retrieval failed! ${JSON.stringify(
                                    result
                                )}`
                            );
                        }
                    })
                    .catch((error: ResultError) => {
                        handleAuthenticatedError(error, ErrorType.Retrieve);
                        throw error;
                    });
            }
        }

        function retrieveAuthenticatedVaccineRecord(
            hdid: string
        ): Promise<void> {
            const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
            const vaccinationStatusService =
                container.get<IVaccinationStatusService>(
                    SERVICE_IDENTIFIER.VaccinationStatusService
                );

            logger.debug(`Retrieving authenticated vaccination record`);
            setAuthenticatedVaccineRecordRequested(hdid);

            return vaccinationStatusService
                .getAuthenticatedVaccineRecord(hdid)
                .then((result) => {
                    const payload = result.resourcePayload;
                    if (result.resultStatus === ResultType.Success) {
                        logger.info("Authenticated vaccination record loaded");
                        setAuthenticatedVaccinationRecord(hdid, payload);
                    } else if (
                        result.resultError?.actionCode === ActionType.Refresh &&
                        !payload.loaded &&
                        payload.retryin > 0
                    ) {
                        logger.info(
                            "Authenticated vaccination record not loaded but will try again"
                        );
                        setAuthenticatedVaccineRecordStatusMessage(
                            hdid,
                            "Please wait a moment while we download your proof of vaccination."
                        );
                        setTimeout(() => {
                            logger.info(
                                "Re-querying for downloading the authenticated vaccination record"
                            );
                            retrieveAuthenticatedVaccineRecord(hdid);
                        }, payload.retryin);
                    } else {
                        if (result.resultError) {
                            throw result.resultError;
                        }
                        logger.warn(
                            `Authenticated vaccination record retrieval failed! ${JSON.stringify(
                                result
                            )}`
                        );
                    }
                })
                .catch((error: ResultError) => {
                    logger.error(
                        "Authenticated vaccination record not loaded due to unexpected error"
                    );
                    handleAuthenticatedPdfError(
                        hdid,
                        error,
                        ErrorType.Retrieve
                    );
                });
        }

        function authenticatedVaccineRecordState(
            hdid: string
        ): VaccineRecordState {
            return getAuthenticatedVaccineRecordState(hdid);
        }

        function stopAuthenticatedVaccineRecordDownload(hdid: string) {
            setAuthenticatedVaccineRecordDownload(hdid, false);
        }

        return {
            authenticatedVaccinationStatus,
            authenticatedIsLoading,
            authenticatedError,
            authenticatedStatusMessage,
            retrieveAuthenticatedVaccineStatus,
            retrieveAuthenticatedVaccineRecord,
            authenticatedVaccineRecordState,
            stopAuthenticatedVaccineRecordDownload,
        };
    }
);
