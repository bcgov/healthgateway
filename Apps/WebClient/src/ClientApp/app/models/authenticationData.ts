import User from "@/models/user"

export default class AuthenticationData {
    public token!: string;
    public isAuthenticated!: boolean;
    public user!: User;
}