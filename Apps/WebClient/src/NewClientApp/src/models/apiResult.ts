import ApiWarning from "./apiWarning";

export default interface ApiResult<T> {
    // The request resource payload (could be empty)
    resourcePayload?: T;
    // The warning associated to the request (could be empty)
    warning?: ApiWarning;
}
