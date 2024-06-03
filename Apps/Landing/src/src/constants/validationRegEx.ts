export default abstract class ValidationRegEx {
    public static readonly PhoneNumberMasked =
        /^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$/;

    public static readonly Email =
        /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/; // nosonar
}
