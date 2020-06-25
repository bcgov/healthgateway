export default interface Communication {
    // Gets or sets the id.
    id: string;

    // Gets or sets the communication subject.
    subject: string;

  // Gets or sets the communication text.
  text: string;

  // Gets or sets the effective date time
  effectiveDateTime: Date;

  // Gets or sets the expiry date time
  expiryDateTime: Date;
}
