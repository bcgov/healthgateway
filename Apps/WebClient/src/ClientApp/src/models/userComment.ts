import { StringISODate } from "./dateWrapper";

export default interface UserComment {
    // Gets or sets the id.
    id?: string;

    // Gets or sets the user hdid.
    userProfileId: string;

    // The entry this comment belongs to.
    parentEntryId: string;

    // Gets or sets the text of the comment.
    text: string;

    // Gets or sets the comment datetime.
    createdDateTime: StringISODate;

    // Gets or sets the comment db version.
    version: number;
}
