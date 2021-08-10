// Model that provides a user representation of addresses.
export default interface Address {
    // Gets or sets the lines of the address.
    streetLines: string[];

    // Gets or sets the city.
    city: string;

    // Gets or sets the province or state.
    state: string;

    // Gets or sets the postal code.
    postalCode: string;

    // Gets or sets the country.
    country: string;
}
