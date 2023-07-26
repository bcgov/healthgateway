import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { ActionType } from "@/constants/actionType";
import { ResultType } from "@/constants/resulttype";
import { VaccinationState } from "@/constants/vaccinationState";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import { CustomBannerError, ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";
import { ILogger, IVaccinationStatusService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";

export const useVaccinationStatusPublicStore = defineStore(
    "vaccinationStatusPublic",
    () => {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const vaccinationStatusService =
            container.get<IVaccinationStatusService>(
                SERVICE_IDENTIFIER.VaccinationStatusService
            );

        const errorStore = useErrorStore();

        interface Vaccination {
            data?: VaccinationStatus;
            error?: CustomBannerError;
            status: LoadStatus;
            statusMessage: string;
        }

        interface VaccinationRecord {
            data?: CovidVaccineRecord;
            error?: CustomBannerError;
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

        const vaccinationRecord = ref<VaccinationRecord>({
            data: undefined,
            error: undefined,
            status: LoadStatus.NONE,
            statusMessage: "",
        });

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

        const isFullyVaccinated = computed(
            () =>
                vaccination.value.data?.state ===
                VaccinationState.FullyVaccinated
        );

        const isPartiallyVaccinated = computed(
            () =>
                vaccination.value.data?.state ===
                VaccinationState.PartiallyVaccinated
        );

        const isVaccinationNotFound = computed(
            () => vaccination.value.data?.state === VaccinationState.NotFound
        );

        // Vaccine Record
        const vaccineRecord = computed(() => vaccinationRecord.value.data);

        const vaccineRecordIsLoading = computed(
            () => vaccinationRecord.value.status === LoadStatus.REQUESTED
        );

        const vaccineRecordError = computed(
            () => vaccinationRecord.value.error
        );

        const vaccineRecordStatusMessage = computed(
            () => vaccinationRecord.value.statusMessage
        );

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

        function setVaccinationStatusError(
            error: CustomBannerError | undefined
        ) {
            vaccination.value.data = undefined;
            vaccination.value.error = error;
            vaccination.value.status = LoadStatus.ERROR;
            vaccination.value.statusMessage = "";
        }

        function setVaccinationStatusStatusMessage(statusMessage: string) {
            vaccination.value.statusMessage = statusMessage;
        }

        // Vaccination Record
        function setVaccinationRecordRequested() {
            vaccinationRecord.value.status = LoadStatus.REQUESTED;
            vaccinationRecord.value.statusMessage = "";
            vaccinationRecord.value.error = undefined;
        }

        function setVaccinationRecord(covidVaccineRecord: CovidVaccineRecord) {
            vaccinationRecord.value.data = covidVaccineRecord;
            vaccinationRecord.value.status = LoadStatus.LOADED;
            vaccinationRecord.value.statusMessage = "";
            vaccinationRecord.value.error = undefined;
        }

        function setVaccinationRecordError(
            error: CustomBannerError | undefined
        ) {
            vaccinationRecord.value.error = error;
            vaccinationRecord.value.status = LoadStatus.ERROR;
        }

        function setVaccinationRecordStatusMessage(statusMessage: string) {
            vaccinationRecord.value.statusMessage = statusMessage;
        }

        // Helpers
        function handleError(error: ResultError) {
            logger.error(`ERROR: ${JSON.stringify(error)}`);

            if (error.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("publicVaccineCard");
                setVaccinationStatusError(undefined);
            } else {
                const customBannerError: CustomBannerError = {
                    title: "Our Apologies",
                    description:
                        "We've found an issue and the Health Gateway team is working hard to fix it.",
                };

                if (error.actionCode === ActionType.DataMismatch) {
                    customBannerError.title = "Data Mismatch";
                    customBannerError.description = error.resultMessage;
                }
                setVaccinationStatusError(customBannerError);
            }
        }

        function handlePdfError(error: ResultError) {
            logger.error(`ERROR: ${JSON.stringify(error)}`);

            if (error.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("vaccineCardComponent");
                setVaccinationRecordError(undefined);
            } else {
                const customBannerError: CustomBannerError = {
                    title: "Our Apologies",
                    description:
                        "We've found an issue and the Health Gateway team is working hard to fix it.",
                };
                setVaccinationRecordError(customBannerError);
            }
        }

        // Actions
        function retrieveVaccinationStatus(
            phn: string,
            dateOfBirth: StringISODate,
            dateOfVaccine: StringISODate
        ): Promise<void> {
            logger.debug(`Retrieving public vaccination status`);
            setVaccinationStatusRequested();
            return vaccinationStatusService
                .getPublicVaccineStatus(phn, dateOfBirth, dateOfVaccine)
                .then((result) => {
                    const payload = result.resourcePayload;
                    if (result.resultStatus === ResultType.Success) {
                        setVaccinationStatus(payload);
                    } else if (
                        result.resultError?.actionCode === ActionType.Refresh &&
                        !payload.loaded &&
                        payload.retryin > 0
                    ) {
                        logger.info("Public vaccination status not loaded");
                        setVaccinationStatusStatusMessage(
                            "Please wait a moment while we retrieve your proof of vaccination."
                        );
                        setTimeout(() => {
                            logger.info(
                                "Re-querying for public vaccination status"
                            );
                            retrieveVaccinationStatus(
                                phn,
                                dateOfBirth,
                                dateOfVaccine
                            );
                        }, payload.retryin);
                    } else {
                        throw result.resultError;
                    }
                })
                .catch((error: ResultError) => {
                    handleError(error);
                    throw error;
                });
        }

        function retrieveVaccinationRecord(
            phn: string,
            dateOfBirth: StringISODate,
            dateOfVaccine: StringISODate
        ): Promise<void> {
            logger.debug(`Retrieving public vaccination record`);
            setVaccinationRecordRequested();
            return vaccinationStatusService
                .getPublicVaccineStatusPdf(phn, dateOfBirth, dateOfVaccine)
                .then((result) => {
                    const payload = result.resourcePayload;
                    if (result.resultStatus === ResultType.Success) {
                        setVaccinationRecord(payload);
                    } else if (
                        result.resultError?.actionCode === ActionType.Refresh &&
                        !payload.loaded &&
                        payload.retryin > 0
                    ) {
                        logger.info("Public vaccination proof not loaded");
                        setVaccinationRecordStatusMessage(
                            "Please wait a moment while we download your proof of vaccination."
                        );
                        setTimeout(() => {
                            logger.info(
                                "Re-querying for public proof of vaccination"
                            );
                            retrieveVaccinationRecord(
                                phn,
                                dateOfBirth,
                                dateOfVaccine
                            );
                        }, payload.retryin);
                    } else {
                        throw result.resultError;
                    }
                })
                .catch((error: ResultError) => {
                    handlePdfError(error);
                });
        }

        return {
            vaccinationStatus,
            vaccinationStatusIsLoading,
            vaccinationStatusError,
            vaccinationStatusStatusMessage,
            isFullyVaccinated,
            isPartiallyVaccinated,
            isVaccinationNotFound,
            vaccineRecord,
            vaccineRecordIsLoading,
            vaccineRecordError,
            vaccineRecordStatusMessage,
            retrieveVaccinationStatus,
            retrieveVaccinationRecord,
        };
    }
);
