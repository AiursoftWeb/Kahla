/**
 * Example input: `<{a: string, b: boolean, c: number, d: boolean}, boolean>`
 * Example output: `{b: boolean, d: boolean}`
 */
export type PropertiesWithType<T, P> = {
    [K in keyof T as T[K] extends P ? K : never]: T[K];
};

/**
 * Example input: `<{a: string, b: boolean, c: number, d: boolean}, boolean>`
 * Example output: `'b' | 'd'`
 */
export type PropertyNamesWithType<T, P> = keyof PropertiesWithType<T, P>;
