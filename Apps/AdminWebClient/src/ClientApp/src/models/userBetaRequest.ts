// Model that provides a user representation of a BetaRequest.
export default interface UserBetaRequest {
  // Gets or sets the beta request id.
  id: string;

  // Gets or sets the email for the beta request.
  emailAddress: string;

  // Gets or sets the version of the resource.
  version: number;

  // Gets or sets the date when the request was created.
  registrationDatetime: Date;
}
