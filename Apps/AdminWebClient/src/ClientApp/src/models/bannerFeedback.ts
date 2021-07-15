import { ResultType } from "@/constants/resulttype";

export default interface BannerFeedback {
    type: ResultType;
    title: string;
    message: string;
}
