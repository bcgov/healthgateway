declare const _NODE_ENV: string;

export enum EnvironmentType
{
    production = 'production',
    development = 'development'
}

export default abstract class Process {         
    public static NODE_ENV:string = _NODE_ENV;
}