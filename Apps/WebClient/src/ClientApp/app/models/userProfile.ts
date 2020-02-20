export default interface UserProfile {
  // The user hdid.
  hdid: string;

  // Value indicating whether the user accepted the terms of service.
  acceptedTermsOfService: boolean;

  // The user email.
  email: string;

  // Flag to know if the terms of service have been updated since last login
  hasTermsOfServiceUpdated: boolean;

  // Datetime of the user's last login
  lastLoginDate: Date;
}

export interface CreateUserRequest {
  // User profile to create.
  profile: UserProfile;

  // Code used to validate if the user has an invite.
  inviteCode: string;
}
