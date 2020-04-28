// Model that provides a user representation of admin communications.
export default interface Communication {
  // Gets or sets the id.
  id?: string;

  // Gets or sets the subject.
  subject: string;

  // Gets or sets the text.
  text: string;

  // Gets or sets the effective date.
  effectiveDateTime: Date;

  // Gets or sets the expiry date.
  expiryDateTime: Date;
}
