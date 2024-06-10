export const enum EnvironmentType {
    production = "production",
    development = "development",
}

export default abstract class Process {
    public static readonly NODE_ENV =
        process.env.NODE_ENV || EnvironmentType.development;
}
