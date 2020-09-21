export default class UserComment {
    // Gets or sets the id.
    public id?: string;

    // Gets or sets the user hdid.
    public userProfileId: string = "";

    // The entry this comment belongs to.
    public parentEntryId: string = "";

    // Gets or sets the text of the comment.
    public text: string = "";

    // Gets or sets the comment datetime.
    public createdDateTime: Date = new Date();

    // Gets or sets the comment db version.
    public version: number = -1;
}
