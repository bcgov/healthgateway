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
    public maskedPHN!: string;

    /**
     * The dependent name.
     */
    public name!: string;

    /**
     * The dependent birth date.
     */
    public dateOfBirth!: StringISODate;

    /**
     * The dependent gender.
     */
    public gender!: string;
}
