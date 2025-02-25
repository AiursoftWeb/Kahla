export function pickProperties<T, K extends keyof T>(object: T, ...propertyNames: K[]): Pick<T, K> {
    const result: Partial<T> = {};
    for (const key of propertyNames) {
        result[key] = object[key];
    }
    return result as Pick<T, K>;
}
