export default interface UserEmailInvite {
    // Gets or sets the user hdid.
    id: string;
    // Gets or sets the users directed identifier.
    hdId: string;
    // Gets or sets a value indicating whether the invite was validated.
    validated: boolean;
    // Gets or sets the associated email that was sent for this invite.
    emailId: string;
    // Gets or sets the invite key.
    inviteKey: string;
    // Gets or sets the email address for the invite.
    emailAddress: string;
}
