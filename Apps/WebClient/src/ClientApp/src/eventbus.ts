import Vue from "vue";

const EventBus = new Vue();
export default EventBus;

export class EventMessageName {
    public static readonly CreateNote = "note-create";
    public static readonly EditNote = "note-edit";
    public static readonly ViewEntryDetails = "view-entry-details";
    public static readonly RegisterBeforeUnloadWaitlistListener =
        "waitlist-register-on-before-unload-listener";
    public static readonly UnregisterBeforeUnloadWaitlistListener =
        "waitlist-unregister-on-before-unload-listener";
}
