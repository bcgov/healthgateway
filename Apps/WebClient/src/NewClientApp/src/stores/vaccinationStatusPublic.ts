import { ActionType } from "@/constants/actionType";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import { CustomBannerError, ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";
import { ILogger, IVaccinationStatusService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { defineStore } from "pinia";
import { computed, ref } from "vue";

export const useVaccinationStatusStore = defineStore(
    "vaccinationStatusPublic",
    () => {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const vaccinationStatusService =
            container.get<IVaccinationStatusService>(
                SERVICE_IDENTIFIER.VaccinationStatusService
            );

        const errorStore = useErrorStore();

        interface PublicVaccinationStatus {
            vaccinationStatus?: VaccinationStatus;
            error?: CustomBannerError;
            status: LoadStatus;
            statusMessage: string;
        }

        interface PublicVaccinationRecord {
            vaccinationRecord?: CovidVaccineRecord;
            error?: CustomBannerError;
            status: LoadStatus;
            statusMessage: string;
        }

        // Refs
        const publicVaccination = ref<PublicVaccinationStatus>({
            vaccinationStatus: undefined,
            error: undefined,
            status: LoadStatus.NONE,
            statusMessage: "",
        });

        const publicVaccinationRecord = ref<PublicVaccinationRecord>({
            vaccinationRecord: undefined,
            error: undefined,
            status: LoadStatus.NONE,
            statusMessage: "",
        });

        // Computed
        // Public Vaccination Status
        const publicVaccinationStatus = computed(
            () => publicVaccination.value.vaccinationStatus
        );

        const publicIsLoading = computed(
            () => publicVaccination.value.status === LoadStatus.REQUESTED
        );

        const publicError = computed(() => publicVaccination.value.error);

        const publicStatusMessage = computed(
            () => publicVaccination.value.statusMessage
        );

        // Public Vaccination Record
        const publicVaccineRecord = computed(
            () => publicVaccinationRecord.value.vaccinationRecord
        );

        const publicVaccineRecordIsLoading = computed(
            () => publicVaccinationRecord.value.status === LoadStatus.REQUESTED
        );

        const publicVaccineRecordError = computed(
            () => publicVaccinationRecord.value.error
        );

        const publicVaccineRecordStatusMessage = computed(
            () => publicVaccinationRecord.value.statusMessage
        );

        // Mutations
        // Public Vaccination Status
        function setPublicVaccinationRequested() {
            publicVaccination.value.error = undefined;
            publicVaccination.value.status = LoadStatus.REQUESTED;
            publicVaccination.value.statusMessage = "";
        }

        function setPublicVaccinationStatus(
            vaccinationStatus: VaccinationStatus
        ) {
            publicVaccination.value.vaccinationStatus = {
                ...vaccinationStatus,
                issueddate: new DateWrapper().toISO(),
            };
            publicVaccination.value.status = LoadStatus.LOADED;
            publicVaccination.value.statusMessage = "";
        }

        function setPublicVaccinationStatusError(
            error: CustomBannerError | undefined
        ) {
            publicVaccination.value.vaccinationStatus = undefined;
            publicVaccination.value.error = error;
            publicVaccination.value.status = LoadStatus.ERROR;
            publicVaccination.value.statusMessage = "";
        }

        function setPublicVaccinationStatusMessage(statusMessage: string) {
            publicVaccination.value.statusMessage = statusMessage;
        }

        // Public Vaccination Record
        function setPublicVaccinationRecordRequested() {
            publicVaccinationRecord.value.status = LoadStatus.REQUESTED;
            publicVaccinationRecord.value.statusMessage = "";
            publicVaccinationRecord.value.error = undefined;
        }

        function setPublicVaccinationRecord(
            covidVaccineRecord: CovidVaccineRecord
        ) {
            publicVaccinationRecord.value.vaccinationRecord =
                covidVaccineRecord;
            publicVaccinationRecord.value.status = LoadStatus.LOADED;
            publicVaccinationRecord.value.statusMessage = "";
            publicVaccinationRecord.value.error = undefined;
        }

        function setPublicVaccinationRecordError(
            error: CustomBannerError | undefined
        ) {
            publicVaccinationRecord.value.error = error;
            publicVaccinationRecord.value.status = LoadStatus.ERROR;
        }

        function setPublicVaccinationRecordStatusMessage(
            statusMessage: string
        ) {
            publicVaccinationRecord.value.statusMessage = statusMessage;
        }

        // Helpers
        function handlePublicError(error: ResultError) {
            logger.error(`ERROR: ${JSON.stringify(error)}`);

            if (error.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("publicVaccineCard");
                setPublicVaccinationStatusError(undefined);
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
                setPublicVaccinationStatusError(customBannerError);
            }
        }

        function handlePdfError(error: ResultError) {
            logger.error(`ERROR: ${JSON.stringify(error)}`);

            if (error.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("vaccineCardComponent");
                setPublicVaccinationRecordError(undefined);
            } else {
                const customBannerError: CustomBannerError = {
                    title: "Our Apologies",
                    description:
                        "We've found an issue and the Health Gateway team is working hard to fix it.",
                };
                setPublicVaccinationRecordError(customBannerError);
            }
        }

        // Actions
        function retrievePublicVaccineStatus(
            phn: string,
            dateOfBirth: StringISODate,
            dateOfVaccine: StringISODate
        ): Promise<void> {
            logger.debug(`Retrieving public vaccination status`);
            setPublicVaccinationRequested();
            return vaccinationStatusService
                .getPublicVaccineStatus(phn, dateOfBirth, dateOfVaccine)
                .then((result) => {
                    const payload = result.resourcePayload;
                    if (result.resultStatus === ResultType.Success) {
                        setPublicVaccinationStatus(payload);
                    } else if (
                        result.resultError?.actionCode === ActionType.Refresh &&
                        !payload.loaded &&
                        payload.retryin > 0
                    ) {
                        logger.info("Public vaccination status not loaded");
                        setPublicVaccinationStatusMessage(
                            "Please wait a moment while we retrieve your proof of vaccination."
                        );
                        setTimeout(() => {
                            logger.info(
                                "Re-querying for public vaccination status"
                            );
                            retrievePublicVaccineStatus(
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
                    handlePublicError(error);
                    throw error;
                });
        }

        function retrievePublicVaccineRecord(
            phn: string,
            dateOfBirth: StringISODate,
            dateOfVaccine: StringISODate
        ): Promise<void> {
            logger.debug(`Retrieving public vaccination record`);
            setPublicVaccinationRecordRequested();
            return vaccinationStatusService
                .getPublicVaccineStatusPdf(phn, dateOfBirth, dateOfVaccine)
                .then((result) => {
                    const payload = result.resourcePayload;
                    if (result.resultStatus === ResultType.Success) {
                        setPublicVaccinationRecord(payload);
                    } else if (
                        result.resultError?.actionCode === ActionType.Refresh &&
                        !payload.loaded &&
                        payload.retryin > 0
                    ) {
                        logger.info("Public vaccination proof not loaded");
                        setPublicVaccinationRecordStatusMessage(
                            "Please wait a moment while we download your proof of vaccination."
                        );
                        setTimeout(() => {
                            logger.info(
                                "Re-querying for public proof of vaccination"
                            );
                            retrievePublicVaccineRecord(
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
            publicVaccinationStatus,
            publicIsLoading,
            publicError,
            publicStatusMessage,
            publicVaccineRecord,
            publicVaccineRecordIsLoading,
            publicVaccineRecordError,
            publicVaccineRecordStatusMessage,
            retrievePublicVaccineStatus,
            retrievePublicVaccineRecord,
        };
    }
);
