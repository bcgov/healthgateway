export default interface UserProfile {
  // The user hdid.
  hdid: string;

  // Value indicating whether the user accepted the terms of service.
  acceptedTermsOfService: boolean;

  // The user email.
  email: string;
}

export interface CreateUserRequest {
  // User profile to create.
  profile: UserProfile;

  // Code used to validate if the user has an invite.
  inviteCode: string;
}
