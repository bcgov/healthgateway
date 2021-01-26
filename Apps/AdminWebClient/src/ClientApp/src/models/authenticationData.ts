import UserProfile from "@/models/userProfile";

export default interface AuthenticationData {
    token: string;
    isAuthenticated: boolean;
    isAuthorized: boolean;
    roles: string[];
    userProfile: UserProfile;
}
