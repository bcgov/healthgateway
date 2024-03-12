import { ClientType } from "@/constants/clientType";

export default interface UserFeedback {
    comment: string;
    clientType?: ClientType;
}
