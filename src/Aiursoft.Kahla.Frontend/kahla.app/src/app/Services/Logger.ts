import { Injectable } from '@angular/core';

const LOG_STYLE_BASE = 'color: white; font-weight: 500; border-radius: 4px; font-size: 1em;';
const OK_STYLE = 'background-color: #16a085;' + LOG_STYLE_BASE;
const INFO_STYLE = 'background-color: #2980b9;' + LOG_STYLE_BASE;
const WARN_STYLE = 'background-color: #f39c12;' + LOG_STYLE_BASE;
const ERROR_STYLE = 'background-color: #c0392b;' + LOG_STYLE_BASE;
const DEBUG_STYLE = 'background-color: #34495e;' + LOG_STYLE_BASE;

@Injectable({
    providedIn: 'root',
})
export class Logger {
    // This is the only way to keep the console.log context
    public debug = console.log.bind(console, '%c DBUG ', DEBUG_STYLE) as typeof console.log;
    public ok = console.info.bind(console, '%c  OK  ', OK_STYLE) as typeof console.info;
    public info = console.info.bind(console, '%c INFO ', INFO_STYLE) as typeof console.info;
    public warn = console.warn.bind(console, '%c WARN ', WARN_STYLE) as typeof console.warn;
    public error = console.error.bind(console, '%c ERR! ', ERROR_STYLE) as typeof console.error;
}

export const logger = new Logger();
