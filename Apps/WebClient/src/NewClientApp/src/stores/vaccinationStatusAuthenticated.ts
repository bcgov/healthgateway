import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { VaccinationState } from "@/constants/vaccinationState";
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

export const useVaccinationStatusAuthenticatedStore = defineStore(
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

        interface Vaccination {
            data?: VaccinationStatus;
            error?: ResultError;
            status: LoadStatus;
            statusMessage: string;
        }

        // Refs
        const vaccination = ref<Vaccination>({
            data: undefined,
            error: undefined,
            status: LoadStatus.NONE,
            statusMessage: "",
        });

        const vaccineRecordStates = ref(new Map<string, VaccineRecordState>());

        // Computed
        // Vaccination Status
        const vaccinationStatus = computed(() => vaccination.value.data);

        const vaccinationStatusIsLoading = computed(
            () => vaccination.value.status === LoadStatus.REQUESTED
        );

        const vaccinationStatusError = computed(() => vaccination.value.error);

        const vaccinationStatusStatusMessage = computed(
            () => vaccination.value.statusMessage
        );

        const isPartiallyVaccinated = computed(
            () =>
                vaccination.value.data?.state ===
                VaccinationState.PartiallyVaccinated
        );

        const isVaccinationNotFound = computed(
            () => vaccination.value.data?.state === VaccinationState.NotFound
        );

        // Getters
        // Vaccine Records
        function vaccineRecordState(hdid: string): VaccineRecordState {
            return getVaccineRecordState(hdid);
        }

        // Mutations
        // Vaccination Status
        function setVaccinationStatusRequested() {
            vaccination.value.error = undefined;
            vaccination.value.status = LoadStatus.REQUESTED;
            vaccination.value.statusMessage = "";
        }

        function setVaccinationStatus(vaccinationStatus: VaccinationStatus) {
            vaccination.value.data = {
                ...vaccinationStatus,
                issueddate: new DateWrapper().toISO(),
            };
            vaccination.value.status = LoadStatus.LOADED;
            vaccination.value.statusMessage = "";
        }

        function setVaccinationStatusError(error: ResultError) {
            vaccination.value.data = undefined;
            vaccination.value.error = error;
            vaccination.value.status = LoadStatus.ERROR;
            vaccination.value.statusMessage = "";
        }

        function setVaccinationStatusStatusMessage(statusMessage: string) {
            vaccination.value.statusMessage = statusMessage;
        }

        // Vaccination Record
        function setVaccineRecordRequested(hdid: string) {
            const currentState = getVaccineRecordState(hdid);
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

        function setVaccineRecord(hdid: string, record: CovidVaccineRecord) {
            const currentState = getVaccineRecordState(hdid);
            const nextState: VaccineRecordState = {
                ...currentState,
                record,
                error: undefined,
                status: LoadStatus.LOADED,
                statusMessage: "",
            };
            setAuthenticatedVaccineRecordState(hdid, nextState);
        }

        function setVaccineRecordError(hdid: string, error: ResultError) {
            const currentState = getVaccineRecordState(hdid);
            const nextState: VaccineRecordState = {
                ...currentState,
                record: undefined,
                error,
                status: LoadStatus.ERROR,
                statusMessage: "",
            };
            setAuthenticatedVaccineRecordState(hdid, nextState);
        }

        function setVaccineRecordStatusMessage(
            hdid: string,
            statusMessage: string
        ) {
            const currentState = getVaccineRecordState(hdid);
            const nextState: VaccineRecordState = {
                ...currentState,
                statusMessage,
            };
            setAuthenticatedVaccineRecordState(hdid, nextState);
        }

        function setVaccineRecordResultMessage(
            hdid: string,
            resultMessage: string
        ) {
            const currentState = getVaccineRecordState(hdid);
            const nextState: VaccineRecordState = {
                ...currentState,
                resultMessage,
            };
            setAuthenticatedVaccineRecordState(hdid, nextState);
        }

        function setVaccineRecordDownload(hdid: string, download: boolean) {
            const currentState = getVaccineRecordState(hdid);
            const nextState: VaccineRecordState = {
                ...currentState,
                download,
            };
            setAuthenticatedVaccineRecordState(hdid, nextState);
        }

        // Mutation Helpers
        function getVaccineRecordState(hdid: string): VaccineRecordState {
            return (
                vaccineRecordStates.value.get(hdid) ?? {
                    ...defaultVaccineRecordState,
                    hdid,
                }
            );
        }

        function setAuthenticatedVaccineRecordState(
            hdid: string,
            vaccineRecordState: VaccineRecordState
        ) {
            vaccineRecordStates.value.set(hdid, vaccineRecordState);
        }

        // Action Helpers
        function handleAuthenticatedError(
            error: ResultError,
            errorType: ErrorType
        ) {
            logger.error(`ERROR: ${JSON.stringify(error)}`);
            setVaccinationStatusError(error);

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
            setVaccineRecordError(hdid, error);

            if (error.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("vaccineCardComponent");
            } else {
                if (error.actionCode === ActionType.Invalid) {
                    setVaccineRecordResultMessage(hdid, "No records found");
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
        function retrieveVaccinationStatus(hdid: string): Promise<void> {
            if (vaccination.value.status === LoadStatus.LOADED) {
                logger.debug(
                    `Authenticated vaccination status found stored, not querying!`
                );
                return Promise.resolve();
            } else {
                logger.debug(`Retrieving authenticated vaccination status`);
                setVaccinationStatusRequested();
                return vaccinationStatusService
                    .getAuthenticatedVaccineStatus(hdid)
                    .then((result) => {
                        const payload = result.resourcePayload;
                        if (result.resultStatus === ResultType.Success) {
                            setVaccinationStatus(payload);
                        } else if (
                            result.resultError?.actionCode ===
                                ActionType.Refresh &&
                            !payload.loaded &&
                            payload.retryin > 0
                        ) {
                            logger.info(
                                "Authenticated vaccination status not loaded"
                            );
                            setVaccinationStatusStatusMessage(
                                "Please wait a moment while we retrieve your proof of vaccination."
                            );
                            setTimeout(() => {
                                logger.info(
                                    "Re-querying for authenticated proof of vaccination"
                                );
                                retrieveVaccinationStatus(hdid);
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

        function retrieveVaccineRecord(hdid: string): Promise<void> {
            logger.debug(`Retrieving authenticated vaccination record`);
            setVaccineRecordRequested(hdid);

            return vaccinationStatusService
                .getAuthenticatedVaccineRecord(hdid)
                .then((result) => {
                    const payload = result.resourcePayload;
                    if (result.resultStatus === ResultType.Success) {
                        logger.info("Authenticated vaccination record loaded");
                        setVaccineRecord(hdid, payload);
                    } else if (
                        result.resultError?.actionCode === ActionType.Refresh &&
                        !payload.loaded &&
                        payload.retryin > 0
                    ) {
                        logger.info(
                            "Authenticated vaccination record not loaded but will try again"
                        );
                        setVaccineRecordStatusMessage(
                            hdid,
                            "Please wait a moment while we download your proof of vaccination."
                        );
                        setTimeout(() => {
                            logger.info(
                                "Re-querying for downloading the authenticated vaccination record"
                            );
                            retrieveVaccineRecord(hdid);
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

        function stopVaccineRecordDownload(hdid: string) {
            setVaccineRecordDownload(hdid, false);
        }

        return {
            vaccinationStatus,
            vaccinationStatusIsLoading,
            vaccinationStatusError,
            vaccinationStatusStatusMessage,
            isPartiallyVaccinated,
            isVaccinationNotFound,
            vaccineRecordState,
            retrieveVaccinationStatus,
            retrieveVaccineRecord,
            stopVaccineRecordDownload,
        };
    }
);
