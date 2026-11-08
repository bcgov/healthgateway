import { useMemo, useRef, useState } from "react";
import { View } from "react-native";

import { TooManyRequests } from "@/components/site/TooManyRequests";
import {
    AppText,
    Dialog,
    ExternalLink,
    HgButton,
    HgMenu,
    MessageDialog,
} from "@/components/ui";
import { EntryType } from "@/constants/entryType";
import {
    ErrorSourceType,
    ErrorType,
    TooManyRequestsLocation,
} from "@/constants/errorType";
import { SENSITIVE_DOCUMENT_MESSAGE } from "@/constants/sensitiveDocument";
import { Action, Actor, Destination, Origin, Text } from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { isSessionEnded, ResultError } from "@/models/errors";
import {
    ReportFormatType,
    reportMimeTypeMap,
    TemplateType,
} from "@/models/report";
import { ReportFilterBuilder } from "@/models/reportFilter";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { useErrorStore } from "@/stores/error.store";
import { getReportHeaderData } from "@/stores/report";
import { getDataset, getFormat } from "@/utils/eventData";
import { openExternal } from "@/utils/openExternal";
import { saveBase64File } from "@/utils/saveFile";

import {
    ImmunizationReport,
    type ImmunizationReportHandle,
} from "./ImmunizationReport";

const BOOK_VACCINATION_URL = "https://www.getvaccinated.gov.bc.ca/s/";

interface Props {
    visible: boolean;
    onClose: () => void;
    hdid: string;
    isDependent?: boolean;
}

/** Port of RecommendationsDialogComponent.vue */
export function RecommendationsDialog({
    visible,
    onClose,
    hdid,
    isDependent = false,
}: Props) {
    const { isMobile } = useBreakpoint();
    const reportFilter = useMemo(() => ReportFilterBuilder.buildEmpty(), []);
    const report = useRef<ImmunizationReportHandle>(null);
    const [isGeneratingReport, setIsGeneratingReport] = useState(false);
    const [hasRecords, setHasRecords] = useState(false);
    const [reportFormatType, setReportFormatType] = useState(
        ReportFormatType.PDF
    );
    const [confirmVisible, setConfirmVisible] = useState(false);

    const template = isDependent
        ? TemplateType.DependentImmunizationRecommendation
        : TemplateType.ImmunizationRecommendation;

    function showConfirmationModal(type: ReportFormatType) {
        setReportFormatType(type);
        setConfirmVisible(true);
    }

    async function downloadReport() {
        setIsGeneratingReport(true);
        try {
            services.tracking.trackEvent({
                action: Action.Download,
                text: Text.Export,
                dataset: getDataset(EntryType.Immunization),
                format: getFormat(reportFormatType),
                actor: isDependent ? Actor.Guardian : Actor.User,
            });
            if (!report.current) return;
            const result = await report.current.generateReport(
                reportFormatType,
                getReportHeaderData(hdid, reportFilter)
            );
            const mimeType = reportMimeTypeMap.get(reportFormatType) ?? "";
            await saveBase64File(
                result.resourcePayload.data,
                mimeType,
                result.resourcePayload.fileName
            );
        } catch (error) {
            if (isSessionEnded(error)) return;
            const resultError =
                error instanceof ResultError ? error : undefined;
            logger.error(describeError(error));
            if (resultError?.statusCode === 429)
                useErrorStore
                    .getState()
                    .setTooManyRequestsError(
                        TooManyRequestsLocation.RecommendationsDialog
                    );
            else
                useErrorStore
                    .getState()
                    .addError(
                        ErrorType.Download,
                        ErrorSourceType.ExportRecords,
                        resultError?.traceId
                    );
        } finally {
            setIsGeneratingReport(false);
        }
    }

    function visitVaccinationBooking() {
        services.tracking.trackEvent({
            action: Action.Visit,
            text: Text.ExternalLink,
            destination: Destination.BookVaccine,
            origin: Origin.ImmunizationRecommendationDialog,
        });
        void openExternal(BOOK_VACCINATION_URL);
    }

    return (
        <>
            <Dialog
                visible={visible}
                title="Recommended Immunizations"
                tone="toolbar"
                persistent
                maxWidth={1000}
                fullscreen={isMobile}
                testID="recommendations-dialog"
                closeTestID="recommendations-dialog-close-button"
                closeNativeID="recommendations-dialog-close-button"
                onRequestClose={onClose}
                actions={
                    <>
                        <HgButton
                            testID="close-recommendations-dialog-button"
                            text="Close"
                            variant="secondary"
                            onPress={onClose}
                        />
                        <HgMenu
                            testID="export-record-menu"
                            items={[
                                ReportFormatType.PDF,
                                ReportFormatType.CSV,
                                ReportFormatType.XLSX,
                            ].map((type) => ({
                                title: type,
                                onPress: () => showConfirmationModal(type),
                            }))}
                        >
                            {(open) => (
                                <HgButton
                                    testID="export-recommendations-record-button"
                                    nativeID="export-recommendations-record-button"
                                    text="Download Record"
                                    disabled={!hasRecords}
                                    loading={isGeneratingReport}
                                    onPress={open}
                                />
                            )}
                        </HgMenu>
                    </>
                }
            >
                <TooManyRequests
                    location={TooManyRequestsLocation.RecommendationsDialog}
                />
                <ImmunizationReport
                    ref={report}
                    hdid={hdid}
                    filter={reportFilter}
                    isDependent={isDependent}
                    template={template}
                    forceShow
                    hideImmunizations
                    hideRecommendationHeader
                    onIsEmptyChanged={(isEmpty) => setHasRecords(!isEmpty)}
                    recommendationsDescription={
                        isDependent ? (
                            <AppText className="mb-4">
                                School-aged children are offered most
                                immunizations in their school, particularly in
                                grades 6 and 9. The school can let you know
                                which vaccines are offered. You need to book an
                                appointment to get your child vaccinated against
                                COVID{"‑"}19.{" "}
                                <ExternalLink href="https://www2.gov.bc.ca/gov/content/covid-19/vaccine">
                                    Find out how.
                                </ExternalLink>
                            </AppText>
                        ) : (
                            <View>
                                <AppText className="mb-4">
                                    Recommended immunizations are based on the{" "}
                                    <ExternalLink href="https://www.healthlinkbc.ca/health-library/immunizations/schedules">
                                        BC Vaccine Schedule
                                    </ExternalLink>
                                    . For more information, please visit{" "}
                                    <ExternalLink href="https://www.healthlinkbc.ca/health-library/immunizations">
                                        Immunizations | HealthLink BC
                                    </ExternalLink>
                                    .
                                </AppText>
                                <HgButton
                                    testID="book-vaccination-button"
                                    className="mb-4"
                                    text="Book a COVID or Flu Vaccination"
                                    onPress={visitVaccinationBooking}
                                />
                            </View>
                        )
                    }
                />
            </Dialog>
            <MessageDialog
                visible={confirmVisible}
                title="Sensitive Document"
                message={SENSITIVE_DOCUMENT_MESSAGE}
                onClose={() => setConfirmVisible(false)}
                onSubmit={() => void downloadReport()}
            />
        </>
    );
}
