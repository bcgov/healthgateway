import { StringISODate } from "@/models/dateWrapper";

/**
 * The dependent view model.
 */
export default class Dependent {
    /**
     * The dependent hdid.
     */
    public hdid!: string;

    /**
     * The dependent masked phn.
     */
    public maskedPersonalHealthNumber!: string;

    /**
     * The dependent first name.
     */
    public firstName!: string;

    /**
     * The dependent last name.
     */
    public lastName!: string;

    /**
     * The dependent birth date.
     */
    public birthDate!: StringISODate;

    /**
     * The dependent gender.
     */
    public gender!: string;
}
