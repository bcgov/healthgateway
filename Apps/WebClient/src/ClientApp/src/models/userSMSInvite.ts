export default interface UserSMSInvite {
  // Gets or sets a value indicating whether the invite was validated.
  validated: boolean;
  // Gets or sets the associated sms number that was sent for this invite.
  smsNumber: string;
  // Gets or sets a value indicating whether the code had too many verification attempts.
  tooManyFailedAttempts: boolean;
}
