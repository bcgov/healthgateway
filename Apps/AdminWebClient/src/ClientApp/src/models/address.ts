// Model that provides a user representation of addresses.
export default class Address {
    // Gets or sets the lines of the address.
    public streetLines!: string[];

    // Gets or sets the city.
    public city!: string;

    // Gets or sets the province or state.
    public state!: string;

    // Gets or sets the postal code.
    public postalCode!: string;

    // Gets or sets the country.
    public country!: string;
}
