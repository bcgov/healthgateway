declare const _NODE_ENV: string;

export enum EnvironmentType
{
    production = 'production',
    development = 'development'
}

export default abstract class Process {         
    public static NODE_ENV:string = process.env.NODE_ENV || _NODE_ENV;
}