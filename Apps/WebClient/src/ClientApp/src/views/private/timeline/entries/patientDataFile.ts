import type { PatientDataFile } from "@/models/patientData";
import { bytesToBase64 } from "@/utils/bytes";
import { saveBase64File } from "@/utils/saveFile";

/** `new Blob([new Uint8Array(file.content)], { type })` + saveAs, through the shared file save */
export const savePatientDataFile = (file: PatientDataFile, fileName: string) =>
    saveBase64File(bytesToBase64(file.content), file.contentType, fileName);
