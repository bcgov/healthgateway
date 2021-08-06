// Model that provides a user representation of addresses.
export default interface Address {
    // Gets or sets the address.
    address: string;

    // Gets or sets the second address line.
    address2?: string;

    // Gets or sets the city.
    city: string;

    // Gets or sets the province.
    province: string;

    // Gets or sets the country.
    country: string;

    // Gets or sets the postal code.
    postalCode: string;
}
