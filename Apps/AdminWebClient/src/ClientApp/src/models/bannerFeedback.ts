import { FeedbackType } from "@/constants/feedbacktype";

export default interface BannerFeedback {
    type: FeedbackType;
    title: string;
    message: string;
}
