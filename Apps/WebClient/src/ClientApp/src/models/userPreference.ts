export default interface UserPreference {
    // Gets or sets the user preference id.
    id?: string;
    // Gets or sets the users directed identifier.
    hdId: string;
    // Gets or sets a value indicating if the user dismissed the note notification.
    TutorialPopover: boolean;
}
