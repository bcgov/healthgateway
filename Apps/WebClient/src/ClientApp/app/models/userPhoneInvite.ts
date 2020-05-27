export default interface UserPhoneInvite {
  // Gets or sets a value indicating whether the invite was validated.
  validated: boolean;
  // Gets or sets the associated phone number that was sent for this invite.
  phoneNumber: string;
}
