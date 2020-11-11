export class EnvUtil {
    static getRequiredEnvVariable(name: string): string {
        const value = process.env[name];
        if (value === undefined)
            throw new Error(`Environnement variable ${name} is not specified.`);
        return value;
    }
}
