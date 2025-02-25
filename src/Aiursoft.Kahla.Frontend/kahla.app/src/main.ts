import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/app.module';

// Magic code for mobile devices to show hover effect correctly.
// Reference: https://stackoverflow.com/questions/8330559/hover-effects-using-css3-touch-events
// eslint-disable-next-line @typescript-eslint/no-empty-function
document.body.addEventListener('touchstart', () => {}, false);

void platformBrowserDynamic().bootstrapModule(AppModule);
