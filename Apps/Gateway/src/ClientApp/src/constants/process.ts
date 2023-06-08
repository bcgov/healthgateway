export const enum EnvironmentType {
    production = "production",
    development = "development",
}

export default abstract class Process {
    public static NODE_ENV =
        process.env.NODE_ENV || EnvironmentType.development;
}
