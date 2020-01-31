// A collection of configuration items for use by Health Gateway and approved applications.
export default class ExternalConfiguration {
  // Gets or sets the OpenIdConnect configuration.
  public openIdConnect!: OpenIdConnectConfiguration;

  // Gets or sets the List of Identity providers.
  public identityProviders!: IdentityProviderConfiguration[];

  // Gets or sets the Health Gateway Admin specific configuration.
  public admin!: AdminClientConfiguration;

  // Gets or sets the Service Endpoints.
  public serviceEndpoints!: { [id: string]: string };
}

// An object representing a configured Health Gateway IdentityProvider.
export class IdentityProviderConfiguration {
  // Gets or sets the Id of the Identity Provider.
  public id!: string;

  // Gets or sets the name of the Identity Provider.
  public name!: string;

  // Gets or sets the Font Awesome Icon that we recommend to use.
  public icon?: string;

  // Gets or sets the Identity Provider hint.
  public hint?: string;

  // Gets or sets a value indicating whether this identity provider should be used.
  public disabled?: boolean;
}

// Configuration to be used by external clients for authentication.
export class OpenIdConnectConfiguration {
  // Gets or sets the OpenIDConnect Authority.
  public authority!: string;
  // Gets or sets the OpenIdConnect Client ID.
  public clientId!: string;
  // Gets or sets the OpenIDConnect Response types.
  public responseType!: string;
  // Gets or sets the OpenIDConnect Scopes.
  public scope!: string;
  // Gets or sets the Callback URIs.
  public callbacks!: { [id: string]: string };
}

// Configuration data to be used by the Health Gateway Admin client.
export class AdminClientConfiguration {
  // Gets or sets the logging level used by the Admin.
  public logLevel?: string;
  // Gets or sets the Admin timeout values.
  public timeouts?: TimeOutsConfiguration;
  // Gets or sets the ExternalURLs used by the Admin.
  public externalURLs?: { [id: string]: string };
}

// Various timeout values used by the VUE Admin application.
export class TimeOutsConfiguration {
  // Gets or sets the idle time in seconds that the Admin will use before it automatically logs the user out.
  public idle!: number;
  // Gets or sets the amount of time in seconds after which the user will be redirected from the logout page back to the home.
  public logoutRedirect?: string;
}
