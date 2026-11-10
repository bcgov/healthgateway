/** Port of models/userNote.ts */
export interface UserNote {
    id?: string;
    hdId: string;
    title: string;
    text: string;
    /** ISO date */
    journalDate: string;
    version: number;
}
